using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Battery
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region 成员变量
        /// <summary>
        /// 进度
        /// </summary>
        private int _proVlaue = 0;
        /// <summary>
        /// 线程
        /// </summary>
        private BackgroundWorker _worker = new BackgroundWorker();
        /// <summary>
        /// 随机数
        /// </summary>
        private Random _rd = new Random();
        /// <summary>
        /// 外部大气泡最大个数
        /// </summary>
        private int _maxOuterBigBubbleCount = 10;
        /// <summary>
        /// 外部大气泡信息集合
        /// </summary>
        private List<BubbleInfo> _outerBigBubbleInfoList = new List<BubbleInfo>();
        /// <summary>
        /// 外部小气泡最大个数
        /// </summary>
        private int _maxOuterSmallBubbleCount = 20;
        /// <summary>
        /// 外部小气泡信息集合
        /// </summary>
        private List<BubbleInfo> _outerSmallBubbleInfoList = new List<BubbleInfo>();
        /// <summary>
        /// 内部气泡最大个数
        /// </summary>
        private int _maxInnerBubbleCount = 5;
        /// <summary>
        /// 内部气泡信息集合
        /// </summary>
        private List<BubbleInfo> _innerBubbleInfoList = new List<BubbleInfo>();
        #endregion

        #region 事件
        #region loaded
        /// <summary>
        /// loaded
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            _worker.WorkerReportsProgress = true;
            _worker.DoWork += _worker_DoWork;
            _worker.ProgressChanged += _worker_ProgressChanged;
            _worker.RunWorkerAsync();
        }
        #endregion
        #region dowork
        /// <summary>
        /// dowork
        /// </summary>
        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (_proVlaue <= 100)
            {
                _worker.ReportProgress(_proVlaue);
                Thread.Sleep(1000);
                _proVlaue += 1;
            }
        }
        #endregion
        #region 进度变化
        /// <summary>
        /// 进度变化
        /// </summary>
        void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            text_Value.Text = _proVlaue.ToString();
            rec_Water.Height = _proVlaue;

            if (e.ProgressPercentage == 100)
            {
                //注销事件
                CompositionTarget.Rendering -= CompositionTarget_Rendering;
                //清除粒子
                cvs_Inner.Children.Clear();
                cvs_Outer.Children.Clear();
                _outerBigBubbleInfoList.Clear();
                _outerSmallBubbleInfoList.Clear();
                _innerBubbleInfoList.Clear();
            }
        }
        #endregion
        #region 帧渲染
        /// <summary>
        /// 帧渲染
        /// </summary>
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            OuterBigBubbleAnimation();
            AddOuterBigBubble();
            OuterSmallBubbleAnimation();
            AddOuterSmallBubble();
            InnerBubbleAnimation();
            AddInnerBubble();
        }
        #endregion
        #endregion

        #region 方法
        #region 获取气泡
        /// <summary>
        /// 获取气泡
        /// </summary>
        /// <param name="diameter">直径</param>
        /// <returns>Viewbox</returns>
        private Viewbox GetBubble(double diameter)
        {
            #region 气泡轮廓
            Ellipse ellOuter = new Ellipse();
            ellOuter.Width = 200;
            ellOuter.Height = 200;
            RadialGradientBrush rgBrush = new RadialGradientBrush();
            rgBrush.GradientStops.Add(new GradientStop() { Offset = 0, Color = Color.FromArgb(255, 20, 240, 10) });
            rgBrush.GradientStops.Add(new GradientStop() { Offset = 0.85, Color = Color.FromArgb(255, 20, 150, 10) });
            rgBrush.GradientStops.Add(new GradientStop() { Offset = 1, Color = Color.FromArgb(255, 20, 100, 10) });
            ellOuter.Fill = rgBrush;
            #endregion

            #region 月牙形状的反光
            //下半弧
            PathFigure pf0 = new PathFigure() { StartPoint = new Point(100, 185) };
            pf0.Segments.Add(new ArcSegment() { Point = new Point(185, 100), Size = new Size(100, 100) });
            PathGeometry pg0 = new PathGeometry();
            pg0.Figures.Add(pf0);
            //上半弧
            PathFigure pf1 = new PathFigure() { StartPoint = new Point(100, 185) };
            pf1.Segments.Add(new ArcSegment() { Point = new Point(185, 100), Size = new Size(200, 200) });
            PathGeometry pg1 = new PathGeometry();
            pg1.Figures.Add(pf1);

            GeometryGroup gg = new GeometryGroup();
            gg.Children.Add(pg0);
            gg.Children.Add(pg1);

            Path pathMoon = new Path();
            pathMoon.Data = gg;
            pathMoon.Fill = new SolidColorBrush(Color.FromArgb((byte)(255 * 0.7), 255, 255, 255));
            pathMoon.Effect = new BlurEffect() { Radius = 10 };
            #endregion

            #region 两个小圆球的反光
            //上面的小的椭圆
            Ellipse ellSmall = new Ellipse();
            ellSmall.Width = 20;
            ellSmall.Height = 10;
            ellSmall.HorizontalAlignment = HorizontalAlignment.Left;
            ellSmall.VerticalAlignment = VerticalAlignment.Top;
            ellSmall.Margin = new Thickness(60, 40, 0, 0);
            ellSmall.Fill = new SolidColorBrush(Color.FromArgb((byte)(255 * 0.5), 255, 255, 255));
            ellSmall.Effect = new BlurEffect() { Radius = 10 };
            //下面的大的椭圆
            Ellipse ellBig = new Ellipse();
            ellBig.Width = 40;
            ellBig.Height = 20;
            ellBig.HorizontalAlignment = HorizontalAlignment.Left;
            ellBig.VerticalAlignment = VerticalAlignment.Top;
            ellBig.Margin = new Thickness(30, 55, 0, 0);
            ellBig.Fill = new SolidColorBrush(Color.FromArgb((byte)(255 * 0.5), 255, 255, 255));
            ellBig.Effect = new BlurEffect() { Radius = 10 };
            #endregion

            Grid grid = new Grid() { Width = 200, Height = 200 };
            grid.Children.Add(ellOuter);
            grid.Children.Add(ellSmall);
            grid.Children.Add(ellBig);
            grid.Children.Add(pathMoon);

            Viewbox vb = new Viewbox();
            vb.Child = grid;
            vb.Width = diameter;
            vb.Height = diameter;

            return vb;
        }
        #endregion
        #region 外部大气泡动画
        /// <summary>
        /// 外部大气泡动画
        /// </summary>
        private void OuterBigBubbleAnimation()
        {
            for (int i = 0; i < _outerBigBubbleInfoList.Count; i++)
            {
                BubbleInfo info = _outerBigBubbleInfoList[i];
                UIElement bubble = info.Bubble;
                if (info.Moved <= info.Distance)
                {
                    double cvsTop = Canvas.GetTop(bubble);
                    Canvas.SetTop(bubble, cvsTop - info.Velocity);
                    info.Moved += info.Velocity;
                }
                else
                {
                    cvs_Outer.Children.Remove(bubble);
                    _outerBigBubbleInfoList.Remove(info);
                    i--;
                }
            }
        }
        #endregion
        #region 添加外部大气泡
        /// <summary>
        /// 添加外部大气泡
        /// </summary>
        private void AddOuterBigBubble()
        {
            //大气泡 半径6-10 在Canvers中部20-30 速率0.2-0.5 距离30-60
            if (_outerBigBubbleInfoList.Count < _maxOuterBigBubbleCount)
            {
                int size = _rd.Next(6, 11);
                Viewbox bubble = GetBubble(size);
                Canvas.SetLeft(bubble, _rd.Next(20, 31) - size / 2);
                Canvas.SetTop(bubble, 100 - size / 2);
                cvs_Outer.Children.Add(bubble);

                BubbleInfo info = new BubbleInfo();
                info.Velocity = _rd.Next(20, 51) / 100.00;
                info.Distance = _rd.Next(30, 61);
                info.Bubble = bubble;

                _outerBigBubbleInfoList.Add(info);
            }
        }
        #endregion
        #region 外部小气泡动画
        /// <summary>
        /// 外部小气泡动画
        /// </summary>
        private void OuterSmallBubbleAnimation()
        {
            for (int i = 0; i < _outerSmallBubbleInfoList.Count; i++)
            {
                BubbleInfo info = _outerSmallBubbleInfoList[i];
                UIElement bubble = info.Bubble;
                if (info.Moved <= info.Distance)
                {
                    double cvsTop = Canvas.GetTop(bubble);
                    Canvas.SetTop(bubble, cvsTop - info.Velocity);
                    info.Moved += info.Velocity;
                }
                else
                {
                    cvs_Outer.Children.Remove(bubble);
                    _outerSmallBubbleInfoList.Remove(info);
                    i--;
                }
            }
        }
        #endregion
        #region 添加外部小气泡
        /// <summary>
        /// 添加外部小气泡
        /// </summary>
        private void AddOuterSmallBubble()
        {
            //小气泡 半径2-5 在整个Canvers随机分散0-50 速率0.4-1.0 距离10-100
            if (_outerSmallBubbleInfoList.Count < _maxOuterSmallBubbleCount)
            {
                int size = _rd.Next(2, 6);
                Ellipse bubble = new Ellipse() { Width = size, Height = size, Fill = new SolidColorBrush(Color.FromRgb(15, 190, 9)) };
                Canvas.SetLeft(bubble, _rd.Next(0, 51) - size / 2);
                Canvas.SetTop(bubble, 100 - size / 2);
                cvs_Outer.Children.Add(bubble);

                BubbleInfo info = new BubbleInfo();
                info.Velocity = _rd.Next(40, 101) / 100.00;
                info.Distance = _rd.Next(10, 101);
                info.Bubble = bubble;

                _outerSmallBubbleInfoList.Add(info);
            }
        }
        #endregion
        #region 内部气泡动画
        /// <summary>
        /// 内部气泡动画
        /// </summary>
        private void InnerBubbleAnimation()
        {
            for (int i = 0; i < _innerBubbleInfoList.Count; i++)
            {
                BubbleInfo info = _innerBubbleInfoList[i];
                UIElement bubble = info.Bubble;
                if (info.Moved <= info.Distance)
                {
                    double cvsTop = Canvas.GetTop(bubble);
                    Canvas.SetTop(bubble, cvsTop - info.Velocity);
                    info.Moved += info.Velocity;
                }
                else
                {
                    cvs_Inner.Children.Remove(bubble);
                    _innerBubbleInfoList.Remove(info);
                    i--;
                }
            }
        }
        #endregion
        #region 添加内部气泡
        /// <summary>
        /// 添加内部气泡
        /// </summary>
        private void AddInnerBubble()
        {
            //小气泡 半径4-8 在整个Canvers随机分散5-45 速率0.05-0.1 距离5-15
            if (_innerBubbleInfoList.Count < _maxInnerBubbleCount)
            {
                int size = _rd.Next(4, 9);
                Viewbox bubble = GetBubble(size);
                Canvas.SetLeft(bubble, _rd.Next(5, 46) - size / 2);
                Canvas.SetTop(bubble, 100 - rec_Water.Height - size / 2);
                cvs_Inner.Children.Add(bubble);

                BubbleInfo info = new BubbleInfo();
                info.Velocity = _rd.Next(5, 11) / 100.00;
                info.Distance = _rd.Next(5, 16);
                info.Bubble = bubble;

                _innerBubbleInfoList.Add(info);
            }
        }
        #endregion
        #endregion
    }

    /// <summary>
    /// 气泡信息
    /// </summary>
    public class BubbleInfo
    {
        /// <summary>
        /// 速率(每帧移动的距离)
        /// </summary>
        public double Velocity { get; set; }
        /// <summary>
        /// 距离
        /// </summary>
        public double Distance { get; set; }
        /// <summary>
        /// 已经移动的距离
        /// </summary>
        public double Moved { get; set; }
        /// <summary>
        /// 气泡
        /// </summary>
        public UIElement Bubble { get; set; }
    }
}
