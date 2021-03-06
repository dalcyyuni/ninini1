﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EOSDigital.API;
using EOSDigital.SDK;
using System.Diagnostics;
using System.Windows.Threading;
using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using System.Windows.Shapes;
using System.Net;
using System.Drawing.Imaging;
using System.Drawing;

namespace Kinect2FaceHD_NET
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Page
    {
        

     

        //

        private KinectSensor _sensor = KinectSensor.GetDefault();

        private BodyFrameSource _bodySource = null;

        private BodyFrameReader _bodyReader = null;

        private HighDefinitionFaceFrameSource _faceSource = null;

        private HighDefinitionFaceFrameReader _faceReader = null;

        private FaceAlignment _faceAlignment = null;

        private FaceModel _faceModel = null;

        private List<Ellipse> _points = new List<Ellipse>();
        private ColorFrameReader reader = null;
        private readonly int bytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
        private readonly WriteableBitmap _bmp = new WriteableBitmap(1920, 1080, 96, 96, PixelFormats.Bgr32, null);
        Byte[] _frameData = null;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;
        private static int id_number = 0;
        private int count = 0;
        private Boolean hasFace = false;
        public Register()
        {
            this.WindowHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            this.WindowWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            InitializeComponent();
          

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            //
            _sensor = KinectSensor.GetDefault();
            _sensor.Open();
            //
            _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
            _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

            //
            //  this.reader = _sensor.ColorFrameSource.OpenReader();
            // reader.FrameArrived += ColorFrameArrived;
            if (_sensor != null)
            {
                _bodySource = _sensor.BodyFrameSource;
                _bodyReader = _bodySource.OpenReader();
                _bodyReader.FrameArrived += BodyReader_FrameArrived;

                _faceSource = new HighDefinitionFaceFrameSource(_sensor);

                _faceReader = _faceSource.OpenReader();
                _faceReader.FrameArrived += FaceReader_FrameArrived;

                _faceModel = new FaceModel();
                _faceAlignment = new FaceAlignment();

                _sensor.Open();
            }

        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    Body[] bodies = new Body[frame.BodyCount];
                    frame.GetAndRefreshBodyData(bodies);

                    Body body = bodies.Where(b => b.IsTracked).FirstOrDefault();

                    if (!_faceSource.IsTrackingIdValid)
                    {
                        if (body != null)
                        {
                            _faceSource.TrackingId = body.TrackingId;
                        }
                    }
                }
            }
        }

        public void getNumber()
        {

            System.Net.HttpWebRequest wRep;
            HttpWebResponse wRes;
            Uri uri;
            string cookie = "";
            string resResult = "";
            try
            {
                uri = new Uri("http://203.252.218.16:8081/test/randomId.jsp");
                wRep = (HttpWebRequest)WebRequest.Create(uri);
                wRep.Method = "GET";
                wRep.ServicePoint.Expect100Continue = false;
                wRep.CookieContainer = new CookieContainer();
                wRep.CookieContainer.SetCookies(uri, cookie);

                using (wRes = (HttpWebResponse)wRep.GetResponse())
                {
                    Stream respPostStream = wRes.GetResponseStream();
                    StreamReader readerPost = new StreamReader(respPostStream, Encoding.GetEncoding("EUC-KR"), true);
                    resResult = readerPost.ReadToEnd();
                    if (resResult != null)
                    {
                        Int32 number = Int32.Parse(resResult);
                        id_number = number;
                        hi.Text = "Serial Code : ninini" + id_number;
                        Debug.WriteLine("--------------");
                        //Debug.WriteLine(resResult);
                        Debug.WriteLine(resResult);
                        Debug.WriteLine("--------------");
                    }
                    //if (resResult != null)
                    //{
                    //Int32 number = Int32.Parse(resResult);
                    //return number;
                    //}




                }
            }
            catch (WebException ex) { }
        }

        private static bool CopyScreen()
        {

            var width = (int)System.Windows.SystemParameters.PrimaryScreenWidth;
            var height = (int)System.Windows.SystemParameters.PrimaryScreenHeight;
            var screenBmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bmpGraphics = Graphics.FromImage(screenBmp);

            bmpGraphics.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(width, height));
            Bitmap bm3 = new Bitmap(screenBmp);

            bm3.Save("C:\\Users\\339-1\\Desktop\\유니폴더\\눈써비\\얼굴인식v3\\kinect-2-face-hd-master - 복사본\\Kinect2FaceHD\\Kinect2FaceHD_NET\\Image\\Login_data\\"+id_number+".jpg", ImageFormat.Jpeg);
            bm3.Dispose();


            return true;

        }


        private void FaceReader_FrameArrived(object sender, HighDefinitionFaceFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null && frame.IsFaceTracked)
                {
                    frame.GetAndRefreshFaceAlignmentResult(_faceAlignment);
                    //UpdateFacePoints();
                }
            }


        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {

            var reference = e.FrameReference.AcquireFrame();


            string leftHandState = "-";
            string rightHandState = "-";
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);

                    foreach (var body in _bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                // Find the joints
                                Joint handRight = body.Joints[JointType.HandRight];
                                Joint thumbRight = body.Joints[JointType.ThumbRight];

                                Joint handLeft = body.Joints[JointType.HandLeft];
                                Joint thumbLeft = body.Joints[JointType.ThumbLeft];
                                count++;
                                if (count == 10)
                                    getNumber();
                                else if (count == 20)
                                {
                                    if (CopyScreen())
                                    {
                                        Debug.WriteLine(count);
                                        FaceLogin fl = new FaceLogin();
                                        fl.HasFace(id_number);
                                        Debug.WriteLine(fl.hasFace);
                                        hi.Text = fl.hasFace + "";
                                        this.hasFace = fl.hasFace;
                                    }
                                }
                                else if (count > 25 && count < 35 && this.hasFace == false )
                                {
                                   
                                    //FaceLogin fl = new FaceLogin();
                                    ///fl.HasFace(id_number);
                                    //Debug.WriteLine(fl.hasFace);
                                   // hi.Text = fl.hasFace + "";
                                   // this.hasFace = fl.hasFace;
                                }

                               // if (fl.end == false)
                               // {
                                    //login_message.Text = "로그인 중";
                              //  }
                               // else
                               // {
                                   // if (Microsoft.Samples.Kinect.HDFaceBasics.App.check_login == false)
                                       // login_message.Text = "로그인 실패";
                                   // el//se
                                      //  login_message.Text = "로그인 성공";
                              //  }
                                /*
                                if (count < 200)
                                {
                                    count++;
                                    login_message.Text = "로그인 중입니다\n 여기를 보세요";
                                    // Capture();

                                }

                                if (count == 20)
                                {
                                    //CopyScreen();
                                }
                                
                                else if (count ==40)
                                {
                                    fl.Login();
                                }
                                Debug.WriteLine(Microsoft.Samples.Kinect.HDFaceBasics.App.check_login);
                                if (count > 90 &&count<100)
                                {
                                    if (Microsoft.Samples.Kinect.HDFaceBasics.App.check_login == false)
                                            login_message.Text = "로그인 중";//
                                    else
                                    {
                                        login_message.Text = "로그인 성공";
                                    }
                                }
                                else if (count > 110 && Microsoft.Samples.Kinect.HDFaceBasics.App.check_login == false)
                                    login_message.Text = "로그인 실패";
                                    */

                                switch (body.HandLeftState)
                                {
                                    case HandState.Open:
                                        leftHandState = "Open";
                                        //  left_check = true;
                                        //check = true;
                                        break;
                                    case HandState.Closed:
                                        leftHandState = "Closed";
                                        //left_check = false;
                                        //check = true;
                                        break;
                                    case HandState.Lasso:
                                        leftHandState = "Lasso";
                                        break;
                                    case HandState.Unknown:
                                        //left_check = false;
                                        // leftHandState = "Unknown...";
                                        break;
                                    case HandState.NotTracked:
                                        // left_check = false;
                                        //  leftHandState = "Not tracked";
                                        break;
                                    default:
                                        break;
                                }

                                // LeftHandState.Text = "Left : "+leftHandState;

                                switch (body.HandRightState)
                                {
                                    case HandState.Open:
                                        rightHandState = "Open";

                                        Microsoft.Samples.Kinect.HDFaceBasics.App.page = 2;
                                        try
                                        {
                                            // this.NavigationService.Navigate(new Uri("UserControl6.xaml", UriKind.Relative));

                                        }
                                        catch (NullReferenceException) { }

                                        //right_check = true;
                                        //check = true;
                                        break;
                                    case HandState.Closed:
                                        rightHandState = "Closed";
                                        // right_check = false;
                                        //check = true;
                                        break;
                                    case HandState.Lasso:
                                        //  CopyScreen();
                                        //  rightHandState = "Lasso";
                                        break;
                                    case HandState.Unknown:
                                        //  right_check = false;
                                        rightHandState = "Unknown...";
                                        break;
                                    case HandState.NotTracked:
                                        // right_check = false;
                                        rightHandState = "Not tracked";
                                        break;

                                    default:
                                        break;
                                }
                                LeftHandState.Text = "Left : " + leftHandState;
                                RightHandState.Text = "Right : " + rightHandState;
                            }
                        }
                    }
                }
            }
        }



    }
}
