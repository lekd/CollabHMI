using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int userID;
        WebSocketSharp.WebSocket wsClient = null;
        bool isWSConnected = false;
        public MainWindow()
        {
            InitializeComponent();
            InitWebsocketConnection();
            Random rnd = new Random();
            userID = rnd.Next();
        }
        void InitWebsocketConnection()
        {
            Uri wsUri = new Uri("ws://192.168.0.184:4040/main.html", UriKind.Absolute);
            wsClient = new WebSocketSharp.WebSocket(wsUri.AbsoluteUri);
            wsClient.Connect();
            isWSConnected = true;
            wsClient.OnOpen += WsClient_OnOpen;
            wsClient.OnMessage += WsClient_OnMessage;
        }
        #region Websocket Event Handler
        
        private void WsClient_OnOpen(object sender, EventArgs e)
        {
            isWSConnected = true;
        }
        private void WsClient_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            if(e.IsText)
            {
                string msgText = e.Data;
                MouseEventMessage mouseEventMsg = JsonConvert.DeserializeObject<MouseEventMessage>(msgText);
                //test display the received message to check the result
                Action displayReceivedMsg = delegate
                {
                    lbldebugText.Content = string.Format("RemotePos: ({0}, {1})", mouseEventMsg.RelPosX, mouseEventMsg.RelPosY);
                };
                lbldebugText.Dispatcher.Invoke(displayReceivedMsg);
            }
        }
        #endregion

        #region Local Mouse Event Handler
        Point getMouseLocationOnImage(MouseEventArgs mouseEvtArgs)
        {
            Point mousePosOnImage = new Point();
            if(imgHMIBkg.Source != null)
            {
                var controlSpacePosition = mouseEvtArgs.GetPosition(imgHMIBkg);
                mousePosOnImage.X = Math.Floor(controlSpacePosition.X * imgHMIBkg.Source.Width / imgHMIBkg.ActualWidth);
                mousePosOnImage.Y = Math.Floor(controlSpacePosition.Y * imgHMIBkg.Source.Height / imgHMIBkg.ActualHeight);
                return mousePosOnImage;
            }
            return new Point(-1, -1);
        }
        private void imgHMIBkg_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void imgHMIBkg_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void imgHMIBkg_MouseMove(object sender, MouseEventArgs e)
        {
            Point mouseP = getMouseLocationOnImage(e);
            Point relMouseP = new Point(mouseP.X / imgHMIBkg.Source.Width, mouseP.Y / imgHMIBkg.Source.Height);
            //lbldebugText.Content = String.Format("Local mouse Pos: ({0},{1})", relMouseP.X, relMouseP.Y);
            MouseEventMessage mouseEventMsg = new MouseEventMessage();
            mouseEventMsg.UserID = userID;
            mouseEventMsg.RelPosX = (float)relMouseP.X;
            mouseEventMsg.RelPosY = (float)relMouseP.Y;
            //at the moment, by the default the focused area is the entire HMI screen, if focus on a smaller area, this needs to be changed
            mouseEventMsg.FocusAreaTop = 0;
            mouseEventMsg.FocusAreaLeft = 0;
            mouseEventMsg.FocusAreaRight = 1.0f;
            mouseEventMsg.FocusAreaBottom = 1.0f;
            sendOverMouseEvent(mouseEventMsg);
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isWSConnected)
            {
                wsClient.Close();
                isWSConnected = false;
            }
        }
        void sendOverMouseEvent(MouseEventMessage msg)
        {
            string jsonMsg = JsonConvert.SerializeObject(msg, Formatting.Indented);
            if(isWSConnected)
            {
                wsClient.Send(jsonMsg);
            }
        }

    }
}
