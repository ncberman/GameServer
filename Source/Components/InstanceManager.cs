using Common.Logging;
using GameLibrary.Model;
using GameServer.Source.Models;
using System.Collections.Concurrent;

namespace GameServer.Source.Components
{
    public class InstanceManager
    {
        private static readonly ILog Logger = LogManager.GetLogger<InstanceManager>();
        private static readonly Lazy<InstanceManager> lazyInstance = new(() => new InstanceManager());
        public static InstanceManager Instance => lazyInstance.Value;

        readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, Instance>> InstanceDictionary = new();

        public InstanceManager() 
        {
            var homeInstance = new Instance("Home");
            var newDict = InstanceDictionary.GetOrAdd("Home", new ConcurrentDictionary<Guid, Instance>());
            newDict.GetOrAdd(homeInstance.InstanceId, homeInstance);
            Logger.Info($"InstanceManager constructed");
        }

        public Guid JoinInstance(string instanceName, string clientId, SharedCharacter character)
        {
            if(InstanceDictionary.TryGetValue(instanceName, out var InstanceIdDict))
            {
                // Find instance of the same name that isn't full
                foreach(var instance in InstanceIdDict.Values)
                {
                    if (!instance.Filled)
                    {
                        instance.AddCharacter(clientId, character);
                        return instance.InstanceId;
                    }
                }
                // Only get here if all instances are full
                var newInstance = new Instance(instanceName);
                InstanceIdDict.TryAdd(newInstance.InstanceId, newInstance);
                newInstance.AddCharacter(clientId, character);
                return newInstance.InstanceId;
            }
            else
            {
                // if no instances exist with that name
                var newDict = InstanceDictionary.GetOrAdd(instanceName, new ConcurrentDictionary<Guid, Instance>());
                var newInstance = new Instance(instanceName);
                newDict.TryAdd(newInstance.InstanceId, newInstance);
                newInstance.AddCharacter(clientId, character);
                return newInstance.InstanceId;
            }
        }
    }
}
