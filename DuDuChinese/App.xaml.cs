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
            SplashFactory = (e) => new Views.Splash(e);

            #region App settings

            var _settings = SettingsService.Instance;
            RequestedTheme = _settings.AppTheme;
            CacheMaxDuration = _settings.CacheMaxDuration;
            ShowShellBackButton = _settings.UseShellBackButton;

            #endregion
        }

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

        // Learning engine shared between all pages
        // LearningEngine

        // Speech recognition elements
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
                    foreach (var voice in SpeechSynthesizer.AllVoices)
                    {
                        var v = voice;
                        string lang = voice.Language;
                        if (lang == "zh-CN")
                        {
                            speechContext.Languages = new string[] { lang };
                            synthesizer.Voice = voice;
                            break;  // Select female voice
                        }
                    }
                    speechResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("LocalizationTTSResources");
                }
                return this.synthesizer;
            }
        }
        private ResourceContext speechContext;
        private ResourceMap speechResourceMap;

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
                    ModalContent = new Views.Busy(),
                };
            }
            await Task.CompletedTask;
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // long-running startup tasks go here
            await Task.Delay(5000);

            NavigationService.Navigate(typeof(Views.MainPage));
            await Task.CompletedTask;
        }
    }
}

