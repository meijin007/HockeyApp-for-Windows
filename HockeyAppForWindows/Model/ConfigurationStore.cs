﻿using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using HockeyApp.AppLoader.Extensions;

namespace HockeyApp.AppLoader.Model
{
    [DataContract]
    public class ConfigurationStore
    {
        private static ConfigurationStore _instance = null;
        public static ConfigurationStore Instance
        {
            get
            {
                if (_instance == null)
                {
                    byte[] jsonStream = Convert.FromBase64String(HockeyApp.AppLoader.Properties.Settings.Default.Configuration);
                    if (jsonStream != null && jsonStream.Length > 0)
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ConfigurationStore));
                        MemoryStream ms = new MemoryStream(jsonStream);
                        _instance = serializer.ReadObject(ms) as ConfigurationStore;
                    }
                    else
                    {
                        _instance = new ConfigurationStore();
                        _instance.UserConfigurations = new List<UserConfiguration>();
                    }
                }
                return _instance;
            }
        }

        private ConfigurationStore()
        {
            
        }

         [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            //changing defaultconfig from configname to guid, because config name can be changed from now on
            if(!string.IsNullOrEmpty(this.DefaultConfigurationName)){
                var found = this.UserConfigurations.FirstOrDefault(p=>p.ConfigurationName.Equals(this.DefaultConfigurationName));
                this.SetDefaultUserConfiguration(found);
                this.DefaultConfigurationName = null;
                this.Save();
            }
            
        }

        public void Save(){
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ConfigurationStore));
            MemoryStream ms = new MemoryStream();
            serializer.WriteObject(ms, this);
            HockeyApp.AppLoader.Properties.Settings.Default.Configuration = Convert.ToBase64String(ms.ToArray());
            HockeyApp.AppLoader.Properties.Settings.Default.Save();
        }

        public bool IsConfigurationNameUsed(string configurationName)
        {
            return this.UserConfigurations != null && this.UserConfigurations.Any(p => p.ConfigurationName.ToUpper().Equals(configurationName.ToUpper()));
        }

        public void SetDefaultUserConfiguration(UserConfiguration uc=null){
            if (uc != null && this.UserConfigurations.IndexOf(uc) > -1)
            {
                this.DefaultConfigurationId = uc.ConfigurationId;
            }
            else
            {
                this.DefaultConfigurationId = Guid.Empty;
            }
            this.Save();
        }

        public UserConfiguration DefaultUserConfiguration{
            get{
                return this.DefaultConfigurationId==Guid.Empty?null
                    : this.UserConfigurations.FirstOrDefault(p=>p.ConfigurationId.Equals(this.DefaultConfigurationId));
            }
        }

        public string WriteFileToSettingsFolder(Stream stream, string name)
        {
            string fileName = Path.Combine(HockeyAppLoader.App.UserFolder, name);
            FileStream fs = new FileStream(fileName,FileMode.OpenOrCreate);
            stream.CopyTo(fs);
            fs.Flush();
            fs.Close();
            return fileName;
        }

        public bool ExistsFileInSettingsFolder(string name)
        {
            string fileName = Path.Combine(HockeyAppLoader.App.UserFolder, name);
            return File.Exists(fileName);
        }

        public string GetFullPathToFileInSettingsFolder(string name)
        {
            return Path.Combine(HockeyAppLoader.App.UserFolder, name);
        }

        [DataMember]
        public List<UserConfiguration> UserConfigurations { get; set; }

        //Deprecated!
        [DataMember]
        public string DefaultConfigurationName{get;private set;}

        [DataMember]
        private Guid DefaultConfigurationId { get; set; }
    }
}
