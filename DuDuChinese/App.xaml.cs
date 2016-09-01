using Windows.UI.Xaml;
using System.Threading.Tasks;
using DuDuChinese.Services.SettingsServices;
using Windows.ApplicationModel.Activation;
using Template10.Controls;
using Template10.Common;
using System;
using System.Linq;
using Windows.UI.Xaml.Data;
using CC_CEDICT.Universal;
using Windows.Media.SpeechSynthesis;
using Windows.ApplicationModel.Resources.Core;
using System.Collections.Generic;
using System.IO.IsolatedStorage;

namespace DuDuChinese
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki

    [Bindable]
    sealed partial class App : Template10.Common.BootStrapper
    {
        public App()
        {
            InitializeComponent();
            //SplashFactory = (e) => new Views.Splash(e);

            #region App settings

            var _settings = SettingsService.Instance;
            RequestedTheme = _settings.AppTheme;
            CacheMaxDuration = _settings.CacheMaxDuration;
            ShowShellBackButton = _settings.UseShellBackButton;

            #endregion
        }

        #region Dictionary

        /// <summary>
        /// List manager shared between pages
        /// </summary>
        ListManager _ListManager;
        public ListManager ListManager
        {
            get
            {
                if (_ListManager == null)
                    _ListManager = new ListManager();
                return _ListManager;
            }
        }

        /// <summary>
        /// CC_CEDICT.Dictionary shared between search and list pages (latter for email).
        /// </summary>
        public Dictionary Dictionary;

        #endregion

        #region Speach synthesizer

        // Speech recognition elements
        private ResourceContext speechContext;
        private ResourceMap speechResourceMap;
        private SpeechSynthesizer synthesizer = null;

        public SpeechSynthesizer Synthesizer
        {
            get
            {
                if (this.synthesizer == null)
                {
                    // Initalize speech synthesizer
                    synthesizer = new SpeechSynthesizer();
                    speechContext = ResourceContext.GetForCurrentView();
                    speechContext.Languages = new string[] { SpeechSynthesizer.DefaultVoice.Language };
                    bool chineseLanguageFound = false;
                    foreach (var voice in SpeechSynthesizer.AllVoices)
                    {
                        var v = voice;
                        string lang = voice.Language;
                        if (lang == "zh-CN")
                        {
                            speechContext.Languages = new string[] { lang };
                            synthesizer.Voice = voice;
                            chineseLanguageFound = true;
                            break;  // Select female voice
                        }
                    }
                    if (!chineseLanguageFound)
                    {
                        this.synthesizer = null;
                        return null;
                    }

                    speechResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("LocalizationTTSResources");
                }
                return this.synthesizer;
            }
        }

        #endregion

        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            if (Window.Current.Content as ModalDialog == null)
            {
                // create a new frame 
                var nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);

                // create modal root
                Window.Current.Content = new ModalDialog
                {
                    DisableBackButtonWhenModal = true,
                    Content = new Views.Shell(nav),
                };
            }
            await Task.CompletedTask;
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // Remove lists to be removed
            Template10.Services.SettingsService.ISettingsHelper _helper = new Template10.Services.SettingsService.SettingsHelper();
            List<string> listsToBeRemoved = _helper.Read<List<string>>("ListsToBeRemoved", new List<string>());
            foreach (string filename in listsToBeRemoved)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("ManagedList.Delete(): {0}", filename));
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                    if (store.FileExists(filename))
                        store.DeleteFile(filename);
            }
            _helper.Write<List<string>>("ListsToBeRemoved", new List<string>());

            // Refresh app theme based on the stored value
            var _settings = SettingsService.Instance;
            Views.Shell.HamburgerMenu.RefreshStyles(_settings.AppTheme);

            // Change TitleBar colours
            var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            appView.TitleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
            appView.TitleBar.ButtonHoverBackgroundColor = Windows.UI.Colors.LightGray;

            NavigationService.Navigate(typeof(Views.MainPage));
            await Task.CompletedTask;
        }
    }
}

