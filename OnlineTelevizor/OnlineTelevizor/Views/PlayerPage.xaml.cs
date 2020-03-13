﻿using LibVLCSharp.Shared;
using OnlineTelevizor.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OnlineTelevizor.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlayerPage : ContentPage
    {
        LibVLC _libVLC = null;
        MediaPlayer _mediaPlayer;
        Media _media = null;
        private string _mediaUrl;
        bool _fullscreen = false;

        public PlayerPage()
        {
            InitializeComponent();

            Core.Initialize();

            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC) { EnableHardwareDecoding = true };

            videoView.MediaPlayer = _mediaPlayer;
        }

        private void ShowAudioOnlyIcon()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                Thread.Sleep(1000); // 1s

                Device.BeginInvokeOnMainThread(() =>
                {
                    if (_mediaPlayer.VideoTrack == -1)
                    {
                        AudioOnlyimage.IsVisible = true;
                        videoView.IsVisible = false;
                    }
                    else
                    {
                        AudioOnlyimage.IsVisible = false;
                        videoView.IsVisible = true;
                    }
                });
            }).Start();
        }

        public void OnDoubleTapped(object sender, EventArgs e)
        {
            if (!_fullscreen)
            {
                MessagingCenter.Send(String.Empty, BaseViewModel.EnableFullScreen);
                _fullscreen = true;
            } else
            {
                MessagingCenter.Send(String.Empty, BaseViewModel.DisableFullScreen);
                _fullscreen = false;
            }
        }

        public bool Playing
        {
            get
            {
                return videoView.MediaPlayer.IsPlaying;
            }
        }

        public void SetMediaUrl(string mediaUrl)
        {
            _mediaUrl = mediaUrl;

            if (Playing)
            {
                videoView.MediaPlayer.Stop();
                _media = new Media(_libVLC, _mediaUrl, FromType.FromLocation);

                videoView.MediaPlayer.Play(_media);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Start();

            if (!_fullscreen)
            {
                OnDoubleTapped(this, null);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            Stop();

            if (_fullscreen)
            {
                OnDoubleTapped(this, null);
            }
        }

        public void Start()
        {
            _media = new Media(_libVLC, _mediaUrl, FromType.FromLocation);
            videoView.MediaPlayer.Play(_media);

            ShowAudioOnlyIcon();
        }

        public void Stop()
        {
            videoView.MediaPlayer.Stop();
        }

        public void Resume()
        {
            if (Playing)
            {
                // workaround for black screen after resume (only audio is playing)
                // TODO: resume video without reinitializing

                if (_mediaPlayer.VideoTrack != -1)
                {
                    Stop();

                    VideoStackLayout.Children.Remove(videoView);
                    VideoStackLayout.Children.Add(videoView);

                    Start();
                }
            }
        }

        private void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
        {
            // go back
            Navigation.PopModalAsync();
        }

        private void SwipeGestureRecognizer_Up(object sender, SwipedEventArgs e)
        {
            int currentVol = _mediaPlayer.Volume / 10;

            if (currentVol != 10)
            {
                currentVol += 1;

                if (currentVol > 10)
                {
                    currentVol = 10;
                }

                _mediaPlayer.Volume = currentVol * 10;
            }

            MessagingCenter.Send($"Hlasitost {_mediaPlayer.Volume}%", BaseViewModel.ToastMessage);
        }

        private void SwipeGestureRecognizer_Down(object sender, SwipedEventArgs e)
        {
            int currentVol = _mediaPlayer.Volume / 10;

            if (currentVol != 0)
            {
                currentVol -= 1;

                if (currentVol < 0)
                {
                    currentVol = 0;
                }

                _mediaPlayer.Volume = currentVol * 10;
            }

            MessagingCenter.Send($"Hlasitost {_mediaPlayer.Volume}%", BaseViewModel.ToastMessage);
        }

    }
}
