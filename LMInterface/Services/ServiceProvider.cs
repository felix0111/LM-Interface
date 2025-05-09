using System;
using System.Threading.Tasks;
using Windows.Storage;
using CommunityToolkit.WinUI.Helpers;
using Newtonsoft.Json;

namespace LMInterface.Services
{
    public static class ServiceProvider {

        public static SettingsService SettingsService { get; private set; } = new ();

        public static async Task LoadServices() {
            SettingsService = await LoadService<SettingsService>();
        }

        public static async Task SaveServices() {
            await SaveService(SettingsService);
        }

        private static async Task<T> LoadService<T>() where T : new() {
            //if file doesnt exists, create new service
            if (!await ApplicationData.Current.LocalFolder.FileExistsAsync($"{typeof(T).Name}.json")) return new T();

            //read and deserialize file
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync($"{typeof(T).Name}.json");
            var json = await FileIO.ReadTextAsync(file);
            return JsonConvert.DeserializeObject<T>(json) ?? new T();
        }

        private static async Task SaveService<T>(T service) {
            //create/overwrite file with name of type
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync($"{typeof(T).Name}.json", CreationCollisionOption.ReplaceExisting);

            var json = JsonConvert.SerializeObject(service);
            await FileIO.WriteTextAsync(file, json);
        }
    }
}
