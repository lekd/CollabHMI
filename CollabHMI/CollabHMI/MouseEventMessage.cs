using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabHMI
{
    public class MouseEventMessage
    {
        int _userID;
        float _relPosX;
        float _relPosY;
        float _mouseEventType;
        float _focusAreaLeft;
        float _focusAreaTop;
        float _focusAreaRight;
        float _focusAreaBottom;

        public int UserID { get => _userID; set => _userID = value; }
        public float RelPosX { get => _relPosX; set => _relPosX = value; }
        public float RelPosY { get => _relPosY; set => _relPosY = value; }
        public float MouseEventType { get => _mouseEventType; set => _mouseEventType = value; }
        public float FocusAreaLeft { get => _focusAreaLeft; set => _focusAreaLeft = value; }
        public float FocusAreaTop { get => _focusAreaTop; set => _focusAreaTop = value; }
        public float FocusAreaRight { get => _focusAreaRight; set => _focusAreaRight = value; }
        public float FocusAreaBottom { get => _focusAreaBottom; set => _focusAreaBottom = value; }
    }
}
