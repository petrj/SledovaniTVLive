﻿using Android.Content;
using LoggerService;
using SledovaniTVLive.Models;
using SledovaniTVLive.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;

namespace SledovaniTVLive.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private ISledovaniTVConfiguration _config;

        public Command ShareLogCommand { get; set; }
        public Command PayCommand { get; set; }

        public SettingsViewModel(ILoggingService loggingService, ISledovaniTVConfiguration config, Context context, IDialogService dialogService)
            : base(loggingService, config, dialogService, context)
        {
            _loggingService = loggingService;
            _context = context;
            _dialogService = dialogService;
            _config = config;

            ShareLogCommand = new Command(async () => await ShareLogWithPermissionsRequest());
            PayCommand = new Command(async () => await Pay());
        }

        protected async Task Pay()
        {
            try
            {
                _loggingService.Debug($"Paying product id: {_config.PurchaseProductId}");

                var connected = await CrossInAppBilling.Current.ConnectAsync();

                if (!connected)
                {
                    _loggingService.Info($"Connection to AppBilling service failed");
                    await _dialogService.Information("Připojení k platební službě selhalo.");
                    return;
                }
                
                var purchase = await CrossInAppBilling.Current.PurchaseAsync(_config.PurchaseProductId, ItemType.InAppPurchase, "apppayload");
                if (purchase == null)
                {
                    _loggingService.Info($"Not purchased");
                    //await _dialogService.Information("Platba nebyla uskutečněna.");
                }
                else
                {
                    _loggingService.Info($"Purchase OK");

                    _loggingService.Info($"Purchase Id: {purchase.Id}");
                    _loggingService.Info($"Purchase Token: {purchase.PurchaseToken}");
                    _loggingService.Info($"Purchase State: {purchase.State.ToString()}");
                    _loggingService.Info($"Purchase Date: {purchase.TransactionDateUtc.ToString()}");
                    _loggingService.Info($"Purchase Payload: {purchase.Payload}");
                    _loggingService.Info($"Purchase ConsumptionState: {purchase.ConsumptionState.ToString()}");
                    _loggingService.Info($"Purchase AutoRenewing: {purchase.AutoRenewing}");                    

                    _config.PurchaseToken = purchase.PurchaseToken;
                    _config.PurchaseId = purchase.Id;
                    _config.Purchased = true;

                    //await _dialogService.Information("Platba byla úspěšně provedena.");
                }
            }
            catch (Exception ex)
            {                
                //await _dialogService.Information("Platba se nezdařila.");
                _loggingService.Error(ex, "Payment failed");
            }
            finally
            {
                await CrossInAppBilling.Current.DisconnectAsync();
            }
        }
        
        public int LoggingLevelIndex
        {
            get
            {
                switch (_config.LoggingLevel)
                {
                    case LoggingLevelEnum.Debug:
                        return 0;
                    case LoggingLevelEnum.Info:
                        return 1;
                    case LoggingLevelEnum.Error:
                        return 2;
                }

                return 2;
            }
            set
            {
                // 0 -> Debug
                // 1 -> Info
                // 3 -> Error

                switch (value)
                {
                    case 0: _config.LoggingLevel = LoggingLevelEnum.Debug;
                        break;
                    case 1:
                        _config.LoggingLevel = LoggingLevelEnum.Info;
                        break;
                    case 2:
                        _config.LoggingLevel = LoggingLevelEnum.Error;
                        break;
                }  
                OnPropertyChanged(nameof(LoggingLevelIndex));
            }
        }

        private async Task ShareLogWithPermissionsRequest()
        {
            await RunWithPermission(Permission.Storage, async () => await ShareLog());
        }

        private async Task ShareLog()
        {
            if (!_config.EnableLogging)
            {
                await _dialogService.Information("Logování není povoleno");
                return;
            }

            if (!(_loggingService is BasicLoggingService))
            {
                await _dialogService.Information("Logování bude probíhat až po restartování aplikace");
                return;
            }

            var fName = (_loggingService as BasicLoggingService).LogFilename;

            if (!File.Exists(fName))
            {
                await _dialogService.Information($"Log {fName} nebyl nalezen");
                return;
            }

            await ShareFile(fName);
        }
    }
}
