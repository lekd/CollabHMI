using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabHMI
{
    public class HMIScreenController
    {
        Bitmap background;
        PointF remoteCursorRelPos = new PointF();
        Bitmap remoteCursorBmp;
        public HMIScreenController()
        {
            background = new Bitmap(System.Environment.CurrentDirectory + "\\Resources\\HMIBackground.png");
            remoteCursorBmp = new Bitmap(System.Environment.CurrentDirectory + "\\Resources\\BlackCursor.png");
            remoteCursorBmp = Utilities.ResizeBitmap(remoteCursorBmp, new Size((int)(background.Width * 0.025), (int)(background.Width * 0.025)));
        }
        public void UpdateRemoteCursorRelPos(double cursorRelX,double cursorRelY)
        {
            remoteCursorRelPos.X = (float)cursorRelX;
            remoteCursorRelPos.Y = (float)cursorRelY;
        }
        public Bitmap getCurrentHMISceenshot()
        {
            Bitmap screenshotBmp = new Bitmap(background);
            using (Graphics g = Graphics.FromImage(screenshotBmp))
            {
                Point remoteCursorAbsPos = convertRelPosToAbsPos(remoteCursorRelPos);
                g.DrawImage(remoteCursorBmp, remoteCursorAbsPos);
            }
            return screenshotBmp;
        }
        Point convertRelPosToAbsPos(PointF relPos)
        {
            Point absPoint = new Point();
            absPoint.X = (int)(relPos.X * background.Width);
            absPoint.Y = (int)(relPos.Y * background.Height);
            return absPoint;
        }
    }
}
