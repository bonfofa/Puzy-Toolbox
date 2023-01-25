using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AltoHttp;
using System.IO.Compression;
using System.Windows.Media.Effects;
using Microsoft.Win32;

namespace Puzy_ToolBox
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        AppViewer av = new AppViewer { Name = "Test", Description = "Desc" };
        public DownloadManager dm = new DownloadManager { DownloadEvent = (s, e) => { } };
        Dictionary<string, bool> isDownloadingGame = new Dictionary<string, bool>();
        Dictionary<string, bool> isGameOnLibrary = new Dictionary<string, bool>();
        public MainWindow()
        {
            InitializeComponent();

            using (WebClient wc = new WebClient())
            {
                // This is the place where apps/games is magically created
                try
                {
                    if (Edium.Properties.Settings.Default.TopMostProperties == true)
                    {
                        this.Topmost = true;
                        TopMostCheck.IsChecked = true;
                    }
                    else if (Edium.Properties.Settings.Default.TopMostProperties == false)
                    {
                        this.Topmost = false;
                        TopMostCheck.IsChecked = false;
                    }
                    UsernameLabel.Content = Edium.Properties.Settings.Default.Username;
                    Path.Text = Edium.Properties.Settings.Default.DefaultPath;
                    UsernameTex.Content = Edium.Properties.Settings.Default.Username;
                    Usernametextbox.Text = Edium.Properties.Settings.Default.Username;

                    if (Edium.Properties.Settings.Default.Changed == true)
                    {
                        ImageBrush PFP = new ImageBrush();
                        PFP.ImageSource = new BitmapImage(new Uri(Edium.Properties.Settings.Default.ProfilePic));

                        var ImageIMG = new BitmapImage();
                        ImageIMG.BeginInit();
                        ImageIMG.UriSource = new Uri(Edium.Properties.Settings.Default.ProfilePic);
                        ImageIMG.DecodePixelHeight = 200;
                        ImageIMG.EndInit();

                        PfpImg.Source = ImageIMG;
                        PfpRec.Fill = PFP;
                    }

                    string yson = wc.DownloadString("https://pastebin.com/raw/y8NUuXpB");
                    dynamic json2 = JsonConvert.DeserializeObject(yson);

                    foreach (var app in json2)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty("" + app.Category))
                            {

                                CreateLibraryGame(int.Parse("" + app.Type), "" + app.AppName, "" + app.AppIcon, "" + app.Background, "" + app.Banner, "" + app.URL, "" + app.Description, "" + app.ExePath, "" + app.Plataform, "" + app.ActualPlataform);
                            }
                            else
                            {
                                CreateCategory("" + app.Category);
                                CreateLibraryGame(int.Parse("" + app.Type), "" + app.AppName, "" + app.AppIcon, "" + app.Background, "" + app.Banner, "" + app.URL, "" + app.Description, "" + app.ExePath, "" + app.Plataform, "" + app.ActualPlataform);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: \n\n\"" + ex.Message.ToString() + "\"\n\n Report this to one of the devs");
                        }
                    }
                }
                catch (Exception er)
                {

                }
            }
        }
        HttpDownloader httpdownloader;
        Storyboard storyboard = new Storyboard();
        TimeSpan halfsecond = TimeSpan.FromMilliseconds(500);
        TimeSpan second = TimeSpan.FromSeconds(1);
        public class DownloadManager
        {
            public MouseButtonEventHandler DownloadEvent { get; set; }
        }

        public class AppViewer : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            private string gfnstatus;
            private string name;
            private string description;
            public string GfnStatus
            {
                get => gfnstatus;
                set
                {
                    name = value;
                    OnPropertyChanged();
                }
            }
            public string Name
            {
                get => name;
                set
                {
                    name = value;
                    OnPropertyChanged();
                }

            }
            public string Description
            {
                get => description;
                set
                {
                    description = value;
                    OnPropertyChanged();
                }
            }

            public void ChangeInstallEvent(Rectangle ib, DownloadManager dm, MouseButtonEventHandler Event)
            {
                ib.MouseLeftButtonDown -= dm.DownloadEvent;
                dm.DownloadEvent = Event;
                ib.MouseLeftButtonDown += dm.DownloadEvent;
            }

            protected void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public void ObjectShiftPos(DependencyObject Object, Thickness Get, Thickness Set)
        {
            ThicknessAnimation ShiftAnimation = new ThicknessAnimation()
            {
                From = Get,
                To = Set,
                Duration = second,
                EasingFunction = Smooth,
            };
            Storyboard.SetTarget(ShiftAnimation, Object);
            Storyboard.SetTargetProperty(ShiftAnimation, new PropertyPath(MarginProperty));
            storyboard.Children.Add(ShiftAnimation);
            storyboard.Begin();
        }


        private IEasingFunction Smooth
        {
            get;
            set;
        }
       = new QuarticEase
       {
           EasingMode = EasingMode.EaseInOut
       };

        private void TopDrag_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        int downloads;

        private void GamesLBL_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SeparadorGames.Opacity == 0)
            {
                ((Storyboard)FindResource("fadein")).Begin(SeparadorGames);
            }

            Account.Opacity = 0;
            Account.IsHitTestVisible = false;

            Config.Opacity = 0;
            Config.IsHitTestVisible = false;

            Library.Opacity = 0;
            Library.IsHitTestVisible = false;

            Home.Opacity = 1;
            Home.IsHitTestVisible = true;

            Downloads.Opacity = 0;
            Downloads.IsHitTestVisible = false;
            ObjectShiftPos(SeparadorGames, SeparadorGames.Margin, new Thickness(81, 38, 0, 0));
        }

        private void BibliotecaLBL_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SeparadorGames.Opacity == 0)
            {
                ((Storyboard)FindResource("fadein")).Begin(SeparadorGames);
            }

            Account.Opacity = 0;
            Account.IsHitTestVisible = false;

            Config.Opacity = 0;
            Config.IsHitTestVisible = false;

            Library.Opacity = 1;
            Library.IsHitTestVisible = true;

            Home.Opacity = 0;
            Home.IsHitTestVisible = false;

            Downloads.Opacity = 0;
            Downloads.IsHitTestVisible = false;
            ObjectShiftPos(SeparadorGames, SeparadorGames.Margin, new Thickness(179, 38, 0, 0));
        }

        private void ConfigLBL_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SeparadorGames.Opacity == 0)
            {
                ((Storyboard)FindResource("fadein")).Begin(SeparadorGames);
            }
            Account.Opacity = 0;
            Account.IsHitTestVisible = false;

            Config.Opacity = 1;
            Config.IsHitTestVisible = true;

            Library.Opacity = 0;
            Library.IsHitTestVisible = false;

            Home.Opacity = 0;
            Home.IsHitTestVisible = false;

            Downloads.Opacity = 0;
            Downloads.IsHitTestVisible = false;
        }

        #region DownloadRequest
        private void DownloadRequest(int Type, string Name, string Icon, string URL, string ExePath)
        {
            Directory.CreateDirectory(Edium.Properties.Settings.Default.DefaultPath);
            if (File.Exists(Edium.Properties.Settings.Default.DefaultPath + (Type == 0 ? ExePath : $"\\{Name}{ExePath}")))
            {
                Process.Start(Edium.Properties.Settings.Default.DefaultPath + (Type == 0 ? ExePath : $"\\{Name}{ExePath}"));
            }
            else
            {
                if (isDownloadingGame[Name + URL] == false)
                {
                    if (NoDownloadGrid.Width == 0 == false)
                    {
                        NoDownloadGrid.Opacity = 0;
                        NoDownloadGrid.Width = 0;
                        NoDownloadGrid.Height = 0;
                        DockPanel.Height = 0;
                    }
                    isDownloadingGame[Name + URL] = true;
                    downloads++;
                    DownloadsCount.Opacity = 1;
                    DownloadLabel.Content = downloads.ToString();
                    DockPanel.Height += 140;
                    #region createdownloadlistactully

                    Grid GameGrid = new Grid
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Height = 140,
                        VerticalAlignment = VerticalAlignment.Top,
                        Width = 940
                    };

                    Rectangle GameBKRec = new Rectangle
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Height = 120,
                        VerticalAlignment = VerticalAlignment.Top,
                        Width = 1002,
                        Margin = new Thickness(-62, 0, 0, 0),
                        RadiusX = 42,
                        RadiusY = 42,
                        Fill = new SolidColorBrush(Color.FromRgb(33, 33, 33))
                    };

                    ImageBrush IconIB = new ImageBrush();
                    IconIB.ImageSource = new BitmapImage(new Uri(Icon));

                    Rectangle GameBanner = new Rectangle
                    {
                        Fill = IconIB,
                        Opacity = 0.6,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Height = 120,
                        VerticalAlignment = VerticalAlignment.Top,
                        Width = 458,
                        RadiusY = 46,
                        RadiusX = 46,
                        Margin = new Thickness(-40, 0, 0, 0)
                    };

                    GameBanner.Effect = new DropShadowEffect
                    {
                        BlurRadius = 50,
                        Color = new Color { A = 255, R = 0, G = 0, B = 0 },
                        Direction = 315,
                        ShadowDepth = 5,
                        Opacity = 0.6
                    };

                    Label GameName = new Label
                    {
                        Content = Name,
                        FontFamily = new FontFamily("Segoe UI Variable Display Light"),
                        FontSize = 19,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(440, 14, 0, 0),
                        Width = 226,
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                    };

                    Label DWSpeed = new Label
                    {
                        FontFamily = new FontFamily("Segoe UI Variable Display Light"),
                        Content = "0 MB/s",
                        FontSize = 17,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        Margin = new Thickness(440, 86, 0, 0)
                    };

                    Label DownloadedMB = new Label
                    {
                        FontFamily = new FontFamily("Segoe UI Variable Display Light"),
                        Content = "0 MB",
                        FontSize = 17,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        Margin = new Thickness(540, 86, 0, 0)
                    };

                    ProgressBar ProgressDW = new ProgressBar
                    {
                        BorderBrush = new SolidColorBrush(Colors.Transparent),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Height = 12,
                        VerticalAlignment = VerticalAlignment.Top,
                        Width = 226,
                        Margin = new Thickness(450, 58, 0, 0),
                        Background = new SolidColorBrush(Color.FromRgb(18, 18, 18))
                    };

                    Grid StartButtonGrid = new Grid
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Height = 57,
                        VerticalAlignment = VerticalAlignment.Top,
                        Width = 182,
                        Margin = new Thickness(709, 31, 0, 0)
                    };

                    LinearGradientBrush StartButtonRecGradient = new LinearGradientBrush();
                    StartButtonRecGradient.StartPoint = new Point(0.5, 0);
                    StartButtonRecGradient.EndPoint = new Point(0.5, 1);
                    StartButtonRecGradient.GradientStops.Add(new GradientStop(Color.FromRgb(111, 203, 119), 1));
                    StartButtonRecGradient.GradientStops.Add(new GradientStop(Color.FromRgb(0, 141, 127), 0));

                    Rectangle StartButtonRec = new Rectangle
                    {
                        IsHitTestVisible = false,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Height = 57,
                        RadiusY = 20,
                        RadiusX = 20,
                        VerticalAlignment = VerticalAlignment.Top,
                        Width = 182,
                        Fill = StartButtonRecGradient
                    };

                    StartButtonRec.MouseLeave += (s, e) =>
                    {
                        StartButtonRec.Opacity = 1;
                    };

                    StartButtonRec.MouseEnter += (s, e) =>
                    {
                        StartButtonRec.Opacity = 0.5;
                    };

                    StartButtonRec.MouseLeftButtonDown += (s, e) =>
                    {
                        if (File.Exists(Edium.Properties.Settings.Default.DefaultPath + (Type == 0 ? ExePath : $"\\{Name}{ExePath}")))
                        {
                            Process.Start(Edium.Properties.Settings.Default.DefaultPath + (Type == 0 ? ExePath : $"\\{Name}{ExePath}"));
                        }
                    };

                    Label StartLabel = new Label
                    {
                        IsHitTestVisible = false,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Content = "Installing",
                        FontSize = 18,
                        Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Height = 57,
                        Width = 182
                    };

                    DockPanel.Children.Add(GameGrid);
                    DockPanel.SetDock(GameGrid, Dock.Top);
                    StartButtonGrid.Children.Add(StartButtonRec);
                    StartButtonGrid.Children.Add(StartLabel);
                    GameGrid.Children.Add(GameBKRec);
                    GameGrid.Children.Add(GameBanner);
                    GameGrid.Children.Add(GameName);
                    GameGrid.Children.Add(DWSpeed);
                    GameGrid.Children.Add(DownloadedMB);
                    GameGrid.Children.Add(ProgressDW);
                    GameGrid.Children.Add(StartButtonGrid);
                    #endregion

                    #region Download
                    httpdownloader = new HttpDownloader(URL, Edium.Properties.Settings.Default.DefaultPath + (Type == 0 ? ExePath : $"\\{Name}.zip"));
                    httpdownloader.DownloadCompleted += (g, c) =>
                    {
                        try
                        {
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                downloads--;
                                DownloadLabel.Content = downloads.ToString();
                                if (downloads.ToString().Contains("0"))
                                {
                                    DownloadsCount.Opacity = 0;
                                }
                                if (Type == 1)
                                {
                                    if (!Directory.Exists(Edium.Properties.Settings.Default.DefaultPath + $"\\{Name}"))
                                    {
                                        ZipFile.ExtractToDirectory(Edium.Properties.Settings.Default.DefaultPath + $"\\{Name}.zip", Edium.Properties.Settings.Default.DefaultPath + $"\\{Name}");
                                    }
                                }
                                ProgressDW.Opacity = 0;
                                GameName.Height = 150;
                                StartButtonRec.IsHitTestVisible = true;
                                StartLabel.Content = "Start";
                                DWSpeed.Opacity = 0;
                                DownloadedMB.Opacity = 0;
                                isDownloadingGame[Name + URL] = false;
                            }));
                        }
                        catch (Exception error)
                        {

                        }
                    };
                    httpdownloader.ProgressChanged += (object hender, AltoHttp.ProgressChangedEventArgs l) =>
                    {
                        ProgressDW.Value = (int)l.Progress;
                        DWSpeed.Content = string.Format("{0} MB/s", (l.SpeedInBytes / 1024d / 1024d).ToString("0.00"));
                        DownloadedMB.Content = string.Format("{0} MB", (httpdownloader.TotalBytesReceived / 1024d / 1024d).ToString("0.00"));
                    };
                    httpdownloader.Start();
                    #endregion
                }
                else
                {


                }
            }
        }
        #endregion


        #region Create Game
        int After4GamesReset;
        private void CreateGame(int Type, string Name, string Icon, string Background, string Banner, string URL, string Description, string ExePath, string Plataform, string ActualPlataform)
        {
            if (iDontHaveAnyGames.Width == 0 == false)
            {
                iDontHaveAnyGames.Width = 0;
                iDontHaveAnyGames.Height = 0;
                iDontHaveAnyGames.Opacity = 0;
                iDontHaveAnyGames.IsHitTestVisible = false;
            }

            try
            {
                isGameOnLibrary.Add(Name, true);
            }
            catch (Exception er)
            {

            }
            if (isGameOnLibrary[Name] == true)
            {
                isGameOnLibrary[Name] = false;
                try
                {
                    isDownloadingGame.Add(Name + URL, false);
                }
                catch (Exception caso)
                {

                }

                if (!After4GamesReset.ToString().Contains("4"))
                {
                    After4GamesReset++;
                }
                else
                {
                    After4GamesReset = 0;
                    Wrap.Height += 180;
                }

                ImageBrush BackgroundImage = new ImageBrush();
                BackgroundImage.ImageSource = new BitmapImage(new Uri(Background));

                ImageBrush BannerImg = new ImageBrush();
                BannerImg.ImageSource = new BitmapImage(new Uri(Banner));

                Grid GameGrid = new Grid
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Height = 151,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 250
                };

                Grid MouseHoverState = new Grid
                {
                    Opacity = 0,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Height = 140,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 236
                };

                GameGrid.MouseEnter += (s, e) =>
                {
                    ((Storyboard)FindResource("fadein")).Begin(MouseHoverState);
                };

                GameGrid.MouseLeave += (s, e) =>
                {
                    ((Storyboard)FindResource("fadeout")).Begin(MouseHoverState);
                };

                Rectangle GameImagePicture = new Rectangle
                {
                    Fill = BackgroundImage,
                    IsHitTestVisible = false,
                    Opacity = 0.5,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Height = 141,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 236,
                    RadiusX = 19,
                    RadiusY = 19
                };

                LinearGradientBrush ShadowGradientRec = new LinearGradientBrush();
                ShadowGradientRec.StartPoint = new Point(0.5, 0);
                ShadowGradientRec.EndPoint = new Point(0.5, 1);
                ShadowGradientRec.GradientStops.Add(new GradientStop(Colors.Gray, 1));
                ShadowGradientRec.GradientStops.Add(new GradientStop(Colors.Transparent, 0));

                Rectangle ShadowRec = new Rectangle
                {
                    Opacity = 0.3,
                    Fill = ShadowGradientRec,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Height = 141,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 236,
                    RadiusY = 19,
                    RadiusX = 19
                };

                Image PlataformIMG = new Image
                {
                    IsHitTestVisible = false,
                    Opacity = 0.5,
                    Height = 17,
                    VerticalAlignment = VerticalAlignment.Top,
                    Source = new BitmapImage(new Uri(Plataform)),
                    Margin = new Thickness(202, 119, 29, 0)
                };

                Label GameName = new Label
                {
                    IsHitTestVisible = false,
                    Content = Name,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#ededed"),
                    FontFamily = new FontFamily("Segoe UI Variable Display Light"),
                    FontSize = 9,
                    Margin = new Thickness(10, 118, 0, 0)
                };

                LinearGradientBrush GradientHoberBK = new LinearGradientBrush();
                GradientHoberBK.StartPoint = new Point(0.5, 0);
                GradientHoberBK.EndPoint = new Point(0.5, 1);
                GradientHoberBK.GradientStops.Add(new GradientStop(Color.FromRgb(63, 74, 93), 1));
                GradientHoberBK.GradientStops.Add(new GradientStop(Color.FromArgb(5, 151, 175, 226), 0));

                Rectangle HoverBackGround = new Rectangle
                {
                    Opacity = 1,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Height = 140,
                    RadiusY = 19,
                    RadiusX = 19,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 236,
                    Fill = GradientHoberBK
                };

                Label GameNameHover = new Label
                {
                    FontSize = 12,
                    IsHitTestVisible = false,
                    Content = Name,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#ededed"),
                    Margin = new Thickness(8, 6, 0, 0)
                };

                TextBlock MiniDesc = new TextBlock
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    TextWrapping = TextWrapping.Wrap,
                    Text = Description,
                    VerticalAlignment = VerticalAlignment.Top,
                    FontSize = 10,
                    Height = 66,
                    Width = 216,
                    Margin = new Thickness(10, 32, 0, 0),
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#ededed")
                };

                Image PlataformIMGHover = new Image
                {
                    IsHitTestVisible = false,
                    Opacity = 0.5,
                    Height = 17,
                    VerticalAlignment = VerticalAlignment.Top,
                    Source = new BitmapImage(new Uri(Plataform)),
                    Margin = new Thickness(204, 12, 13, 0)
                };

                Grid PlayButton = new Grid
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Height = 32,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 75,
                    Margin = new Thickness(19, 98, 0, 0),
                };

                LinearGradientBrush PlayButtonRecGradient = new LinearGradientBrush();
                PlayButtonRecGradient.StartPoint = new Point(0.5, 0);
                PlayButtonRecGradient.EndPoint = new Point(0.5, 1);
                PlayButtonRecGradient.GradientStops.Add(new GradientStop(Color.FromRgb(37, 234, 234), 0));
                PlayButtonRecGradient.GradientStops.Add(new GradientStop(Color.FromRgb(0, 118, 107), 1));

                Rectangle PlayButtonRec = new Rectangle
                {
                    Fill = PlayButtonRecGradient,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Height = 32,
                    RadiusY = 6,
                    RadiusX = 6,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 75
                };

                PlayButtonRec.MouseEnter += (s, e) =>
                {
                    PlayButtonRec.Opacity = 0.5;
                };

                PlayButtonRec.MouseLeave += (s, e) =>
                {
                    PlayButtonRec.Opacity = 1;
                };

                PlayButtonRec.MouseLeftButtonDown += (s, e) =>
                {
                    DownloadRequest(Type, Name, Banner, URL, ExePath);
                };

                Label PlayLabel = new Label
                {
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    IsHitTestVisible = false,
                    Content = "Play",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Height = 32,
                    Width = 75,
                    FontSize = 11
                };

                Grid InfoBTN = new Grid
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Height = 32,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 75,
                    Margin = new Thickness(142, 98, 0, 0)
                };

                LinearGradientBrush InfoLBLGradient = new LinearGradientBrush();
                InfoLBLGradient.StartPoint = new Point(0.5, 0);
                InfoLBLGradient.EndPoint = new Point(0.5, 1);
                InfoLBLGradient.GradientStops.Add(new GradientStop(Color.FromRgb(89, 89, 89), 0));
                InfoLBLGradient.GradientStops.Add(new GradientStop(Color.FromRgb(14, 14, 4), 1));

                Rectangle InfoBTNRec = new Rectangle
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Height = 32,
                    RadiusY = 6,
                    RadiusX = 6,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 75,
                    Fill = InfoLBLGradient
                };

                InfoBTNRec.MouseEnter += (s, e) =>
                {
                    InfoBTNRec.Opacity = 0.5;
                };

                InfoBTNRec.MouseLeave += (s, e) =>
                {
                    InfoBTNRec.Opacity = 1;
                };

                MouseButtonEventHandler ib = (se, ev) =>
                {
                    DownloadRequest(Type, Name, Banner, URL, ExePath);
                };

                InfoBTNRec.MouseLeftButtonDown += (s, e) =>
                {
                    av.ChangeInstallEvent(PlayBTNMenu1, dm, ib);
                    DescriptionTex.Text = Description;
                    GameTitle.Content = Name;
                    BannerD.Fill = BannerImg;

                    GamesInfo.Opacity = 1;
                    GamesInfo.IsHitTestVisible = true;

                    GamesLibrary.IsHitTestVisible = false;
                    GamesLibrary.Opacity = 0;
                };

                Label InfoLBL = new Label
                {
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    IsHitTestVisible = false,
                    Content = "Info",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Height = 32,
                    Width = 75,
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#ededed"),
                    FontSize = 11
                };

                InfoBTN.Children.Add(InfoBTNRec);
                InfoBTN.Children.Add(InfoLBL);
                PlayButton.Children.Add(PlayButtonRec);
                PlayButton.Children.Add(PlayLabel);
                MouseHoverState.Children.Add(PlataformIMGHover);
                MouseHoverState.Children.Add(MiniDesc);
                MouseHoverState.Children.Add(GameNameHover);
                MouseHoverState.Children.Add(HoverBackGround);
                MouseHoverState.Children.Add(InfoBTN);
                MouseHoverState.Children.Add(PlayButton);
                GameGrid.Children.Add(GameName);
                GameGrid.Children.Add(PlataformIMG);
                GameGrid.Children.Add(GameImagePicture);
                GameGrid.Children.Add(ShadowRec);
                GameGrid.Children.Add(MouseHoverState);
                Wrap.Children.Add(GameGrid);
            }
        }
        #endregion

        #region create library
        int After4GamesReset2;

        private void CreateCategory(string Category)
        {
            Label CategoryLBL = new Label
            {
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Bottom,
                Content = Category,
                FontFamily = new FontFamily("Segoe UI Variable Display Light"),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Foreground = (Brush)new BrushConverter().ConvertFrom("#ededed"),
                FontSize = 14,
                Height = 36,
                Width = 1011,
            };
            LibraryWrap.Children.Add(CategoryLBL);
            LibraryWrap.Height += 185;
            After4GamesReset2 = 0;
            DockPanel.SetDock(CategoryLBL, Dock.Top);
        }

        private void CreateLibraryGame(int Type, string Name, string Icon, string Background, string Banner, string URL, string Description, string ExePath, string Plataform, string ActualPlataform)
        {
            if (!After4GamesReset2.ToString().Contains("4"))
            {
                After4GamesReset2++;
            }
            else
            {
                After4GamesReset2 = 0;
                LibraryWrap.Height += 151;
            }

            #region stay/nocategorycode
            ImageBrush BackgroundImage = new ImageBrush();
            BackgroundImage.ImageSource = new BitmapImage(new Uri(Background));

            ImageBrush BannerImg = new ImageBrush();
            BannerImg.ImageSource = new BitmapImage(new Uri(Banner));

            var PlataformImg = new BitmapImage();
            PlataformImg.BeginInit();
            PlataformImg.UriSource = new Uri(Plataform);
            PlataformImg.DecodePixelHeight = 200;
            PlataformImg.EndInit();

            Grid GameGrid = new Grid
            {
                Height = 151,
                Width = 244
            };


            Rectangle GamePic = new Rectangle
            {
                Fill = BackgroundImage,
                HorizontalAlignment = HorizontalAlignment.Left,
                Height = 144,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 238,
                RadiusX = 14,
                RadiusY = 14
            };

            LinearGradientBrush ShadowGradient = new LinearGradientBrush();
            ShadowGradient.StartPoint = new Point(0.5, 0);
            ShadowGradient.EndPoint = new Point(0.5, 1);
            ShadowGradient.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0, 0, 0), 1));
            ShadowGradient.GradientStops.Add(new GradientStop(Color.FromRgb(10, 10, 10), 0));

            Rectangle ShadowRec = new Rectangle
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Height = 144,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 238,
                RadiusX = 14,
                RadiusY = 14,
                Fill = ShadowGradient
            };

            Label GameName = new Label
            {
                Content = Name,
                FontFamily = new FontFamily("Segoe UI Variable Display Light"),
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(4, 110, 0, 0),
                Width = 201,
                Foreground = (Brush)new BrushConverter().ConvertFrom("#ededed"),
                FontSize = 14
            };

            Image PlataformImageLogo = new Image
            {
                Source = PlataformImg,
                Opacity = 0.6,
                HorizontalAlignment = HorizontalAlignment.Left,
                Height = 25,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 25,
                Margin = new Thickness(205, 112, 0, 0)
            };

            Grid MouseHoverStateGrid = new Grid
            {
                Opacity = 0,
                HorizontalAlignment = HorizontalAlignment.Left,
                Height = 139,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 238
            };

            LinearGradientBrush MouseHoverShadowGradient = new LinearGradientBrush();
            MouseHoverShadowGradient.StartPoint = new Point(0.5, 0);
            MouseHoverShadowGradient.EndPoint = new Point(0.5, 1);
            MouseHoverShadowGradient.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0, 0, 0), 1));
            MouseHoverShadowGradient.GradientStops.Add(new GradientStop(Color.FromRgb(5, 109, 168), 0));

            Rectangle MouseHoverShadowGrid = new Rectangle
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Height = 144,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 238,
                RadiusX = 14,
                RadiusY = 14,
                Margin = new Thickness(0, 0, 0, -5),
                Fill = MouseHoverShadowGradient
            };

            Label GameNameHover = new Label
            {
                Content = Name,
                FontFamily = new FontFamily("Segoe UI Variable Display Light"),
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(4, 0, 0, 0),
                Width = 201,
                Foreground = (Brush)new BrushConverter().ConvertFrom("#ededed"),
                FontSize = 14
            };

            Image PlataformImageLogoHover = new Image
            {
                Source = PlataformImg,
                Opacity = 0.6,
                HorizontalAlignment = HorizontalAlignment.Left,
                Height = 25,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 25,
                Margin = new Thickness(208, 2, 0, 0)
            };

            TextBlock GameDescription = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                TextWrapping = TextWrapping.Wrap,
                Text = Description,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 61,
                FontSize = 9,
                Margin = new Thickness(10, 34, 0, 0),
                Width = 218,
                Foreground = (Brush)new BrushConverter().ConvertFrom("#ededed")
            };

            Grid AddToLibrary = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Height = 29,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 115,
                Margin = new Thickness(64, 104, 0, 0)
            };

            AddToLibrary.MouseEnter += (s, e) =>
            {
                AddToLibrary.Opacity = 0.5;
            };

            AddToLibrary.MouseLeave += (s, e) =>
            {
                AddToLibrary.Opacity = 1;
            };

            AddToLibrary.MouseLeftButtonDown += (s, e) =>
            {
                CreateGame(Type, Name, Icon, Background, Banner, URL, Description, ExePath, Plataform, ActualPlataform);
            };

            LinearGradientBrush AddLibraryRecGradient = new LinearGradientBrush();
            AddLibraryRecGradient.StartPoint = new Point(0.5, 0);
            AddLibraryRecGradient.EndPoint = new Point(0.5, 1);
            AddLibraryRecGradient.GradientStops.Add(new GradientStop(Color.FromArgb(95, 0, 220, 190), 1));
            AddLibraryRecGradient.GradientStops.Add(new GradientStop(Color.FromRgb(5, 168, 64), 0));
            Rectangle AddLibraryGradientRec = new Rectangle
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Height = 29,
                RadiusY = 11,
                RadiusX = 11,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 115,
                Fill = AddLibraryRecGradient
            };

            Label AddLibraryText = new Label
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                FontSize = 10,
                Content = "Add to library",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 29,
                Width = 115,
                FontFamily = new FontFamily("Segoe UI Semilight"),
            };

            MouseHoverStateGrid.MouseEnter += (s, e) =>
            {
                ((Storyboard)FindResource("fadein")).Begin(MouseHoverStateGrid);
            };

            MouseHoverStateGrid.MouseLeave += (s, e) =>
            {
                ((Storyboard)FindResource("fadeout")).Begin(MouseHoverStateGrid);
            };

            Grid SeparateGamesGrid = new Grid
            {
                Width = 1009,
                Height = 182
            };

            AddToLibrary.Children.Add(AddLibraryGradientRec);
            AddToLibrary.Children.Add(AddLibraryText);
            MouseHoverStateGrid.Children.Add(MouseHoverShadowGrid);
            MouseHoverStateGrid.Children.Add(GameNameHover);
            MouseHoverStateGrid.Children.Add(PlataformImageLogoHover);
            MouseHoverStateGrid.Children.Add(GameDescription);
            MouseHoverStateGrid.Children.Add(AddToLibrary);
            GameGrid.Children.Add(GamePic);
            GameGrid.Children.Add(ShadowRec);
            GameGrid.Children.Add(GameName);
            GameGrid.Children.Add(PlataformImageLogo);
            GameGrid.Children.Add(MouseHoverStateGrid);
            LibraryWrap.Children.Add(GameGrid);
            #endregion
        }
        #endregion

        private void PlayBTNMenu1_MouseEnter(object sender, MouseEventArgs e)
        {
            PlayBTNMenu1.Opacity = 0.5;
        }

        private void PlayBTNMenu1_MouseLeave(object sender, MouseEventArgs e)
        {
            PlayBTNMenu1.Opacity = 1;
        }

        private void GoBackMenu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GamesInfo.Opacity = 0;
            GamesInfo.IsHitTestVisible = false;

            GamesLibrary.IsHitTestVisible = true;
            GamesLibrary.Opacity = 1;
        }

        private void DownloadRec_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Account.Opacity = 0;
            Account.IsHitTestVisible = false;

            Config.Opacity = 0;
            Config.IsHitTestVisible = false;

            Library.Opacity = 0;
            Library.IsHitTestVisible = false;

            Home.Opacity = 0;
            Home.IsHitTestVisible = false;

            Downloads.Opacity = 1;
            Downloads.IsHitTestVisible = true;

            if (SeparadorGames.Opacity == 0 == false)
            {
                ((Storyboard)FindResource("fadeout")).Begin(SeparadorGames);
            }
        }

    

        private void ApplyButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ApplyButton.Opacity = 0.5;
        }

        private void ApplyButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ApplyButton.Opacity = 1;
        }

        bool isEditing = false;
        private void ApplyButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isEditing == true)
            {
                isEditing = false;

                Usernametextbox.Opacity = 0;
                Usernametextbox.IsHitTestVisible = false;

                UsernameLabel.Opacity = 1;
                UsernameLabel.IsHitTestVisible = true;

                BrowseBTN.Opacity = 0;
                BrowseBTN.IsHitTestVisible = false;

                UsernameLabel.Content = Usernametextbox.Text;
                UsernameTex.Content = Usernametextbox.Text;
                Edium.Properties.Settings.Default.Username = Usernametextbox.Text;
                Edium.Properties.Settings.Default.Save();
                ApplyButtonLabel.Content = "Edit";
            }
            else
            {
                isEditing = true;

                Usernametextbox.Opacity = 1;
                Usernametextbox.IsHitTestVisible = true;

                BrowseBTN.Opacity = 1;
                BrowseBTN.IsHitTestVisible = true;

                UsernameLabel.Opacity = 0;
                UsernameLabel.IsHitTestVisible = false;

                ApplyButtonLabel.Content = "Apply";
            }
        }

        private void BrowsePfpGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            PfpRec.Opacity = 0.5;
            BrowsePfpGrid.Opacity = 1;
        }

        private void BrowsePfpGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            PfpRec.Opacity = 1;
            BrowsePfpGrid.Opacity = 0.5;
        }

        private void BrowsePfpGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                Edium.Properties.Settings.Default.Changed = true;
                var ImageIMG = new BitmapImage();
                ImageIMG.BeginInit();
                ImageIMG.UriSource = new Uri(openFileDialog.FileName);
                ImageIMG.DecodePixelHeight = 200;
                ImageIMG.EndInit();

                ImageBrush RecsIMG = new ImageBrush();
                RecsIMG.ImageSource = new BitmapImage(new Uri(openFileDialog.FileName));

                Edium.Properties.Settings.Default.ProfilePic = openFileDialog.FileName.ToString();
                Edium.Properties.Settings.Default.Save();

                PfpImg.Source = ImageIMG;
                PfpRec.Fill = RecsIMG;
            }
        }

        private void TopMostCheck_Checked(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            Edium.Properties.Settings.Default.TopMostProperties = true;
            Edium.Properties.Settings.Default.Save();
        }

        private void TopMostCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Topmost = false;
            Edium.Properties.Settings.Default.TopMostProperties = false;
            Edium.Properties.Settings.Default.Save();
        }

        private void Path_TextChanged(object sender, TextChangedEventArgs e)
        {
            Edium.Properties.Settings.Default.DefaultPath = Path.Text;
            Edium.Properties.Settings.Default.Save();
        }

        private void UserSet_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Account.Opacity = 1;
            Account.IsHitTestVisible = true;

            Config.Opacity = 0;
            Config.IsHitTestVisible = false;

            Library.Opacity = 0;
            Library.IsHitTestVisible = false;

            Home.Opacity = 0;
            Home.IsHitTestVisible = false;

            Downloads.Opacity = 0;
            Downloads.IsHitTestVisible = false;
            if (SeparadorGames.Opacity == 0 == false)
            {
                ((Storyboard)FindResource("fadeout")).Begin(SeparadorGames);
            }
        }

        private void PfpRec_MouseEnter(object sender, MouseEventArgs e)
        {
            PfpRec.Opacity = 0.5;
        }

        private void PfpRec_MouseLeave(object sender, MouseEventArgs e)
        {
            PfpRec.Opacity = 1;
        }
    }
}
