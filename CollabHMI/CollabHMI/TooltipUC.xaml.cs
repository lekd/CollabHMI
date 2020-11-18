using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CollabHMI
{
    /// <summary>
    /// Interaction logic for TooltipUC.xaml
    /// </summary>
    public partial class TooltipUC : UserControl
    {
        Timer tooltipContentScheduler;
        TooltipController tooltipController;
        public TooltipUC()
        {
            InitializeComponent();
            tooltipContentScheduler = new Timer();
            tooltipContentScheduler.Interval = 50;
            tooltipContentScheduler.Elapsed += TooltipContentScheduler_Elapsed;
            tooltipContentScheduler.Start();

            tooltipController = new TooltipController(800,400);
        }

        private void TooltipContentScheduler_Elapsed(object sender, ElapsedEventArgs e)
        {
            Bitmap tooltipContent = tooltipController.ExtractTooltipBitmap();
            Action displayTooltipContent = delegate
            {
                imgTooltipContent.Source = Utilities.ToBitmapImage(tooltipContent, ImageFormat.Png);
            };
            imgTooltipContent.Dispatcher.Invoke(displayTooltipContent);
        }
        public void UpdateHMIScreenBmp(Bitmap hmiScreenshotBmp)
        {
            tooltipController.UpdateCurrentScreenBmp(hmiScreenshotBmp);
        }
        public void UpdateRemoteFocusAndCursor(RectangleF relFocusArea,PointF relCursorPos)
        {
            tooltipController.UpdateCursorPos(relCursorPos.X, relCursorPos.Y);
            tooltipController.UpdateRelFocusArea(relFocusArea.Left, relFocusArea.Top, relFocusArea.Right, relFocusArea.Bottom);
        }
        public void StopUpdatingTooltipContent()
        {
            tooltipContentScheduler.Stop();
        }
    }
}
