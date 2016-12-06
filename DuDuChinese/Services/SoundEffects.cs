using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace DuDuChinese.Services
{
    public enum SoundEfxEnum
    {
        WRONG,
        COMPLETE
    }

    public class SoundEffects
    {
        private Dictionary<SoundEfxEnum, MediaElement> effects;

        public SoundEffects()
        {
            effects = new Dictionary<SoundEfxEnum, MediaElement>();
            LoadEfx();
        }

        private async void LoadEfx()
        {
            effects.Add(SoundEfxEnum.WRONG, await LoadSoundFile("wrong.wav"));
            effects.Add(SoundEfxEnum.COMPLETE, await LoadSoundFile("complete.wav"));
        }

        private async Task<MediaElement> LoadSoundFile(string file)
        {
            MediaElement mediaElement = new MediaElement();
            mediaElement.AutoPlay = false;

            // Load sound from the resources
            Assembly assembly = this.GetType().GetTypeInfo().Assembly;
            AssemblyName assemblyName = new AssemblyName(assembly.FullName);
            Stream resourceStream = assembly.GetManifestResourceStream(assemblyName.Name + ".Assets.Sounds." + file);
            var memStream = new MemoryStream();
            await resourceStream.CopyToAsync(memStream);
            memStream.Position = 0;

            mediaElement.SetSource(memStream.AsRandomAccessStream(), "audio/wav");
            return mediaElement;
        }

        public async void Play(SoundEfxEnum efx)
        {
            var mediaElement = effects[efx];

            if (mediaElement.CurrentState.Equals(Windows.UI.Xaml.Media.MediaElementState.Playing))
                mediaElement.Stop();

            mediaElement.Play();

            switch (efx)
            {
                case SoundEfxEnum.WRONG:
                default:
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    break;
            }
        }
    }
}
