using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabHMI
{
    public class MiniMapController
    {
        SolidBrush borderBrush;
        SolidBrush bkgBrush;
        SolidBrush focusAreaBrush;
        Pen borderPen;

        int miniMapWidth;
        int miniMapHeight;
        RectangleF localUserFocusArea = new RectangleF();
        PointF localRelCursorPos = new Point();
        PointF remoteRelCursorPos = new Point();

        Bitmap minimapBkg;
        Bitmap localUserCursor;
        Bitmap remoteUserCursor;
        public MiniMapController(int w,int h)
        {
            localUserFocusArea = new RectangleF(0, 0, 1.0f, 1.0f);
            miniMapWidth = w;
            miniMapHeight = h;
            borderBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
            bkgBrush = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
            focusAreaBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
            borderPen = new Pen(borderBrush, 3);
            minimapBkg = new Bitmap(w, h);
            using (var g = Graphics.FromImage(minimapBkg))
            {
                //draw background and border
                g.FillRectangle(bkgBrush, new Rectangle(0, 0, w, h));
                g.DrawRectangle(borderPen, new Rectangle(0, 0, w, h));
            }
            localUserCursor = new Bitmap(System.Environment.CurrentDirectory + "\\Resources\\WhiteCursor.png");
            localUserCursor = Utilities.ResizeBitmap(localUserCursor, new Size(30, 30));
            remoteUserCursor = new Bitmap(System.Environment.CurrentDirectory + "\\Resources\\BlackCursor.png");
            remoteUserCursor = Utilities.ResizeBitmap(remoteUserCursor, new Size(30, 30));
        }
        public void updateLocalUserFocusArea(float top, float left, float bottom, float right)
        {
            localUserFocusArea.X = left;
            localUserFocusArea.Y = right;
            localUserFocusArea.Width = right - left;
            localUserFocusArea.Height = bottom - top;
        }
        public void UpdateLocalCursorPos(double localX,double localY)
        {
            localRelCursorPos.X = (float)localX;
            localRelCursorPos.Y = (float)localY;
        }
        public void UpdateRemoteCursorPos(double localX, double localY)
        {
            remoteRelCursorPos.X = (float)localX;
            remoteRelCursorPos.Y = (float)localY;
        }
        public Bitmap getMinimapBitmap()
        {
            Bitmap minimap = new Bitmap(minimapBkg);
            using (Graphics g = Graphics.FromImage(minimap))
            {
                Rectangle absLocalFocusArea = convertRelAreaToAbsArea(localUserFocusArea);
                g.FillRectangle(focusAreaBrush, absLocalFocusArea);
                Point localCursorPos = convertRelPosToAbsPos(localRelCursorPos);
                Point remoteCursorPos = convertRelPosToAbsPos(remoteRelCursorPos);
                g.DrawImage(localUserCursor, localCursorPos);
                g.DrawImage(remoteUserCursor, remoteCursorPos);
            }
            return minimap;
        }
        Rectangle convertRelAreaToAbsArea(RectangleF relArea)
        {
            Rectangle absArea = new Rectangle();
            absArea.Width = (int)(relArea.Width * miniMapWidth);
            absArea.Height = (int)(relArea.Height * miniMapHeight);
            absArea.X = (int)(relArea.X * relArea.Width);
            absArea.Y = (int)(relArea.Y * relArea.Height);
            return absArea;
        }
        Point convertRelPosToAbsPos(PointF relPos)
        {
            Point absPoint = new Point();
            absPoint.X = (int)(relPos.X * miniMapWidth);
            absPoint.Y = (int)(relPos.Y * miniMapHeight);
            return absPoint;
        }
    }
}
