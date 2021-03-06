﻿using System;
using System.Collections.Generic;
using System.Text;
using LoggerService;

namespace OnlineTelevizor.Models
{
    public class DebugConfiguration : IOnlineTelevizorConfiguration
    {
        private TVAPIEnum _tvAPI = TVAPIEnum.SledovaniTV;
        private string _kukiSn = "";
        private string _O2TVUsername = "";
        private string _O2TVPassword = "";
        private string _DVBStreamerUrl = "";
        private string _username ="";
        private string _password = "";
        private string _childLockPIN;
        private string _streamQuality;
        private string _channelFilterGroup;
        private string _channelFilterType;
        private string _channelFilterName;
        private bool _showLocked;
        private bool _showAdultChannels;
        private bool _internalPlayer;
        private bool _fullscreen;
        private bool _playOnBackground;
        private string _autoPlayChannelNumber;
        private string _lastChannelNumber;
        private AppFontSizeEnum _appFontSize = AppFontSizeEnum.Normal;
        private bool _enableLogging = true;
        private bool _doNotSplitScreenOnLandscape = true;
        private LoggingLevelEnum _loggingLevel;
        private bool _purchased = true;
        private bool _dDebugMode = true;
        private string _dDeviceId;
        private string _dDevicePassword;


        public TVAPIEnum TVApi { get => _tvAPI; set => _tvAPI = value; }
        public string KUKIsn { get => _kukiSn; set => _kukiSn = value; }
        public string O2TVUsername { get => _O2TVUsername; set => _O2TVUsername = value; }
        public string O2TVPassword { get => _O2TVPassword; set => _O2TVPassword = value; }
        public string DVBStreamerUrl { get => _DVBStreamerUrl; set => _DVBStreamerUrl = value; }
        public string Username { get => _username; set => _username = value; }
        public string Password { get => _password; set => _password = value; }
        public string ChildLockPIN { get => _childLockPIN; set => _childLockPIN = value; }
        public string StreamQuality { get => _streamQuality; set => _streamQuality = value; }
        public string ChannelFilterGroup { get => _channelFilterGroup; set => _channelFilterGroup = value; }
        public string ChannelFilterType { get => _channelFilterType; set => _channelFilterType = value; }
        public string ChannelFilterName { get => _channelFilterName; set => _channelFilterName = value; }
        public bool ShowLocked { get => _showLocked; set => _showLocked = value; }
        public bool ShowAdultChannels { get => _showAdultChannels; set => _showAdultChannels = value; }
        public bool InternalPlayer { get => _internalPlayer; set => _internalPlayer = value; }
        public bool Fullscreen { get => _fullscreen; set => _fullscreen = value; }
        public bool PlayOnBackground { get => _playOnBackground; set => _playOnBackground = value; }
        public bool DoNotSplitScreenOnLandscape { get => _doNotSplitScreenOnLandscape; set => _doNotSplitScreenOnLandscape = value; }
        public string AutoPlayChannelNumber { get => _autoPlayChannelNumber; set => _autoPlayChannelNumber = value; }
        public string LastChannelNumber { get => _lastChannelNumber; set => _lastChannelNumber = value; }
        public AppFontSizeEnum AppFontSize { get => _appFontSize; set => _appFontSize = value; }
        public bool EnableLogging { get => _enableLogging; set => _enableLogging = value; }
        public LoggingLevelEnum LoggingLevel { get => _loggingLevel; set => _loggingLevel = value; }
        public bool Purchased { get => _purchased; set => _purchased = value; }

        public bool NotPurchased => !Purchased;
        public string PurchaseProductId => "onlinetelevizor.full";

        public bool DebugMode { get => _dDebugMode; set => _dDebugMode = value; }
        public string DeviceId { get => _dDeviceId; set => _dDeviceId = value; }
        public string DevicePassword { get => _dDevicePassword; set => _dDevicePassword = value; }
    }
}
