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
using TasSample.Markup;
using TasSample.Models;
using System.IO;
using System.ComponentModel;
using System.Threading;
using TasSample.Automation;

namespace TasSample
{
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

            this.absolutePosition = new Point();
            this.captureLeftTop = new Point();
            this.captureRightBottom = new Point();
        }

        private ScreenWorkflowStartInfoCollection startInfos;

        public ScreenWorkflowStartInfoCollection StartInfos
        {
            get
            {
                if (this.startInfos == null)
                {
                    this.startInfos = XamlFile.Read<ScreenWorkflowStartInfoCollection>("StartConfig.xaml");
                }
                return this.startInfos;
            }
        }

        private Point absolutePosition;

        private Point captureLeftTop;
        private Point captureRightBottom;

        private Thread workflowThread;

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            this.fileNameComboBox.ItemsSource = Directory.GetFiles(Properties.Settings.Default.WorkflowDirPath).ToDictionary(f => System.IO.Path.GetFileNameWithoutExtension(f));

            this.basePositionTextBlock.Text = this.absolutePosition.ToString();
            this.absoluteTextBlock.Text = this.absolutePosition.ToString();

            this.relativeTextBlock.Text = new Point().ToString();

            this.captureLeftTopTextBlock.Text = this.captureLeftTop.ToString();
            this.captureRightBottomTextBlock.Text = this.captureRightBottom.ToString();

            this.imageFormatsComboBox.ItemsSource = Enum.GetValues(typeof(ScreenImageFormat)).Cast<ScreenImageFormat>().ToDictionary(f => f.ToString().ToUpperInvariant());
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (this.getAbsoluteToggleButton.IsChecked == true)
            {
                this.absolutePosition = ScreenManager.GetCursorPosition();

                this.basePositionTextBlock.Text = this.absolutePosition.ToString();
                this.absoluteTextBlock.Text = this.absolutePosition.ToString();

                this.getAbsoluteToggleButton.IsChecked = false;
            }

            if (this.getRelativeToggleButton.IsChecked == true)
            {
                this.relativeTextBlock.Text = (ScreenManager.GetCursorPosition() - this.absolutePosition).ToString();

                if (this.copyRelativeCheckBox.IsChecked == true)
                {
                    Clipboard.SetText(this.relativeTextBlock.Text);
                }

                this.getRelativeToggleButton.IsChecked = false;
            }

            if (this.getColorToggleButton.IsChecked == true)
            {
                Color color = ScreenImage.GetColor(ScreenManager.GetCursorPosition());

                this.colorTextBlock.Text = color.ToString();
                this.colorTextBlock.Background = new SolidColorBrush(color);
                this.colorTextBlock.Foreground = new SolidColorBrush(IsBright(color) ? Colors.Black : Colors.White);

                if (this.copyColorCheckBox.IsChecked == true)
                {
                    Clipboard.SetText(this.colorTextBlock.Text);
                }

                this.getColorToggleButton.IsChecked = false;
            }

            if (this.captureLeftTopToggleButton.IsChecked == true)
            {
                this.captureLeftTop = ScreenManager.GetCursorPosition();

                this.captureLeftTopTextBlock.Text = this.captureLeftTop.ToString();

                this.captureLeftTopToggleButton.IsChecked = false;
            }

            if (this.captureRightBottomToggleButton.IsChecked == true)
            {
                this.captureRightBottom = ScreenManager.GetCursorPosition();

                this.captureRightBottomTextBlock.Text = this.captureRightBottom.ToString();

                this.captureRightBottomToggleButton.IsChecked = false;
            }
        }

        private static bool IsBright(Color color)
        {
            return color.R + color.G + color.B >= 384;
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists((string)this.fileNameComboBox.SelectedValue))
            {
                MessageBox.Show("指定されたファイルは存在しません。");
                return;
            }

            this.startButton.IsEnabled = false;

            ScreenWorkflowContext.CurrentBasePosition = this.absolutePosition;

            SequentialScreenWorkflow activity = XamlFile.Read<SequentialScreenWorkflow>((string)this.fileNameComboBox.SelectedValue);

            this.workflowThread = new Thread(() =>
            {
                activity.Start();

                this.Dispatcher.Invoke(new Action(() => { this.startButton.IsEnabled = true; }));
            });

            this.workflowThread.Start();

            //this.RunWorker(activity);
        }

        private void window_Activated(object sender, EventArgs e)
        {
            if (this.workflowThread != null && this.workflowThread.ThreadState != ThreadState.Stopped)
            {
                this.workflowThread.Abort();

                this.startButton.IsEnabled = true;
            }
        }

        private void savaImageButton_Click(object sender, RoutedEventArgs e)
        {
            Vector v = this.captureRightBottom - this.captureLeftTop;

            if (v.X <= 0 || v.Y <= 0)
            {
                MessageBox.Show("領域の縦横のサイズが正ではありません。");
                return;
            }

            ScreenImageFormat format = (ScreenImageFormat)this.imageFormatsComboBox.SelectedValue;

            ScreenImage image = ScreenImage.GetImage(captureLeftTop, captureRightBottom);

            image.Save(GetFilePathToSaveImage(format), format);
        }

        private static string GetFilePathToSaveImage(ScreenImageFormat format)
        {
            return System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                DateTime.Now.ToString("yyyyMMddHHmmss") + GetExtension(format)
            );
        }

        private static string GetExtension(ScreenImageFormat format)
        {
            switch (format)
            {
                case ScreenImageFormat.Bmp:
                    return ".bmp";
                case ScreenImageFormat.Gif:
                    return ".gif";
                case ScreenImageFormat.Icon:
                    return ".ico";
                case ScreenImageFormat.Jpeg:
                    return ".jpg";
                case ScreenImageFormat.Png:
                    return ".png";
                default:
                    throw new InvalidOperationException();
            }
        }

        [Obsolete]
        private void RunWorker(SequentialScreenWorkflow activity)
        {
            this.startButton.IsEnabled = false;

            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (o, e) =>
            {
                activity.Start();
            };

            worker.RunWorkerCompleted += (o, e) =>
            {
                this.startButton.IsEnabled = true;
            };

            worker.RunWorkerAsync();
        }
    }
}
