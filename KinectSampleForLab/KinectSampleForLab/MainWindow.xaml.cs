using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace KinectSampleForLab {
    /// <summary>
    /// Kinectのはじめの一歩テンプレート
    /// インタラクションフレームを使うときには別途、Intaraction.DLLが必要です。
    /// </summary>
    public partial class MainWindow : Window {

        private KinectSensor kinect;


        //カラーフレーム関係
        private Int32Rect colorImageBitmapRect;
        private int colorImageStride;
        private int colorFrameWidth;
        private int colorFrameHeight;
        private WriteableBitmap colorBitmap;

        //デプスフレーム関係        
        private int depthImageStride;
        private Int32Rect depthImageBitmapRect;
        private int depthFrameWidth;
        private int depthFrameHeight;
        private DepthImagePixel[] depthPixelData;
        private short[] depthBitmapPixelData;        
        private WriteableBitmap depthBitmap;

        //インタラクションストリーム利用時
        //InteractionStream iStream;


        public MainWindow() {
            InitializeComponent();


            kinectInitAll();
        
        
        }

        private void button1_Click(object sender, RoutedEventArgs e) {

            Start();



        }

        public void Start() {

         
            kinect.Start();
         
            //インタラクションストリームを利用する場合
            //iStream = new InteractionStream(kinect, new KinectAdapter());
            //iStream.InteractionFrameReady += stream_InteractionFrameReady;
            
        }

        private void kinectInitAll() {

            kinect = KinectSensor.KinectSensors[0];
            System.Diagnostics.Debug.WriteLine("AllFrameReadyモード");
            kinect.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinect_AllFramesReady);


            //スケルトンフレームの有効化
            // kinect.SkeletonStream.Enable(tsp);
            kinect.SkeletonStream.Enable();
            //Depthフレームの有効化
            kinect.DepthStream.Enable();

            //赤外線照射のOFF
            // kinect.ForceInfraredEmitterOff = true;


            //カメラの有効化
            kinect.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30); //RGBモード
            //kinect.ColorStream.Enable(ColorImageFormat.InfraredResolution640x480Fps30);　//赤外線モード
　
            colorImageBitmapRect = new Int32Rect(0, 0, kinect.ColorStream.FrameWidth, kinect.ColorStream.FrameHeight);
            colorImageStride = kinect.ColorStream.FrameWidth * kinect.ColorStream.FrameBytesPerPixel;
            colorFrameHeight = kinect.ColorStream.FrameHeight;
            colorFrameWidth = kinect.ColorStream.FrameWidth;
            
            //colorBitmapの生成（32bitカラーか16bitグレースケールか？）
            colorBitmap = new WriteableBitmap(kinect.ColorStream.FrameWidth, kinect.ColorStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
            //colorBitmap = new WriteableBitmap(kinect.ColorStream.FrameWidth, kinect.ColorStream.FrameHeight, 96, 96, PixelFormats.Gray16, null);
            
            //Depth系初期設定
            depthImageStride = kinect.DepthStream.FrameWidth * kinect.DepthStream.FrameBytesPerPixel;
            depthImageBitmapRect = new Int32Rect(0, 0, kinect.ColorStream.FrameWidth, kinect.ColorStream.FrameHeight);
            depthBitmap = new WriteableBitmap(kinect.DepthStream.FrameWidth, kinect.DepthStream.FrameHeight, 96, 96, PixelFormats.Gray16, null);

            this.depthPixelData = new DepthImagePixel[kinect.DepthStream.FramePixelDataLength];
            this.depthBitmapPixelData = new short[kinect.DepthStream.FramePixelDataLength];
            this.depthFrameHeight = kinect.ColorStream.FrameHeight;
            this.depthFrameWidth = kinect.ColorStream.FrameWidth;


            //ここで、ImageコントロールとWritableBitmapの対応
            colorImage1.Source = colorBitmap;
            //Depthを表示したければこちら
           // colorImage1.Source = depthBitmap;

        }

        void kinect_AllFramesReady(object sender, AllFramesReadyEventArgs args) {
            using (ColorImageFrame colorFrame = args.OpenColorImageFrame())
            using (DepthImageFrame depthFrame = args.OpenDepthImageFrame())
            using (SkeletonFrame skeletonFrame = args.OpenSkeletonFrame()) {
  
                byte[] pixelData;
                //いずれのフレームもnullでなければ処理をすすめる
                if (colorFrame != null && depthFrame != null && skeletonFrame != null) {
                    //スケルトンの情報をSkeleton配列に格納する
                    Skeleton[] skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletonData);                    

                    //カラーフレームの情報をByte配列に格納する
                    pixelData = new byte[colorFrame.PixelDataLength];
                    colorFrame.CopyPixelDataTo(pixelData);
                    //表示処理
                    colorBitmap.WritePixels(this.colorImageBitmapRect, pixelData, this.colorImageStride, 0);

                    ////InteractionStreamを利用する場合の処理
                    //iStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                    //iStream.ProcessSkeleton(skeletonData, kinect.AccelerometerGetCurrentReading(),
                    //                           skeletonFrame.Timestamp);


                    //DepthFrame表示用処理
                    depthFrame.CopyPixelDataTo(this.depthBitmapPixelData);
                    this.depthBitmap.WritePixels(this.depthImageBitmapRect, this.depthBitmapPixelData, this.depthImageStride, 0);

                    //深度情報取得用
                    depthFrame.CopyDepthImagePixelDataTo(this.depthPixelData);

                    //ある関節の情報をとってくる
                    //Joint target= getJointPosition(skeletonFrame, JointType.Head);
                    //string str = string.Format("x={0},y={1},z={2}", target.Position.X, target.Position.Y, target.Position.Z);
                    //textBlock1.Text = str;


                


                }

                //System.Diagnostics.Debug.WriteLine(joint.Position.Y);
            }


        }


        ///**
        // *  インタラクションフレームを利用する場合
        // * 
        // * */
        //void stream_InteractionFrameReady(object sender, InteractionFrameReadyEventArgs e) {


        //    using (var interactionFrame = e.OpenInteractionFrame()) {
        //        if (interactionFrame != null) {
        //            var userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];
        //            interactionFrame.CopyInteractionDataTo(userInfos);

        //            //List<InteractionHandPointer> hands = new List<InteractionHandPointer>();

        //            foreach (var user in userInfos) {
        //                if (user.SkeletonTrackingId != 0) {
        //                    foreach (var hand in user.HandPointers) {
        //                        if (hand.HandType == InteractionHandType.Right) {
        //                            //hands.Add(hand);
        //                            if (hand.HandEventType != 0) {
        //                                System.Diagnostics.Debug.WriteLine(hand.HandType + ":" + hand.HandEventType);
        //                                // break;
        //                            }
        //                        }

        //                    }
        //                }
        //            }

        //            //Grid.ItemsSource = hands;
        //        }
        //    }

        //}



        /**
         *  ある関節の座標を取り出す
         * 
         * */
        public Joint getJointPosition(SkeletonFrame skeletonFrame, JointType jointID) {
            int playerIndex=0;
           
                Skeleton[] skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                skeletonFrame.CopySkeletonDataTo(skeletons);

                //一番最初に引っかかったユーザを対象
                for (playerIndex = 0; playerIndex < skeletons.Length; playerIndex++) {
                    if (skeletons[playerIndex].TrackingState == SkeletonTrackingState.Tracked) {
                        break;
                    }
                }
                //見つからなかった場合
                if (playerIndex == skeletons.Length) {
                    System.Diagnostics.Debug.WriteLine("PlayerLost!!:"+playerIndex);
                    return new Joint();  //ここの処理はこれではダメですね
                }
                Skeleton skeleton = skeletons[playerIndex];
                Joint target = skeleton.Joints[jointID];

                string str = string.Format("x={0},y={1},z={2}", target.Position.X, target.Position.Y, target.Position.Z);
                System.Diagnostics.Debug.WriteLine(str);
                return target;

           
        }



        /**     ここから下は　描画サンプル　**/


        private void DrawEllipse(ColorImagePoint p) {

            
            canvas1.Children.Clear();
            Ellipse myEllipse = new Ellipse();
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();

            // Describes the brush's color using RGB values. 
            // Each value has a range of 0-255.
            mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 0);
            myEllipse.Fill = mySolidColorBrush;
            myEllipse.StrokeThickness = 2;
            //myEllipse.Stroke = Brushes.Black;
            // Set the width and height of the Ellipse.
            myEllipse.Width = 50;
            myEllipse.Height = 50;

            System.Windows.Controls.Canvas.SetLeft(myEllipse, p.X);
            System.Windows.Controls.Canvas.SetTop(myEllipse, p.Y);
            canvas1.Children.Add(myEllipse);

        }



    }
}
