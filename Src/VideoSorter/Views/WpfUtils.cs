﻿namespace VideoSorter.Views;

public static class WpfUtils
{
  public static Window FindParentWindow(FrameworkElement element)
  {
    if (element.Parent == null) return null;

    if (element.Parent as Window != null) return element.Parent as Window;

    if (element.Parent as FrameworkElement != null) return FindParentWindow(element.Parent as FrameworkElement);

    return null;
  }
}
