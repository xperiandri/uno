﻿#if __ANDROID__ || __IOS__
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.UI.Controls
{
	/// <summary>
	/// An ItemsStackPanel implementation which doesn't rely on high-level native list controls.
	/// </summary>
	/// <remarks>For now this panel mainly exists for testing purposes, to be able to debug the WASM/MacOS implementation on Android or iOS.</remarks>
	public partial class ManagedItemsStackPanel : Panel
	{
		ManagedVirtualizingPanelLayout _layout;

		internal bool ShouldInterceptInvalidate { get; set; }

		public ManagedItemsStackPanel()
		{
			CreateLayoutIfNeeded();
			_layout.Initialize(this);
		}

		private ManagedVirtualizingPanelLayout GetLayouter()
		{
			CreateLayoutIfNeeded();
			return _layout;
		}

		private void CreateLayoutIfNeeded()
		{
			if (_layout == null)
			{
				_layout = new ManagedItemsStackPanelLayout();
				_layout.BindToEquivalentProperty(this, nameof(Orientation));
				//				_layout.BindToEquivalentProperty(this, nameof(AreStickyGroupHeadersEnabled));
				//				_layout.BindToEquivalentProperty(this, nameof(GroupHeaderPlacement));
				//				_layout.BindToEquivalentProperty(this, nameof(GroupPadding));
				//#if !XAMARIN_IOS
				//				_layout.BindToEquivalentProperty(this, nameof(CacheLength));
				//#endif
			}
		}

		#region Orientation DependencyProperty

		public Orientation Orientation
		{
			get { return (Orientation)this.GetValue(OrientationProperty); }
			set { this.SetValue(OrientationProperty, value); }
		}

		public static readonly DependencyProperty OrientationProperty =
			DependencyProperty.Register(
				"Orientation",
				typeof(Orientation),
				typeof(ManagedItemsStackPanel),
				new FrameworkPropertyMetadata(
					defaultValue: (Orientation)Orientation.Vertical,
					options: FrameworkPropertyMetadataOptions.None,
					propertyChangedCallback: (s, e) => ((ManagedItemsStackPanel)s)?.OnOrientationChanged((Orientation)e.OldValue, (Orientation)e.NewValue)
				)
			);

		protected virtual void OnOrientationChanged(Orientation oldOrientation, Orientation newOrientation)
		{
			OnOrientationChangedPartial(oldOrientation, newOrientation);
			OnOrientationChangedPartialNative(oldOrientation, newOrientation);
		}

		partial void OnOrientationChangedPartial(Orientation oldOrientation, Orientation newOrientation);
		partial void OnOrientationChangedPartialNative(Orientation oldOrientation, Orientation newOrientation);

		#endregion
#if __IOS__
		public override void SetSuperviewNeedsLayout()
		{
			if (ShouldInterceptInvalidate)
			{
				return;
			}

			base.SetSuperviewNeedsLayout();
		}
#elif __ANDROID__
		protected override bool NativeRequestLayout()
		{
			if (ShouldInterceptInvalidate)
			{
				ForceLayout();
				return false;
			}
			else
			{
				return base.NativeRequestLayout();
			}
		}
#endif
	}
}

#endif
