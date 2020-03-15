﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Uno.Extensions;
#if __ANDROID__ || __IOS__
using _VirtualizingPanelLayout = Windows.UI.Xaml.Controls.ManagedVirtualizingPanelLayout;
#else
using _VirtualizingPanelLayout = Windows.UI.Xaml.Controls.VirtualizingPanelLayout;
#endif

namespace Windows.UI.Xaml.Controls
{
	/// <summary>
	/// Handles creation, recycling and binding of items for an <see cref="IVirtualizingPanel"/>.
	/// </summary>
	internal class VirtualizingPanelGenerator
	{
		private const int CacheLimit = 10;
		private const int NoTemplateItemId = -1;
		private readonly _VirtualizingPanelLayout _owner;
		/// <summary>
		/// Recycled item containers that can be reused as needed. This is actually multiple caches indexed by template id.
		/// </summary>
		private readonly Dictionary<int, Stack<FrameworkElement>> _itemContainerCache = new Dictionary<int, Stack<FrameworkElement>>();
		/// <summary>
		/// Caching the id is more efficient, and also important in the case of the ItemsSource changing, when the (former) item may no longer be in the new collection.
		/// </summary>
		private Dictionary<int, int> _idCache = new Dictionary<int, int>();

		/// <summary>
		/// Items that have been temporarily scrapped and can be reused without being rebound.
		/// </summary>
		private readonly Dictionary<int, FrameworkElement> _scrapCache = new Dictionary<int, FrameworkElement>();

		private ItemsControl ItemsControl => _owner.ItemsControl;

		public VirtualizingPanelGenerator(_VirtualizingPanelLayout owner)
		{
			_owner = owner;
		}

		/// <summary>
		/// Returns a container view bound to the item at <paramref name="index"/>, recycling an available view if possible, or creating a new container if not.
		/// </summary>
		public FrameworkElement DequeueViewForItem(int index)
		{
			//Try scrap first to save rebinding view
			var scrapped = TryGetScrappedContainer(index);
			if (scrapped != null)
			{
				return scrapped;
			}

			var id = GetItemId(index);

			var container = TryDequeueCachedContainer(id);
			if (this.Log().IsEnabled(LogLevel.Debug))
			{
				this.Log().LogDebug($"{GetMethodTag()} item={index} container={container} PreviousDC={container?.DataContext}");
			}
			if (container == null)
			{
				container = ItemsControl.GetContainerForIndex(index) as FrameworkElement;
			}

			ItemsControl.PrepareContainerForIndex(container, index);

			return container;
		}

		/// <summary>
		/// Try to retrieve a cached container for a given template <paramref name="id"/>. Returns null if none is available.
		/// </summary>
		private FrameworkElement TryDequeueCachedContainer(int id)
		{
			if (_itemContainerCache.TryGetValue(id, out var cache))
			{
				if (cache.Count > 0)
				{
					var cachedView = cache.Pop();
					cachedView.Visibility = Visibility.Visible;
					return cachedView;
				}
			}

			return null;
		}

		/// <summary>
		/// Try to retrieve a temporarily-scrapped container for given item <paramref name="index"/>. Returns null if none is available.
		/// </summary>
		private FrameworkElement TryGetScrappedContainer(int index)
		{
			if (_scrapCache.TryGetValue(index, out var container))
			{
				_scrapCache.Remove(index);

				return container;
			}

			return null;
		}

		/// <summary>
		/// Return a container to the reuse pool once it has finished being displayed.
		/// </summary>
		/// <param name="container">The item container to recycle.</param>
		/// <param name="index">The item index that the container was being used for.</param>
		public void RecycleViewForItem(FrameworkElement container, int index)
		{
			var id = GetItemId(index);

			if (!(_owner.XamlParent?.IsIndexItsOwnContainer(index) ?? false))
			{
				if (this.Log().IsEnabled(LogLevel.Debug))
				{
					this.Log().LogDebug($"{GetMethodTag()} container={container} index={index}");
				}

				var cache = _itemContainerCache.FindOrCreate(id, () => new Stack<FrameworkElement>());

				if (cache.Count < CacheLimit)
				{
					cache.Push(container);
				}
				else
				{
					// Cache is full, remove the view
					DiscardContainer(container);
				}
			}
			else
			{
				// Non-generated containers cannot be recycled as they
				// are placed at specific positions.

				if (this.Log().IsEnabled(LogLevel.Debug))
				{
					this.Log().LogDebug($"{GetMethodTag()} itemIsItsOwnContainer container={container} index={index}");
				}

				DiscardContainer(container);
			}
		}

		/// <summary>
		/// Completely remove an item container that will not be reused again.
		/// </summary>
		private static void DiscardContainer(FrameworkElement container)
		{
			var parent = (container.Parent) as Panel;
			if (parent != null)
			{
				parent.Children.Remove(container);
			}
		}

		/// <summary>
		/// Send view to temporary scrap. Intended for use during lightweight layout rebuild. Views in scrap will be reused without being rebound if a view for that position is requested.
		/// </summary>
		/// <param name="container">The item container to scrap</param>
		/// <param name="index">The item index that the container is currently bound to</param>
		public void ScrapViewForItem(FrameworkElement container, int index)
		{
			_scrapCache[index] = container;
		}

		/// <summary>
		/// Empty scrap, this should be called after a lightweight layout rebuild.
		/// </summary>
		public void ClearScrappedViews()
		{
			foreach (var kvp in _scrapCache)
			{
				RecycleViewForItem(kvp.Value, kvp.Key);
			}

			_scrapCache.Clear();
		}

		/// <summary>
		/// Hide cached views that are no longer displaying materialized items. Doing this in a single batch, rather than repeatedly toggling
		/// the visibility as items are recycled and then reused, significantly improves scrolling performance in WebAssembly.
		/// </summary>
		public void UpdateVisibilities()
		{
			foreach (var cache in _itemContainerCache)
			{
				foreach (var view in cache.Value)
				{
					// This is a crude means of 'hiding' the view. We prefer not to unload it because recycling is cheaper if it stays in
					// the visual tree, but we should probably go to greater effort to conceal it from, eg, traversals of the visual tree.
					view.Visibility = Visibility.Collapsed;
				}
			}
		}

		/// <summary>
		/// Get the container reuse ID associated with the item <paramref name="index"/>, based on the template associated with it (eg if <see cref="ItemsControl.ItemTemplateSelector"/> is set)
		/// </summary>
		private int GetItemId(int index)
		{	
			if(_idCache.TryGetValue(index, out var value))
			{
				return value;
			}
			var item = ItemsControl?.GetItemFromIndex(index);
			var template = ItemsControl?.ResolveItemTemplate(item);
			var id = template?.GetHashCode() ?? NoTemplateItemId;
			_idCache.Add(index, id);

			return id;
		}

		private string GetMethodTag([CallerMemberName] string caller = null)
			=> $"{nameof(VirtualizingPanelGenerator)}.{caller}()";

		internal void ClearIdCache()
		{
			_idCache.Clear();
		}
	}
}
