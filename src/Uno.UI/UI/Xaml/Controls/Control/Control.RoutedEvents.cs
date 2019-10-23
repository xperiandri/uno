using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Uno.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace Windows.UI.Xaml.Controls
{
	partial class Control
	{
		private static readonly Dictionary<Type, RoutedEventFlag> ImplementedRoutedEvents
			= new Dictionary<Type, RoutedEventFlag>();

		private static readonly List<(string name, RoutedEventFlag flag, Type[] args)> _routedEventsMap
			= new List<(string name, RoutedEventFlag flag, Type[] args)> {

			(nameof(OnPointerPressed), RoutedEventFlag.PointerPressed, new[]{ typeof(Windows.UI.Xaml.Input.PointerRoutedEventArgs) }),
			(nameof(OnPointerReleased), RoutedEventFlag.PointerReleased, new[]{ typeof(Windows.UI.Xaml.Input.PointerRoutedEventArgs) }),
			(nameof(OnPointerEntered), RoutedEventFlag.PointerEntered, new[]{ typeof(Windows.UI.Xaml.Input.PointerRoutedEventArgs) }),
			(nameof(OnPointerExited), RoutedEventFlag.PointerExited, new[]{ typeof(Windows.UI.Xaml.Input.PointerRoutedEventArgs) }),
			(nameof(OnPointerMoved), RoutedEventFlag.PointerMoved, new[]{ typeof(Windows.UI.Xaml.Input.PointerRoutedEventArgs) }),
			(nameof(OnPointerCanceled), RoutedEventFlag.PointerCanceled, new[]{ typeof(Windows.UI.Xaml.Input.PointerRoutedEventArgs) }),
			(nameof(OnPointerCaptureLost), RoutedEventFlag.PointerCaptureLost, new[]{ typeof(Windows.UI.Xaml.Input.PointerRoutedEventArgs) }),
			(nameof(OnManipulationStarting), RoutedEventFlag.ManipulationStarting, new[]{ typeof(Windows.UI.Xaml.Input.ManipulationStartingRoutedEventArgs) }),
			(nameof(OnManipulationStarted), RoutedEventFlag.ManipulationStarted, new[]{ typeof(Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs) }),
			(nameof(OnManipulationDelta), RoutedEventFlag.ManipulationDelta, new[]{ typeof(Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs) }),
			(nameof(OnManipulationInertiaStarting), RoutedEventFlag.ManipulationInertiaStarting, new[]{ typeof(Windows.UI.Xaml.Input.ManipulationInertiaStartingRoutedEventArgs) }),
			(nameof(OnManipulationCompleted), RoutedEventFlag.ManipulationCompleted, new[]{ typeof(Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs) }),
			(nameof(OnTapped), RoutedEventFlag.Tapped, new[]{ typeof(Windows.UI.Xaml.Input.TappedRoutedEventArgs) }),
			(nameof(OnDoubleTapped), RoutedEventFlag.DoubleTapped, new[]{ typeof(Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs) }),
			(nameof(OnKeyDown), RoutedEventFlag.KeyDown, new[]{ typeof(Windows.UI.Xaml.Input.KeyRoutedEventArgs) }),
			(nameof(OnKeyUp), RoutedEventFlag.KeyUp, new[]{ typeof(Windows.UI.Xaml.Input.KeyRoutedEventArgs) }),
			(nameof(OnLostFocus), RoutedEventFlag.LostFocus, new[]{ typeof(Windows.UI.Xaml.RoutedEventArgs) }),
			(nameof(OnGotFocus), RoutedEventFlag.GotFocus, new[]{ typeof(Windows.UI.Xaml.RoutedEventArgs) }),
		};

		internal static RoutedEventFlag GetImplementedRoutedEvents(Type type)
		{
			ValidateRoutedEventStaticTable();

			// TODO: GetImplementedRoutedEvents() should be evaluated at compile-time
			// and the result placed in a partial file.

			if (ImplementedRoutedEvents.TryGetValue(type, out var result))
			{
				return result;
			}

			result = RoutedEventFlag.None;

			var baseClass = type.BaseType;
			if (baseClass == null || type == typeof(Control) || type == typeof(UIElement))
			{
				return result;
			}

			foreach (var evt in _routedEventsMap)
			{
				result |= GetIsEventOverrideImplemented(type, evt.name, evt.args) ? evt.flag : RoutedEventFlag.None;
			}

			return ImplementedRoutedEvents[type] = result;
		}

		[Conditional("Debug")]
		private static void ValidateRoutedEventStaticTable()
		{
			if (ImplementedRoutedEvents.Count == 0)
			{
				foreach (var evt in _routedEventsMap)
				{
					var args2 = typeof(Control)
						.GetMethod(
							evt.name,
							BindingFlags.NonPublic | BindingFlags.Instance)?.GetParameters()?.Select(p => p.ParameterType);

					if (!(args2?.SequenceEqual(evt.args) ?? false))
					{
						throw new InvalidOperationException($"The routed event static registration table is invalid for event {evt.name}");
					}
				}
			}
		}

		private static bool GetIsEventOverrideImplemented(Type type, string name, Type[] args)
		{
			var method = type
				.GetMethod(
					name,
					BindingFlags.NonPublic | BindingFlags.Instance,
					null,
					args,
					null);

			return method != null
				&& method.IsVirtual
				&& method.DeclaringType != typeof(Control);
		}
	}
}
