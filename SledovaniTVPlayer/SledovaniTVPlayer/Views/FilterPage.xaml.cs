﻿using Android.Content;
using LoggerService;
using SledovaniTVPlayer.Models;
using SledovaniTVPlayer.Services;
using SledovaniTVPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SledovaniTVPlayer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FilterPage : ContentPage
    {
        private FilterPageViewModel _viewModel;

        public string FilterForGroup { get; set; } = null;
        public string FilterForType { get; set; } = null;

        public FilterPage(ILoggingService loggingService, ISledovaniTVConfiguration config, Context context, TVService service)
        {
            InitializeComponent();

            var dialogService = new DialogService(this);

            BindingContext = _viewModel = new FilterPageViewModel(loggingService, config, dialogService, context, service);            
        }

        private async void Group_Tapped(object sender, ItemTappedEventArgs e)
        {
            var filterItem = e.Item as FilterItem;
            FilterForGroup = filterItem.Name;
        }

        private async void Type_Tapped(object sender, ItemTappedEventArgs e)
        {
            var filterItem = e.Item as FilterItem;
            FilterForType = filterItem.Name;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _viewModel.RefreshCommand.Execute(null);

            // selecting all groups and types
            GroupListView.ItemAppearing+=
                delegate
                {
                    GroupListView.SelectedItem = _viewModel.Groups[0];
                    TypeListView.SelectedItem = _viewModel.Types[0];
                };

            FilterForGroup = null;
            FilterForType = null;
        }
    }
}