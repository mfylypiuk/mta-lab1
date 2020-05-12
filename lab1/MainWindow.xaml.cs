using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace lab1
{
    enum PlayerType
    {
        Audio,
        Video
    }

    enum PlayerStatus
    {
        None,
        Active,
        Paused,
        Stoped
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> files;
        private MediaPlayer mediaPlayer;
        private VideoPlayer videoPlayer;
        private int currentFileIndex;
        private int selectedFileIndex;
        private PlayerStatus playerStatus;
        private PlayerType playerType;
        private DispatcherTimer timerPlayback;

        public ObservableCollection<string> Files
        {
            get
            {
                if (files == null)
                {
                    files = LoadFiles();
                }

                return files;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            SongsListBox.ItemsSource = Files;
            playerStatus = PlayerStatus.None;
            StartPauseButton.IsEnabled = false;
            AudioSlider.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(AudioSlider_MouseLeftButtonUp), true);
        }

        private ObservableCollection<string> LoadFiles()
        {
            ObservableCollection<string> _files = new ObservableCollection<string>();
            string path = "files";
            string[] files = Directory.GetFiles(path, "*.*").Where(file => file.EndsWith(".mp3") || file.EndsWith(".mp4")).ToArray();

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Replace(path + "\\", string.Empty);
            }

            foreach (var item in files)
            {
                _files.Add(item);
            }

            return _files;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (currentFileIndex - 1 < 0)
            {
                Play(files.Count - 1);
            }
            else
            {
                Play(currentFileIndex - 1);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer == null)
            {
                mediaPlayer = new MediaPlayer();
            }

            if (playerStatus == PlayerStatus.Paused)
            {
                if (playerType == PlayerType.Audio)
                {
                    mediaPlayer.Play();
                }
                else if (playerType == PlayerType.Video)
                {
                    videoPlayer.MediaElement.Play();
                }

                StartPauseButton.Content = "▶";
                playerStatus = PlayerStatus.Active;
            }
            else if (playerStatus == PlayerStatus.None || playerStatus == PlayerStatus.Stoped || playerStatus == PlayerStatus.Active)
            {
                if (SongsListBox.SelectedIndex == -1)
                {
                    Play(0);
                }
                else
                {
                    Play(SongsListBox.SelectedIndex);
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (playerStatus == PlayerStatus.Active)
            {
                if (playerType == PlayerType.Audio)
                {
                    mediaPlayer.Pause();
                }
                else if (playerType == PlayerType.Video)
                {
                    videoPlayer.MediaElement.Pause();
                }

                StartPauseButton.Content = "▶";
                playerStatus = PlayerStatus.Paused;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer == null && videoPlayer == null)
            {
                return;
            }

            if (playerStatus == PlayerStatus.Active || playerStatus == PlayerStatus.Paused)
            {
                if (playerType == PlayerType.Audio)
                {
                    mediaPlayer.Stop();
                }
                else if (playerType == PlayerType.Video)
                {
                    videoPlayer.MediaElement.Stop();
                    videoPlayer.Close();
                }

                timerPlayback.Stop();
                StartPauseButton.Content = "▶";
            }

            playerStatus = PlayerStatus.Stoped;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (currentFileIndex + 1 >= files.Count)
            {
                Play(0);
            }
            else
            {
                Play(currentFileIndex + 1);
            }
        }

        private void Play(int fileIndex)
        {
            string fileName = files[fileIndex];
            string path = "files\\" + files[fileIndex];

            if (System.IO.Path.GetExtension(fileName) == ".mp3")
            {
                if (videoPlayer != null)
                {
                    videoPlayer.MediaElement.Stop();
                    videoPlayer.Close();
                    videoPlayer = null;
                }

                mediaPlayer.Stop();
                mediaPlayer.Open(new Uri(path, UriKind.Relative));
                mediaPlayer.Play();
                playerType = PlayerType.Audio;

                while (!mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    CreateAndStartTimer();
                }
            }
            else if (System.IO.Path.GetExtension(fileName) == ".mp4")
            {
                if (mediaPlayer != null)
                {
                    mediaPlayer.Stop();
                }

                videoPlayer = new VideoPlayer();
                videoPlayer.Show();
                videoPlayer.MediaElement.Source = new Uri(path, UriKind.Relative);
                videoPlayer.MediaElement.Play();
                playerType = PlayerType.Video;

                while (!videoPlayer.MediaElement.NaturalDuration.HasTimeSpan)
                {
                    CreateAndStartTimer();
                }
            }

            Title.Text = fileName;
            currentFileIndex = fileIndex;
            playerStatus = PlayerStatus.Active;
        }

        private void CreateAndStartTimer()
        {
            timerPlayback = new DispatcherTimer();
            timerPlayback.Interval = TimeSpan.FromMilliseconds(10);
            timerPlayback.Tick += TimerPlayback_Tick;
            timerPlayback.Start();
        }

        private void PlayNext()
        {
            if (Repeat.IsChecked.HasValue && Repeat.IsChecked.Value)
            {
                Play(currentFileIndex);
            }
            else
            {
                if (currentFileIndex + 1 >= files.Count)
                {
                    Play(0);
                }
                else
                {
                    Play(currentFileIndex + 1);
                }
            }
        }

        private void TimerPlayback_Tick(object sender, object e)
        {
            long currentMediaTicks = 0;
            long totalMediaTicks = 0;

            if (playerType == PlayerType.Audio)
            {
                currentMediaTicks = mediaPlayer.Position.Ticks;
                totalMediaTicks = mediaPlayer.NaturalDuration.TimeSpan.Ticks;
            } 
            else if (playerType == PlayerType.Video)
            {
                if (videoPlayer.MediaElement.NaturalDuration.HasTimeSpan)
                {
                    currentMediaTicks = videoPlayer.MediaElement.Position.Ticks;
                    totalMediaTicks = videoPlayer.MediaElement.NaturalDuration.TimeSpan.Ticks;
                }
            }

            if (currentMediaTicks == totalMediaTicks)
            {
                PlayNext();
            }

            if (playerType == PlayerType.Audio)
            {
                CurrentPosition.Text = mediaPlayer.Position.Minutes.ToString("D2") + ":" + mediaPlayer.Position.Seconds.ToString("D2");
                TotalTime.Text = mediaPlayer.NaturalDuration.TimeSpan.Minutes.ToString("D2") + ":" + mediaPlayer.NaturalDuration.TimeSpan.Seconds.ToString("D2");
            }
            else if (playerType == PlayerType.Video)
            {
                CurrentPosition.Text = videoPlayer.MediaElement.Position.Minutes.ToString("D2") + ":" + videoPlayer.MediaElement.Position.Seconds.ToString("D2");
                TotalTime.Text = videoPlayer.MediaElement.NaturalDuration.TimeSpan.Minutes.ToString("D2") + ":" + videoPlayer.MediaElement.NaturalDuration.TimeSpan.Seconds.ToString("D2");
            }

            if (totalMediaTicks > 0)
            {
                AudioSlider.Value = (double)currentMediaTicks / totalMediaTicks * 10;
            }
            else
            {
                AudioSlider.Value = 0;
            }
        }

        private void AudioSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (playerType == PlayerType.Audio)
            {
                if (mediaPlayer.NaturalDuration.TimeSpan.Seconds > 0)
                {
                    mediaPlayer.Position = TimeSpan.FromSeconds(AudioSlider.Value * mediaPlayer.NaturalDuration.TimeSpan.Seconds);
                }
            }
            else if (playerType == PlayerType.Video)
            {
                if (videoPlayer.MediaElement.NaturalDuration.TimeSpan.Seconds > 0)
                {
                    videoPlayer.MediaElement.Position = TimeSpan.FromSeconds(AudioSlider.Value * videoPlayer.MediaElement.NaturalDuration.TimeSpan.Seconds);
                }
            }
        }

        private void SongsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StartPauseButton.IsEnabled = true;
        }
    }
}
