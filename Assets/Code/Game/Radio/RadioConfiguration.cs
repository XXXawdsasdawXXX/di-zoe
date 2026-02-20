using System;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace Code.Game.Radio
{
    [CreateAssetMenu(fileName = "RadioConfiguration", menuName = "Configuration/Radio")]
    public class RadioConfiguration : ScriptableObject
    {
        [Serializable]
        public struct ChannelData
        {
            public string Id;
            public string Path;
        }
        
        public ChannelData[] Channels;

        
        #region Editor

#if UNITY_EDITOR
        
        [Button()]
        public void UpdateID()
        {
            if (Channels == null || Channels.Length == 0)
            {
                return;
            }
            
            ChannelData[] updatedChannels = new ChannelData[Channels.Length];

            for (int index = 0; index < Channels.Length; index++)
            {
                ChannelData data = Channels[index];

                if (string.IsNullOrEmpty(data.Path))
                {
                    continue;
                }

                string[] olololo = data.Path.Split('/');
                
                string id = olololo[olololo.Length - 1].Split('-')[0];
                
                updatedChannels[index] = new ChannelData
                {
                    Id = id,
                    Path = data.Path
                };
            }

            Channels = updatedChannels;
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        } 

        
        
#endif
        

        #endregion
    }
}