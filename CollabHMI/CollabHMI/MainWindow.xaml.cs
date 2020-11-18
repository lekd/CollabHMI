using Newtonsoft.Json;
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
using Point = System.Drawing.Point;

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
        //minimap handling variables
        MiniMapController minimapController;
        Timer minimapUpdateScheduler;
        //controller to manage the HMI screen
        HMIScreenController hmiScreenController;
        Timer hmiScreenUpdateScheduler;
        //store latest remote mouse event for further processing
        MouseEventMessage latestRemoteMouseEvent;
        public MainWindow()
        {
            InitializeComponent();
            InitWebsocketConnection();
            Random rnd = new Random();
            userID = rnd.Next();
            //init screen controller
            hmiScreenController = new HMIScreenController();
            //init a minimap instance
            minimapController = new MiniMapController(500, 300);
            minimapController.updateLocalUserFocusArea(0, 0, 0.5f, 0.5f);
            //scheduling time to update minimap
            minimapUpdateScheduler = new Timer();
            minimapUpdateScheduler.Interval = 50;
            minimapUpdateScheduler.Elapsed += MinimapUpdateScheduler_Elapsed;
            minimapUpdateScheduler.Start();
            //scheduling time to update latest screenshot
            hmiScreenUpdateScheduler = new Timer();
            hmiScreenUpdateScheduler.Interval = 40;
            hmiScreenUpdateScheduler.Elapsed += HmiScreenUpdateScheduler_Elapsed;
            hmiScreenUpdateScheduler.Start();
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
                //store latest remote mouse event for further processing if needed
                latestRemoteMouseEvent = mouseEventMsg;
                //update remote mouse cursor in minimap
                minimapController.UpdateRemoteCursorPos(mouseEventMsg.RelPosX, mouseEventMsg.RelPosY);
                //test display the received message to check the result
                Action displayReceivedMsg = delegate
                {
                    lbldebugText.Content = string.Format("RemotePos: ({0}, {1})", mouseEventMsg.RelPosX, mouseEventMsg.RelPosY);
                };
                lbldebugText.Dispatcher.Invoke(displayReceivedMsg);
                //update focus area & cursor location of remote user into hmiScreenController & tooltipController
                tooltipRemoteUser.UpdateRemoteFocusAndCursor(new RectangleF(latestRemoteMouseEvent.FocusAreaLeft, latestRemoteMouseEvent.FocusAreaTop,
                                                                            latestRemoteMouseEvent.FocusAreaRight - latestRemoteMouseEvent.FocusAreaLeft,
                                                                            latestRemoteMouseEvent.FocusAreaBottom - latestRemoteMouseEvent.FocusAreaTop),
                                                              new PointF(mouseEventMsg.RelPosX, mouseEventMsg.RelPosY));
                hmiScreenController.UpdateRemoteCursorRelPos(latestRemoteMouseEvent.RelPosX, latestRemoteMouseEvent.RelPosY);
            }
        }
        void sendOverMouseEvent(MouseEventMessage msg)
        {
            string jsonMsg = JsonConvert.SerializeObject(msg, Formatting.Indented);
            if (isWSConnected)
            {
                wsClient.Send(jsonMsg);
            }
        }
        #endregion
        #region visualization update
        private void MinimapUpdateScheduler_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateMinimap();
        }
        object miniMapUpdateLock = new object();
        void UpdateMinimap()
        {
            lock(miniMapUpdateLock)
            {
                //minimapController.UpdateRemoteCursorPos(latestRemoteMouseEvent.RelPosX, latestRemoteMouseEvent.RelPosY);
                Bitmap minimapBmp = minimapController.getMinimapBitmap();
                Action showMinimap = delegate
                {
                    miniMap.Source = Utilities.ToBitmapImage(minimapBmp, ImageFormat.Png);
                };
                miniMap.Dispatcher.Invoke(showMinimap);
            }
        }
        private void HmiScreenUpdateScheduler_Elapsed(object sender, ElapsedEventArgs e)
        {
            tooltipRemoteUser.UpdateHMIScreenBmp(hmiScreenController.getCurrentHMISceenshot());
        }
        #endregion
        #region Local Mouse Event Handler
        System.Windows.Point getMouseLocationOnImage(MouseEventArgs mouseEvtArgs)
        {
            System.Windows.Point mousePosOnImage = new System.Windows.Point();
            if(imgHMIBkg.Source != null)
            {
                var controlSpacePosition = mouseEvtArgs.GetPosition(imgHMIBkg);
                mousePosOnImage.X = Math.Floor(controlSpacePosition.X * imgHMIBkg.Source.Width / imgHMIBkg.ActualWidth);
                mousePosOnImage.Y = Math.Floor(controlSpacePosition.Y * imgHMIBkg.Source.Height / imgHMIBkg.ActualHeight);
                return mousePosOnImage;
            }
            return new System.Windows.Point(-1, -1);
        }
        private void imgHMIBkg_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void imgHMIBkg_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void imgHMIBkg_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point mouseP = getMouseLocationOnImage(e);
            System.Windows.Point relMouseP = new System.Windows.Point(mouseP.X / imgHMIBkg.Source.Width, mouseP.Y / imgHMIBkg.Source.Height);
            //update local cursor position in minimap
            minimapController.UpdateLocalCursorPos(relMouseP.X, relMouseP.Y);
            //lbldebugText.Content = String.Format("Local mouse Pos: ({0},{1})", relMouseP.X, relMouseP.Y);
            MouseEventMessage mouseEventMsg = new MouseEventMessage();
            mouseEventMsg.UserID = userID;
            mouseEventMsg.RelPosX = (float)relMouseP.X;
            mouseEventMsg.RelPosY = (float)relMouseP.Y;
            //at the moment, by the default the focused area is the entire HMI screen, if focus on a smaller area, this needs to be changed
            mouseEventMsg.FocusAreaTop = 0;
            mouseEventMsg.FocusAreaLeft = 0;
            mouseEventMsg.FocusAreaRight = 0.5f;
            mouseEventMsg.FocusAreaBottom = 0.5f;
            sendOverMouseEvent(mouseEventMsg);
            //UpdateMinimap();
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isWSConnected)
            {
                wsClient.Close();
                isWSConnected = false;
            }
            minimapUpdateScheduler.Stop();
            hmiScreenUpdateScheduler.Stop();
            tooltipRemoteUser.StopUpdatingTooltipContent();
        }
        #region Remote Avatar mouse event handler
        private void imgRemoteUserAvatar_MouseEnter(object sender, MouseEventArgs e)
        {
            tooltipRemoteUser.Visibility = Visibility.Hidden;
            //miniMap.Visibility = Visibility.Visible;
        }

        private void imgRemoteUserAvatar_MouseLeave(object sender, MouseEventArgs e)
        {
            tooltipRemoteUser.Visibility = Visibility.Hidden;
            //miniMap.Visibility = Visibility.Hidden;
        }

        private void imgRemoteUserAvatar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            tooltipRemoteUser.Visibility = Visibility.Visible;
            //miniMap.Visibility = Visibility.Hidden;
        }
        #endregion

    }
}
