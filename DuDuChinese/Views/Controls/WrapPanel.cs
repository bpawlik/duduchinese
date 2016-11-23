using DuDuChinese.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DuDuChinese.Views.Controls
{
    public class WrapPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            // Just take up all of the width
            Size finalSize = availableSize;

            try
            {
                finalSize = new Size() { Width = availableSize.Width };

                double x = 0;
                double rowHeight = 0d;
                foreach (var child in Children)
                {
                    // Tell the child control to determine the size needed
                    child.Measure(availableSize);

                    x += child.DesiredSize.Width;
                    if (x > availableSize.Width)
                    {
                        // this item will start the next row
                        x = child.DesiredSize.Width;

                        // adjust the height of the panel
                        finalSize.Height += rowHeight;
                        rowHeight = child.DesiredSize.Height;
                    }
                    else
                    {
                        // Get the tallest item
                        rowHeight = Math.Max(child.DesiredSize.Height, rowHeight);
                    }
                }

                // Add the final height
                finalSize.Height += rowHeight;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("error in MeasureOverride: " + ex.Message);
            }

            if (finalSize.Width >= int.MaxValue || finalSize.Height >= int.MaxValue)
                return new Size();
            else
                return finalSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            try
            {
                Rect finalRect = new Rect(0, 0, finalSize.Width, finalSize.Height);

                double rowHeight = 0;
                foreach (var child in Children)
                {
                    double width = child.DesiredSize.Width;
                    if ((child as ListViewItem).Content is TextBlock)
                    {
                        // A bit of hack used to set properly child width.
                        int length = ((child as ListViewItem).Content as TextBlock).Text.Length;
                        if (LearningEngine.CurrentExercise == LearningExercise.FillGapsChinese)
                        {
                            // This works for chinese characters font size 24 only!
                            switch (length)
                            {
                                case 0:
                                    width = 0;
                                    break;
                                case 1:
                                    width = 28;
                                    break;
                                case 2:
                                    width = 54;
                                    break;
                                case 3:
                                    width = 76;
                                    break;
                            }
                        }
                        else
                        {
                            // This works for english characters Courier font size 24 only!
                            width = length * 14 + 8;
                        }
                    }

                    if ((width + finalRect.X) > finalSize.Width)
                    {
                        // next row!
                        finalRect.X = 0;
                        finalRect.Y += rowHeight;
                        rowHeight = 0;
                    }

                    // Place the item
                    child.Arrange(new Rect(finalRect.X, finalRect.Y, width, child.DesiredSize.Height));

                    // adjust the location for the next items
                    finalRect.X += width;
                    rowHeight = Math.Max(child.DesiredSize.Height, rowHeight);
                }
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("error in MeasureOverride: " + ex.Message);
            }

            return finalSize;
        }
    }
}
