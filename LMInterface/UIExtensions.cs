using System;
using Windows.UI;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;

namespace LMInterface {

    public static class UIExtensions {

        public static bool StringToColor(string strColor, out Color color) {
            string xaml = string.Format("<Color xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">{0}</Color>", strColor);
            try {
                object obj = XamlReader.Load(xaml);
                if (obj != null && obj is Color) {
                    color = (Color)obj;
                    return true;
                }
            } catch (Exception) {
                //Swallow useless exception
            }

            color = new Color();
            return false;
        }

        /// <summary>
        /// Will scroll the ListView to the last item.
        /// </summary>
        public static void ScrollToLastItem(this ListView lv) {
            lv.UpdateLayout();
            lv.SmoothScrollIntoViewWithIndexAsync(lv.Items.Count - 1, ScrollItemPlacement.Top, false, false);
        }

        /// <summary>
        /// Checks if the ListView is scrolled up.
        /// </summary>
        public static bool IsScrolledUp(this ListView lv, double allowedDeviation) {
            Border? border = VisualTreeHelper.GetChild(lv, 0) as Border;
            ScrollViewer? scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
            return scrollViewer?.VerticalOffset < scrollViewer?.ScrollableHeight - allowedDeviation;
        }
    }

}
