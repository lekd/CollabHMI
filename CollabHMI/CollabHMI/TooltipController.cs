using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabHMI
{
    public class TooltipController
    {
        SolidBrush bkgBrush = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
        Bitmap currentScreenBmp;
        int tooltipImgW;
        int tooltipImgH;
        float relCursorPosX;
        float relCursorPosY;
        RectangleF relFocusArea = new RectangleF();
        public TooltipController(int w,int h)
        {
            tooltipImgW = w;
            tooltipImgH = h;
        }
        public void UpdateCurrentScreenBmp(Bitmap curScreenBmp)
        {
            currentScreenBmp = new Bitmap(curScreenBmp);
        }
        public void UpdateCursorPos(double cursorRelX,double cursorRelY)
        {
            relCursorPosX = (float)cursorRelX;
            relCursorPosY = (float)cursorRelY;
        }
        public void UpdateRelFocusArea(float left,float top,float right,float bottom)
        {
            relFocusArea.X = left;
            relFocusArea.Y = top;
            relFocusArea.Width = right - left;
            relFocusArea.Height = bottom - top;
        }
        public Bitmap ExtractTooltipBitmap()
        {
            Rectangle focusAreaOnScreen = convertRelAreaToAbsArea(relFocusArea);
            Bitmap tooltipBmp = new Bitmap(focusAreaOnScreen.Width, focusAreaOnScreen.Height);
            using (Graphics g = Graphics.FromImage(tooltipBmp))
            {
                g.DrawImage(currentScreenBmp, new Rectangle(0, 0, tooltipBmp.Width, tooltipBmp.Height), focusAreaOnScreen, GraphicsUnit.Pixel);
            }
            return tooltipBmp;
        }
        RectangleF cropFocusAreaToZoonInCursor(RectangleF relFocusArea, float cursorRelX,float cursorRelY)
        {
            RectangleF cropFocusArea = new RectangleF();
            cropFocusArea.Width = 0.2f;
            cropFocusArea.Height = 0.2f;
            PointF relFocusAreaCenter = new PointF(relFocusArea.Width / 2 + relFocusArea.X, relFocusArea.Height / 2 + relFocusArea.Y);
            //make sure the left of the crop area doesn't exceed the left of the focus area
            if(cursorRelX - cropFocusArea.Width/2 >= relFocusArea.Left)
            {
                cropFocusArea.X = cursorRelX - cropFocusArea.Width / 2;
            }
            else
            {   
                cropFocusArea.X = relFocusArea.Left;
            }
            //make sure the right of the crop area doesn't exceed the right of the focus area
            if (cursorRelX + cropFocusArea.Width / 2 > relFocusArea.Right)
            {
                cropFocusArea.Width = relFocusArea.Right - cropFocusArea.Left;
            }
            //make sure the top of the crop area doesn't exceed the top of the focus area
            if (cursorRelY - cropFocusArea.Height/2 <= relFocusArea.Top)
            {
                cropFocusArea.Y = cursorRelY - cropFocusArea.Height/2;
            }
            else
            {
                cropFocusArea.Y = relFocusArea.Top;
            }
            //make sure the bottom of the crop area doesn't exceed the bottom of the focus area
            if (cursorRelY + cropFocusArea.Height/2 > relFocusArea.Right)
            {
                cropFocusArea.Width = relFocusArea.Right - cropFocusArea.Left;
            }
            return cropFocusArea;
        }
        Rectangle convertRelAreaToAbsArea(RectangleF relArea)
        {
            Rectangle absArea = new Rectangle();
            absArea.Width = (int)(relArea.Width * currentScreenBmp.Width);
            absArea.Height = (int)(relArea.Height * currentScreenBmp.Height);
            absArea.X = (int)(relArea.X * currentScreenBmp.Width);
            absArea.Y = (int)(relArea.Y * currentScreenBmp.Height);
            return absArea;
        }
    }
}
