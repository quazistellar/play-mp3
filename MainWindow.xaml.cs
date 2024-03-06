using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace play_mp3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private int play_now_ind = -1;
        private bool isPlay = false;
        private bool isRepeateMod = false;
        private bool isChange_polzynok = false;
        List<FileInfo> files = new List<FileInfo>();
        public List<FileInfo> listenedSongs = new List<FileInfo>();
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            Title = "play.mp3";
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(UpdateTimerCallback);
        }
        private void UpdateTimerCallback(object sender, EventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                TimelineSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                TimelineSlider.Value = mediaPlayer.Position.TotalSeconds;
                CurrentTime.Text = mediaPlayer.Position.ToString(@"hh\:mm\:ss");
                RemainingTime.Text = (mediaPlayer.NaturalDuration.TimeSpan - mediaPlayer.Position).ToString(@"hh\:mm\:ss");

                //шедеврокостыль
                if (RemainingTime.Text == "00:00:00")
                {
                    if (isRepeateMod == false)
                    {
                        PlayNextTrack();
                    }
                    else if (isRepeateMod == true)
                    {
                        RestartSong();
                    }
                }
            }
        }

        private void open_music_files_Click(object sender, RoutedEventArgs e)

        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true, Title = "Choose directory with your music!" };

            var result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {

                string folderPath = dialog.FileName;
                DirectoryInfo directory = new DirectoryInfo(folderPath);

                var sortedFiles = directory.GetFiles("*.*", SearchOption.AllDirectories)
                               .Where(f => f.Extension.ToLower() == ".mp3" ||
                                           f.Extension.ToLower() == ".wav" ||
                                           f.Extension.ToLower() == ".wma" ||
                                           f.Extension.ToLower() == ".ogg" ||
                                           f.Extension.ToLower() == ".m4a" ||
                                           f.Extension.ToLower() == ".mpga")
                               .OrderBy(f => f.Name); 

                files = sortedFiles.ToList(); 

                music.ItemsSource = files;

                if (files.Count > 0)
                {
                    PlayMusic(0);
                }
            

            }
        }


        private void PlayMusic(int index)
        {

            TimelineSlider.ValueChanged += TimelineSlider_ValueChanged;

            if (index >= 0 && index < files.Count)
            {
                string filePath = files[index].FullName;
                mediaPlayer.Open(new Uri(filePath));
                mediaPlayer.Play();
                isPlay = true;
                timer.Start();
                FileInfo fileInfo = new FileInfo(filePath);
                Dispatcher.Invoke(() =>
                {
                    listenedSongs.Add(fileInfo);

                });

                Task Timeline = Task.Run(() =>
                {
                    while (true)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            if (!isChange_polzynok)
                            {
                                TimelineSlider.Value = mediaPlayer.Position.TotalSeconds;
                            }
                        });
                        Thread.Sleep(1000);
                    }
                });
            }
            Task time_info = Task.Run(() =>
            {
                while (true)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (mediaPlayer.HasAudio)
                        {
                            CurrentTime.Text = mediaPlayer.Position.ToString(@"hh\:mm\:ss");
                            RemainingTime.Text = (mediaPlayer.NaturalDuration.TimeSpan - mediaPlayer.Position).ToString(@"hh\:mm\:ss");
                            TimelineSlider.ValueChanged += TimelineSlider_ValueChanged;
                        }
                    });
                    Thread.Sleep(1000);
                }
            });


            play_now_ind = index;
            isPlay = true;

            TimelineSlider.AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler(TimelineSlider_DragStarted), true);
            TimelineSlider.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(TimelineSlider_DragCompleted), true);

        }

        private void RestartSong()
        {
            TimelineSlider.Value = 0;
            mediaPlayer.Position = new TimeSpan(Convert.ToInt64(TimelineSlider.Value));
        }

        private void TimelineSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            isChange_polzynok = true;
            mediaPlayer.Pause();
        }

        private void TimelineSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            isChange_polzynok = false;
            TimeSpan newPosition = TimeSpan.FromSeconds(TimelineSlider.Value);
            mediaPlayer.Position = newPosition;
            mediaPlayer.Play();
        }


        private void PlayNextTrack()
        {
            int next_ind = play_now_ind + 1;
            if (next_ind < files.Count)
            {
                PlayMusic(next_ind);
            }
            else
            {
                PlayMusic(0);
            }
        }

        private void PlayPrevTrack()
        {
            int prev_ind = play_now_ind - 1;
            if (prev_ind < files.Count)
            {
                PlayMusic(prev_ind);
            }
            else
            {
                PlayMusic(0);
            }
        }
        private void media_MediaOpened(object sender, RoutedEventArgs e)
        {
            TimelineSlider.Maximum = media.NaturalDuration.TimeSpan.TotalSeconds;

        }


        private void play_Click(object sender, RoutedEventArgs e)
        {
            if (isPlay)
            {
                play.Content = "⏸️";
                mediaPlayer.Pause();
                isPlay = false;
            }
            else
            {
                if (play_now_ind == -1)
                {
                    PlayMusic(0);
                }
                else
                {
                    play.Content = "▷";
                    mediaPlayer.Play();
                    isPlay = true;
                }
            }

        }


        private void TimelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isPlay && !isChange_polzynok)
            {
                isChange_polzynok = true;
                media.Position = new TimeSpan(Convert.ToInt64(TimelineSlider.Value));
            }
        }

        private void next_song_Click(object sender, RoutedEventArgs e)
        {
            PlayNextTrack();
        }

        private void back_song_Click(object sender, RoutedEventArgs e)
        {

            if (play_now_ind == 0)
            {
                PlayMusic(0);
            }
            else
            {
                PlayPrevTrack();
            }
        }


        private void again_song_rezhim_Click(object sender, RoutedEventArgs e)
        {
            if (isRepeateMod)
            {
                isRepeateMod = false;
                again_song_rezhim.Content = "again";
            }
            else if (!isRepeateMod)
            {
                isRepeateMod = true;
                again_song_rezhim.Content = "again: on";
            }

        }

        private void change_volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = 0.8;
            mediaPlayer.Volume = change_volume.Value;
        }

        private void show_history_Click(object sender, RoutedEventArgs e)
        {

            History history = new History(listenedSongs);

            if (history.ShowDialog() == true)
            {
                FileInfo selectedFile = history.SelectedFile;

                if (selectedFile != null)
                {
                    PlayMusic(listenedSongs.IndexOf(selectedFile));
                }
            }
        }

        private void shuffle_songs_Click(object sender, RoutedEventArgs e)
        {
            ShufflePlaylist();
        }


        private bool isPeremeshka = false;
        private List<FileInfo> orig = new List<FileInfo>();

        private void ShufflePlaylist()
        {
            if (!isPeremeshka)
            {
                shuffle_songs.Content = "shuffle: on";
                orig.Clear();
                foreach (var file in files)
                {
                    orig.Add(file);
                }

                Random random = new Random();
                for (int i = 0; i < files.Count; i++)
                {
                    int index = random.Next(i, files.Count);
                    var temp = files[i];
                    files[i] = files[index];
                    files[index] = temp;
                }

                isPeremeshka = true;
                PlayMusic(0);
            }
            else
            {
                shuffle_songs.Content = "shuffle";
                files.Clear();
                foreach (var file in orig)
                {
                    files.Add(file);
                }

                isPeremeshka = false;
                PlayMusic(0);
            }
        }

        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            //да треш
        }

        private void music_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            
            if (music.SelectedItem != null)
            {
               FileInfo itemfile = music.SelectedItem as FileInfo;
               int select_song = files.IndexOf(itemfile);
               if (select_song >= 0)
               {
                  PlayMusic(select_song);
               }
            }
            
        }
    }
}