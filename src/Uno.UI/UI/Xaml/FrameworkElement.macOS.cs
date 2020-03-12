using CoreGraphics;
using Uno.Extensions;
using Uno.UI.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Windows.Foundation;
using Uno.Logging;
using Uno.UI;
using AppKit;

namespace Windows.UI.Xaml
{
	public partial class FrameworkElement
	{
		private CGSize? _lastAvailableSize;
		private CGSize _lastMeasure;

		partial void Initialize();

		public FrameworkElement()
		{
			Initialize();

			RequiresMeasure = true;
			RequiresArrange = true;

			this.RegisterParentChangedCallback(null, OnParentChanged);
		}


		protected internal override void OnInvalidateMeasure()
		{
			base.OnInvalidateMeasure();

			RequiresMeasure = true;
			RequiresArrange = true;

			InvalidateParentMeasure();
		}

		private void OnParentChanged(object instance, object key, DependencyObjectParentChangedEventArgs args)
		{
			InvalidateParentMeasure();
		}

		private void InvalidateParentMeasure()
		{
			var parent = this.GetParent();

			while (parent != null && !(parent is FrameworkElement))
			{
				if (parent is NSView nativeView)
				{
					nativeView.NeedsLayout = true;
					parent = nativeView.Superview;
				}
				else
				{
					parent = null;
				}
			}

			if (parent is FrameworkElement parentFe)
			{
				if (!parentFe.RequiresMeasure)
				{
					parentFe.InvalidateMeasure();
				}
			}
		}

		public override void Layout()
		{
			try
			{
				try
				{
					// _inLayoutSubviews = true;
					var originalRequiresArrange = RequiresArrange;
					var originalRequiresMeasure = RequiresMeasure;

					if (RequiresMeasure)
					{
						XamlMeasure(Bounds.Size);
					}

					if (RequiresArrange)
					{
						OnBeforeArrange();

						var size = SizeFromUISize(Bounds.Size);
						_layouter.Arrange(new Rect(0, 0, size.Width, size.Height));

						OnAfterArrange();
					}
					else
					{
						RequiresArrange = false;
					}
				}
				finally
				{
					//_inLayoutSubviews = false;
					RequiresArrange = false;
				}
			}
			catch(Exception e)
			{
				this.Log().Error($"Layout failed in {GetType()}", e);
			}
		}
		/// <summary>
		/// Called before Arrange is called, this method will be deprecated
		/// once OnMeasure/OnArrange will be implemented completely
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected virtual void OnBeforeArrange()
		{

		}

		/// <summary>
		/// Called after Arrange is called, this method will be deprecated
		/// once OnMeasure/OnArrange will be implemented completely
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected virtual void OnAfterArrange()
		{

		}

		internal CGSize? XamlMeasure(CGSize availableSize)
		{
			// If set layout has not been called, we can 
			// return a previously cached result for the same available size.
			if (
				!RequiresMeasure
				&& _lastAvailableSize.HasValue
				&& availableSize == _lastAvailableSize
			)
			{
				return _lastMeasure;
			}

			_lastAvailableSize = availableSize;
			RequiresMeasure = false;

			var result = _layouter.Measure(SizeFromUISize(availableSize));

			return result.LogicalToPhysicalPixels();
		}

		public CGSize SizeThatFits(CGSize size)
		{
			try
			{
				//_inLayoutSubviews = true;

				var xamlMeasure = XamlMeasure(size);

				if(xamlMeasure != null)
				{
					return _lastMeasure = xamlMeasure.Value;
				}
				else
				{
					return _lastMeasure = CGSize.Empty;
				}
			}
			finally
			{
				//_inLayoutSubviews = false;
			}
		}

		protected Size SizeFromUISize(CGSize size)
		{
			var width = nfloat.IsNaN(size.Width) ? float.PositiveInfinity : size.Width;
			var height = nfloat.IsNaN(size.Height) ? float.PositiveInfinity : size.Height;

			return new Size(width, height).PhysicalToLogicalPixels();
		}

		private bool IsTopLevelXamlView()
		{
			NSView parent = this;
			while (parent != null)
			{
				parent = parent.Superview;
				if (parent is IFrameworkElement)
				{
					return false;
				}
			}
			return true;
		}
	}
}
