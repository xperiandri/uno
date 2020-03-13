using System;
using Windows.UI.Xaml.Controls.Primitives;

namespace Windows.UI.Xaml.Controls
{
	public  partial class MenuFlyoutPresenter : global::Windows.UI.Xaml.Controls.ItemsControl
	{
		public global::Windows.UI.Xaml.Controls.Primitives.MenuFlyoutPresenterTemplateSettings TemplateSettings { get; } = new MenuFlyoutPresenterTemplateSettings();

		public MenuFlyoutPresenter() : base()
		{
		}

		internal bool m_isSubPresenter;
		internal ISubMenuOwner Owner { get; internal set; }
		internal IMenu OwningMenu { get; set; }
		internal IMenuPresenter SubPresenter { get; set; }
		public bool IsSubPresenter { get; internal set; }

		internal bool GetContainsToggleItems()
		{
			throw new NotImplementedException();
		}

		internal bool GetContainsIconItems()
		{
			throw new NotImplementedException();
		}

		internal int GetDepth()
		{
			throw new NotImplementedException();
		}

		internal MenuFlyout GetParentMenuFlyout()
		{
			throw new NotImplementedException();
		}

		internal void SetDepth(int v)
		{
			throw new NotImplementedException();
		}
	}
}
