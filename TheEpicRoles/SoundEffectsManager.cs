using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;

namespace TheEpicRoles
{
    // Class to preload all audio/sound effects that are contained in the embedded resources.
    // The effects are made available through the soundEffects Dict / the get and the play methods.
    public static class SoundEffectsManager
        
    {
        private static Dictionary<string, AudioClip> soundEffects;
        private static bool loaded = false;

        public static void Load(bool reload=true)
        {
            if (loaded && !reload) return;
            soundEffects = new Dictionary<string, AudioClip>();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();
            // Load all raw audio clips from the resources in which they come bundled
            foreach (string resourceName in resourceNames)
            {
                if (resourceName.Contains("TheEpicRoles.Resources.SoundEffects.") && resourceName.Contains(".raw"))
                {
                    soundEffects.Add(resourceName, Helpers.loadAudioClipFromResources(resourceName));
                }
            }

            // Load raw audio clips from disk, replacing the ones with the same name
            string applicationPath = Path.GetDirectoryName(Application.dataPath) + "\\Sound\\raw";
            if (Directory.Exists(applicationPath)) {
                foreach (var file in Directory.EnumerateFiles(applicationPath, "*.raw")) {
                    string fileName = Path.GetFileName(file);
                    string resourceName = "TheEpicRoles.Resources.SoundEffects." + fileName;
                    if (soundEffects.ContainsKey(resourceName)) {
                        int originalLength = soundEffects[resourceName].samples;
                        soundEffects[resourceName] = Helpers.loadAudioClipFromDisk(fileName, maxLength: originalLength);
                    }
                }
            }
            loaded = true;
        }

        public static AudioClip get(string path)
        {
            // Convenience: As all SoundEffects are stored in the same folder, allow using just the name as well
            if (!path.Contains(".") && !soundEffects.ContainsKey(path)) path = "TheEpicRoles.Resources.SoundEffects." + path + ".raw";
            AudioClip returnValue;
            return soundEffects.TryGetValue(path, out returnValue) ? returnValue : null;
        }


        public static void play(string path, float volume=0.8f)
        {
            AudioClip clipToPlay = get(path);
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(clipToPlay, false, volume);
        }

        public static void stop(string path) {
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.StopSound(get(path));
        }
    }
}
