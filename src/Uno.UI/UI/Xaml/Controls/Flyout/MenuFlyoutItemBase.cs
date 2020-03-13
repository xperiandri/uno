using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Input;

namespace Windows.UI.Xaml.Controls
{
	public abstract partial class MenuFlyoutItemBase : Control
	{
		private WeakReference m_wrParentMenuFlyoutPresenter;

		public MenuFlyoutItemBase()
		{

		}

		// Get the parent MenuFlyoutPresenter.
		private protected MenuFlyoutPresenter GetParentMenuFlyoutPresenter()
			=> m_wrParentMenuFlyoutPresenter.Target as MenuFlyoutPresenter;

		// Sets the parent MenuFlyoutPresenter.
		private protected void SetParentMenuFlyoutPresenter(MenuFlyoutPresenter pParentMenuFlyoutPresenter)
			=> m_wrParentMenuFlyoutPresenter.Target = pParentMenuFlyoutPresenter;

		private protected bool GetShouldBeNarrow()
		{
			MenuFlyoutPresenter spPresenter = GetParentMenuFlyoutPresenter();

			var shouldBeNarrow = false;

			if (spPresenter != null)
			{
				MenuFlyout spParentFlyout;

				spParentFlyout = spPresenter.GetParentMenuFlyout();

				if (spParentFlyout != null)
				{
					shouldBeNarrow =
						(spParentFlyout.InputDeviceTypeUsedToOpen == FocusInputDeviceKind.Mouse) ||
						(spParentFlyout.InputDeviceTypeUsedToOpen == FocusInputDeviceKind.Pen) ||
						(spParentFlyout.InputDeviceTypeUsedToOpen == FocusInputDeviceKind.Keyboard);
				}
			}

			return shouldBeNarrow;
		}

	}
}
