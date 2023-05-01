using Firebase.Database;
using Firebase.Database.Query;
using FirebaseAdmin.Auth;
using GameServer.Source.Util;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameServer.Source.Services
{
    class FirebaseRealtimeDatabase
    {
        private readonly FirebaseClient firebase;

        public FirebaseRealtimeDatabase()
        {
            firebase = new FirebaseClient(AppSettings.GetValue<string>("Firebase:Database:Url"));
        }

        public async Task SetDataAsync(string path, object data)
        {
            await firebase.Child(path).PutAsync(data);
        }

        public async Task<T> GetDataAsync<T>(string path)
        {
            var response = await firebase.Child(path).OnceSingleAsync<T>();
            return response;
        }

        public async Task UpdateDataAsync(string path, object data)
        {
            await firebase.Child(path).PatchAsync(data);
        }

        public async Task<List<T>> GetListAsync<T>(string path)
        {
            var response = await firebase.Child(path).OnceAsync<T>();
            var result = new List<T>();
            foreach (var item in response)
            {
                result.Add(item.Object);
            }
            return result;
        }

        public async Task AddDataAsync(string path, string key, object data)
        {
            await firebase.Child(path).Child(key).PutAsync(data);
        }
    }
}
