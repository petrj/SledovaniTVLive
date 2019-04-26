﻿using Android.App;
using Android.Content;
using LoggerService;
using SledovaniTVAPI;
using SledovaniTVLive.Models;
using SledovaniTVLive.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System.Threading;

namespace SledovaniTVLive.ViewModels
{
    public class FilterPageViewModel : BaseViewModel
    {
        private TVService _service;
        private ISledovaniTVConfiguration _config;
        private static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private GroupFilterItem _selectedGroupItem;
        private TypeFilterItem _selectedTypeItem;

        public ObservableCollection<GroupFilterItem> Groups { get; set; } = new ObservableCollection<GroupFilterItem>();
        public ObservableCollection<FilterItem> Types { get; set; } = new ObservableCollection<FilterItem>();

        public Command RefreshCommand { get; set; }
        public Command ClearFilterCommand { get; set; }

        public GroupFilterItem FirstGroup { get; private set; } = new GroupFilterItem() { Name = "*" };
        public TypeFilterItem FirstType { get; private set; } = new TypeFilterItem() { Name = "*" };

        public string ChannelNameFilter
        {
            get
            {
                return _config.ChannelFilterName;
            }
            set
            {
                _config.ChannelFilterName = value;
                OnPropertyChanged(nameof(ChannelNameFilter));
            }
        }

        public GroupFilterItem SelectedGroupItem
        {
            get
            {
                return _selectedGroupItem;
            }
            set
            {   
                _selectedGroupItem = value;
                _config.ChannelFilterGroup = value == null ? "*" : value.Name;
                
                OnPropertyChanged(nameof(SelectedGroupItem));
            }
        }

        public TypeFilterItem SelectedTypeItem
        {
            get
            {
                return _selectedTypeItem;
            }
            set
            {
                _selectedTypeItem = value;
                _config.ChannelFilterType = value == null ? "*" : value.Name;
                OnPropertyChanged(nameof(SelectedTypeItem));
            }
        }

        public FilterPageViewModel(ILoggingService loggingService, ISledovaniTVConfiguration config, IDialogService dialogService, Context context, TVService service)
           : base(loggingService, config, dialogService, context)
        {
            _service = service;
            _loggingService = loggingService;
            _dialogService = dialogService;
            _context = context;
            _config = config;

            ClearFilterCommand = new Command(async () => await ClearFilter());
            RefreshCommand = new Command(async () => await Refresh());
            // SomeCommand = new Command(async () => await Task.Run(delegate { }));            
        }

        private async Task ClearFilter()
        {
            SelectedTypeItem = FirstType;
            SelectedGroupItem = FirstGroup;
            ChannelNameFilter = "";

            await Refresh();
        }

        private async Task Refresh()
        {
            await _semaphoreSlim.WaitAsync();

            IsBusy = true;

            // Clearing Pickers leads to clearing config value via SelectedTypeItem
            var selectedGroupConfig = _config.ChannelFilterGroup;
            var selectedTypeConfig = _config.ChannelFilterType;
           
            try
            {
                Groups.Clear();
                Types.Clear();

                FirstGroup.Count = 0;
                FirstType.Count = 0;

                var groupToItem = new Dictionary<string, GroupFilterItem>();
                var typeToItem = new Dictionary<string, TypeFilterItem>();                

                var channels = await _service.GetChannels();

                foreach (var ch in channels)
                {
                    FirstGroup.Count++;
                    FirstType.Count++;

                    if (!groupToItem.ContainsKey(ch.Group))
                    {              
                        var g = new GroupFilterItem()
                        {
                            Name = ch.Group,
                            Count = 1
                        };

                        if ((!String.IsNullOrEmpty(_config.ChannelFilterGroup)) && (ch.Group == selectedGroupConfig))
                        {
                            SelectedGroupItem = g;
                        }

                        groupToItem.Add(ch.Group,g);
                    } else
                    {
                        groupToItem[ch.Group].Count++;
                    }

                    if (!typeToItem.ContainsKey(ch.Type))
                    {
                        var tp = new TypeFilterItem()
                        {
                            Name = ch.Type,
                            Count = 1
                        };

                        if ((!String.IsNullOrEmpty(_config.ChannelFilterType)) && (ch.Type == selectedTypeConfig))
                        {
                            SelectedTypeItem = tp;
                        }

                        typeToItem.Add(ch.Type, tp);
                    }
                    else
                    {
                        typeToItem[ch.Type].Count++;                        
                    }
                }

                Groups.Add(FirstGroup);
                foreach (var kvp in groupToItem)
                    Groups.Add(kvp.Value);

                Types.Add(FirstType);
                foreach (var kvp in typeToItem)
                    Types.Add(kvp.Value);
            } 
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error while refreshing filter page data");
            }
            finally
            {   
                IsBusy = false;             
                OnPropertyChanged(nameof(IsBusy));

                if (SelectedTypeItem == null)
                    SelectedTypeItem = FirstType;

                if (SelectedGroupItem == null)
                    SelectedGroupItem = FirstGroup;

                _semaphoreSlim.Release();
            }            
        }
    }
}
