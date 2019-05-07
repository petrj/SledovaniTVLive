﻿using Android.App;
using Android.Content;
using LoggerService;
using SledovaniTVAPI;
using OnlineTelevizor.Models;
using OnlineTelevizor.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System.Threading;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;
using Plugin.Toast;

namespace OnlineTelevizor.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private TVService _service;
        private static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public ObservableCollection<ChannelItem> Channels { get; set; } = new ObservableCollection<ChannelItem>();

        private Dictionary<string, ChannelItem> _channelById { get; set; } = new Dictionary<string, ChannelItem>();

        public TVService TVService
        {
            get
            {
                return _service;
            }
        }

        public Command RefreshCommand { get; set; }

        public Command PlayCommand { get; set; }

        public Command RefreshChannelsCommand { get; set; }
        public Command RefreshEPGCommand { get; set; }
        public Command ResetConnectionCommand { get; set; }
        public Command CheckPurchaseCommand { get; set; }

        public MainPageViewModel(ILoggingService loggingService, IOnlineTelevizorConfiguration config, IDialogService dialogService, Context context)
           : base(loggingService, config, dialogService, context)
        {
            _service = new TVService(loggingService, config);
            _loggingService = loggingService;
            _dialogService = dialogService;
            _context = context;
            Config = config;

            RefreshCommand = new Command(async () => await Refresh());

            CheckPurchaseCommand = new Command(async () => await CheckPurchase());

            RefreshEPGCommand = new Command(async () => await RefreshEPG());
            RefreshChannelsCommand = new Command(async () => await RefreshChannels());

            ResetConnectionCommand = new Command(async () => await ResetConnection());

            PlayCommand = new Command(async () => await Play());

            RefreshChannelsCommand.Execute(null);

            // refreshing every min with 3s start delay
            BackgroundCommandWorker.RunInBackground(RefreshEPGCommand, 60, 3);
        }

        public async Task SelectChannelByNumber(string number)
        {
            await _semaphoreSlim.WaitAsync();

            await Task.Run(
                () =>
                {
                    try
                    {
                        // looking for channel by its number:
                        foreach (var ch in Channels)
                        {
                            if (ch.ChannelNumber == number)
                            {
                                SelectedItem = ch;
                                break;
                            }
                        }
                    }
                    finally
                    {
                        OnPropertyChanged(nameof(SelectedItem));

                        _semaphoreSlim.Release();
                    };
                });
        }

        public ChannelItem SelectedItem { get; set; }

        public async Task SelectNextChannel(int step = 1)
        {
            await _semaphoreSlim.WaitAsync();

            await Task.Run(
                () =>
                {
                try
                {
                    if (Channels.Count == 0)
                        return;

                        if (SelectedItem == null)
                        {
                            SelectedItem = Channels[0];
                        }
                        else
                        {
                            bool next = false;
                            var nextCount = 1;
                            for (var i = 0; i < Channels.Count; i++)
                            {
                                var ch = Channels[i];

                                if (next)
                                {
                                    if (nextCount == step || i == Channels.Count-1)
                                    {
                                        SelectedItem = ch;
                                        break;
                                    }
                                    else
                                    {
                                        nextCount++;
                                    }
                                }
                                else
                                {
                                    if (ch == SelectedItem)
                                    {
                                        next = true;
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        OnPropertyChanged(nameof(SelectedItem));

                        _semaphoreSlim.Release();
                    };
                });
        }

        public async Task SelectPreviousChannel(int step = 1)
        {
            await _semaphoreSlim.WaitAsync();

            await Task.Run(
                () =>
                {
                    try
                    {
                        if (Channels.Count == 0)
                            return;

                        if (SelectedItem == null)
                        {
                            SelectedItem = Channels[Channels.Count - 1];
                        }
                        else
                        {
                            bool previous = false;
                            var previousCount = 1;

                            for (var i = Channels.Count-1; i >=0 ; i--)
                            {
                                if (previous)
                                {
                                    if (previousCount == step || i == 0)
                                    {
                                        SelectedItem = Channels[i];
                                        break;
                                    } else
                                    {
                                        previousCount++;
                                    }
                                }
                                else
                                {
                                    if (Channels[i] == SelectedItem)
                                    {
                                        previous = true;
                                    }
                                }
                            }
                        }

                        OnPropertyChanged(nameof(SelectedItem));

                    }
                    finally
                    {
                        OnPropertyChanged(nameof(SelectedItem));

                        _semaphoreSlim.Release();
                    };
                });
        }

        private Dictionary<string, ChannelItem> ChannelById
        {
            get
            {
                return _channelById;
            }
        }

        public string StatusLabel
        {
            get
            {
                if (IsBusy)
                {
                    return "Aktualizují se kanály...";
                }

                switch (_service.Status)
                {
                    case StatusEnum.GeneralError: return $"Chyba";
                    case StatusEnum.ConnectionNotAvailable: return $"Chyba připojení";
                    case StatusEnum.NotInitialized: return "";
                    case StatusEnum.EmptyCredentials: return "Nevyplněny přihlašovací údaje";
                    case StatusEnum.Logged: return GetChannelsStatus;
                    case StatusEnum.LoginFailed: return $"Chybné přihlašovací údaje";
                    case StatusEnum.Paired: return $"Uživatel přihlášen";
                    case StatusEnum.PairingFailed: return $"Chybné přihlašovací údaje";
                    case StatusEnum.BadPin: return $"Chybný PIN";
                    default: return String.Empty;
                }
            }
        }

        private string GetChannelsStatus
        {
            get
            {
                string status = String.Empty;

                if (!Config.Purchased)
                    status = "Verze zdarma. ";

                if (Channels.Count == 0)
                {
                    return $"{status}Není k dispozici žádný kanál";
                }
                else
                if (Channels.Count == 1)
                {
                    return $"{status}Načten 1 kanál";
                }
                else
                if ((Channels.Count >= 2) && (Channels.Count <= 4))
                {
                    return $"{status}Načteny {Channels.Count} kanály";
                }
                else
                {
                    return $"{status}Načteno {Channels.Count} kanálů";
                }
            }
        }

        private async Task Refresh()
        {
            await RefreshChannels(false);

            // reconnecting after 1 sec (connection may fail after resume on wifi with poor signal)
            await Task.Delay(1000);

            if (_service.Status == StatusEnum.ConnectionNotAvailable)
            {
                await RefreshChannels(false);
                await RefreshEPG();
            }
            else
            {
                await RefreshEPG();
            }
        }

        private async Task RefreshEPG(bool SetFinallyNotBusy = true)
        {
            await _semaphoreSlim.WaitAsync();

            IsBusy = true;

            try
            {
                OnPropertyChanged(nameof(StatusLabel));

                foreach (var channelItem in Channels)
                {
                    channelItem.ClearEPG();
                }

                var epg = await _service.GetEPG();
                foreach (var ei in epg)
                {
                    if (ChannelById.ContainsKey(ei.ChannelId))
                    {
                        // updating channel EPG

                        var ch = ChannelById[ei.ChannelId];
                        ch.AddEPGItem(ei);
                    }
                }
            }
            finally
            {
                if (SetFinallyNotBusy)
                {
                    IsBusy = false;
                }

                OnPropertyChanged(nameof(StatusLabel));
                OnPropertyChanged(nameof(IsBusy));

                _semaphoreSlim.Release();
            }
        }

        private async Task RefreshChannels(bool SetFinallyNotBusy = true)
        {
            await _semaphoreSlim.WaitAsync();

            await CheckPurchase();

            IsBusy = true;

            try
            {
                string selectedChannelId = null;
                if (SelectedItem != null)
                {
                    selectedChannelId = SelectedItem.Id;
                }

                OnPropertyChanged(nameof(StatusLabel));

                Channels.Clear();
                _channelById.Clear();

                var channels = await _service.GetChannels();

                foreach (var ch in channels)
                {
                    if (Config.ChannelFilterGroup != "*" &&
                        Config.ChannelFilterGroup != null &&
                        Config.ChannelFilterGroup != ch.Group)
                        continue;

                    if (Config.ChannelFilterType != "*" &&
                        Config.ChannelFilterType != null &&
                        Config.ChannelFilterType != ch.Type)
                        continue;

                    if ((!String.IsNullOrEmpty(Config.ChannelFilterName)) &&
                        (Config.ChannelFilterName != "*") &&
                        !ch.Name.ToLower().Contains(Config.ChannelFilterName.ToLower()))
                        continue;

                    Channels.Add(ch);
                    _channelById.Add(ch.Id, ch); // for faster EPG refresh
                }

                if (selectedChannelId != null && _channelById.ContainsKey(selectedChannelId))
                {
                    SelectedItem = _channelById[selectedChannelId];
                }
                else if (Channels.Count > 0)
                {
                    // selecting first channel
                    SelectedItem = Channels[0];
                }

            } finally
            {
                if (SetFinallyNotBusy)
                {
                    IsBusy = false;
                }
                OnPropertyChanged(nameof(StatusLabel));
                OnPropertyChanged(nameof(IsBusy));
                OnPropertyChanged(nameof(SelectedItem));

                _semaphoreSlim.Release();
            }
        }

        private async Task ResetConnection()
        {
            await _semaphoreSlim.WaitAsync();

            IsBusy = true;

            try
            {
                OnPropertyChanged(nameof(StatusLabel));

                await _service.ResetConnection();
            }
            finally
            {
                IsBusy = false;

                OnPropertyChanged(nameof(StatusLabel));

                _semaphoreSlim.Release();
            }
        }

        public async Task CheckPurchase()
        {
            if (Config.Purchased || Config.DebugMode)
                return;

            _loggingService.Debug($"Checking purchase");

            try
            {
                // contacting service

                var connected = await CrossInAppBilling.Current.ConnectAsync();

                if (!connected)
                {
                    _loggingService.Info($"Connection to AppBilling service failed");
                    await _dialogService.Information("Nepodařilo se ověřit stav zaplacení plné verze.");
                    return;
                }

                // check InAppBillingPurchase
                var purchases = await CrossInAppBilling.Current.GetPurchasesAsync(ItemType.InAppPurchase);
                foreach (var purchase in purchases)
                {

                    if (purchase.ProductId == Config.PurchaseProductId &&
                        purchase.State == PurchaseState.Purchased)
                    {
                        Config.Purchased = true;

                        _loggingService.Debug($"Already purchased (InAppBillingPurchase)");

                        _loggingService.Debug($"Purchase AutoRenewing: {purchase.AutoRenewing}");
                        _loggingService.Debug($"Purchase Payload: {purchase.Payload}");
                        _loggingService.Debug($"Purchase PurchaseToken: {purchase.PurchaseToken}");
                        _loggingService.Debug($"Purchase State: {purchase.State}");
                        _loggingService.Debug($"Purchase TransactionDateUtc: {purchase.TransactionDateUtc}");
                        _loggingService.Debug($"Purchase ConsumptionState: {purchase.ConsumptionState}");

                        break;
                    }
                }

            } catch (Exception ex)
            {
                _loggingService.Error(ex, "Error while checking purchase");
            }
            finally
            {
                await CrossInAppBilling.Current.DisconnectAsync();
            }
        }

        public async Task Play()
        {
            if (SelectedItem == null)
                return;

            await PlayStream(SelectedItem.Url);
        }
    }
}