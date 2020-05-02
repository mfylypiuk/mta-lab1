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
        private ObservableCollection<string> songs;
        private MediaPlayer mediaPlayer;
        private int currentSongIndex;
        private int selectedSongIndex;
        private PlayerStatus playerStatus;
        private DispatcherTimer timerAudioPlayback;

        public ObservableCollection<string> Songs
        {
            get
            {
                if (songs == null)
                {
                    songs = LoadSongs();
                }

                return songs;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            SongsListBox.ItemsSource = Songs;
            playerStatus = PlayerStatus.None;
            AudioSlider.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(AudioSlider_MouseLeftButtonUp), true);
        }

        private ObservableCollection<string> LoadSongs()
        {
            ObservableCollection<string> _songs = new ObservableCollection<string>();
            string path = "music";
            string[] files = Directory.GetFiles(path, "*.mp3");

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Replace(path + "\\", string.Empty);
                files[i] = files[i].Replace(".mp3", string.Empty);
            }

            foreach (var item in files)
            {
                _songs.Add(item);
            }

            return _songs;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (currentSongIndex - 1 < 0)
            {
                Play(songs.Count - 1);
            }
            else
            {
                Play(currentSongIndex - 1);
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
                mediaPlayer.Play();
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
                mediaPlayer.Pause();
                StartPauseButton.Content = "▶";
                playerStatus = PlayerStatus.Paused;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer == null)
            {
                return;
            }

            if (playerStatus == PlayerStatus.Active || playerStatus == PlayerStatus.Paused)
            {
                mediaPlayer.Stop();
                timerAudioPlayback.Stop();
                StartPauseButton.Content = "▶";
            }

            playerStatus = PlayerStatus.Stoped;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (currentSongIndex + 1 >= songs.Count)
            {
                Play(0);
            }
            else
            {
                Play(currentSongIndex + 1);
            }
        }

        private void Play(int songIndex)
        {
            mediaPlayer.Stop();
            string path = "music\\" + songs[songIndex] + ".mp3";
            mediaPlayer.Open(new Uri(path, UriKind.Relative));
            mediaPlayer.Play();
            playerStatus = PlayerStatus.Active;

            while (!mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                timerAudioPlayback = new DispatcherTimer();
                timerAudioPlayback.Interval = TimeSpan.FromMilliseconds(10);
                timerAudioPlayback.Tick += TimerAudioPlayback_Tick;
                timerAudioPlayback.Start();
            }

            SongTitle.Text = songs[songIndex];
            currentSongIndex = songIndex;
        }

        private void PlayNext()
        {
            if (Repeat.IsChecked.HasValue && Repeat.IsChecked.Value)
            {
                Play(currentSongIndex);
            }
            else
            {
                if (currentSongIndex + 1 >= songs.Count)
                {
                    Play(0);
                }
                else
                {
                    Play(currentSongIndex + 1);
                }
            }
        }

        private void TimerAudioPlayback_Tick(object sender, object e)
        {
            long currentMediaTicks = mediaPlayer.Position.Ticks;
            long totalMediaTicks = mediaPlayer.NaturalDuration.TimeSpan.Ticks;

            if (currentMediaTicks == totalMediaTicks)
            {
                PlayNext();
            }

            CurrentPosition.Text = mediaPlayer.Position.Minutes.ToString("D2") + ":" + mediaPlayer.Position.Seconds.ToString("D2");
            TotalTime.Text = mediaPlayer.NaturalDuration.TimeSpan.Minutes.ToString("D2") + ":" + mediaPlayer.NaturalDuration.TimeSpan.Seconds.ToString("D2");

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
            if (mediaPlayer.NaturalDuration.TimeSpan.Seconds > 0)
            {
                mediaPlayer.Position = TimeSpan.FromSeconds(AudioSlider.Value * mediaPlayer.NaturalDuration.TimeSpan.Seconds);
            }
        }
    }
}
