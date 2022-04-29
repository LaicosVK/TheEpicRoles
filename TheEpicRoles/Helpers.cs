
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using UnhollowerBaseLib;
using UnityEngine;
using System.Linq;
using static TheEpicRoles.TheEpicRoles;
using TheEpicRoles.Modules;
using TheEpicRoles.Objects;
using TheEpicRoles.Patches;
using HarmonyLib;
using Hazel;
using System.Net;

namespace TheEpicRoles {

    public enum MurderAttemptResult {
        PerformKill,
        SuppressKill,
        BlankKill
    }
    public static class Helpers {

        public static void enableCursor(string mode) {
            if (mode == "init") {
                Sprite sprite = Helpers.loadSpriteFromResources("TheEpicRoles.Resources.Cursor.png", 115f);
                Cursor.SetCursor(sprite.texture, Vector2.zero, CursorMode.Auto);
                return;
            }
            if (TheEpicRolesPlugin.ToggleCursor.Value) {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
            else {
                Sprite sprite = Helpers.loadSpriteFromResources("TheEpicRoles.Resources.Cursor.png", 115f);
                Cursor.SetCursor(sprite.texture, Vector2.zero, CursorMode.Auto);
            }
        }

        public static Sprite loadSpriteFromResources(string path, float pixelsPerUnit) {
            try {
                Texture2D texture = loadTextureFromResources(path);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            } catch {
                System.Console.WriteLine("Error loading sprite from path: " + path);
            }
            return null;
        }

        public static Texture2D loadTextureFromResources(string path) {
            try {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var byteTexture = new byte[stream.Length];
                var read = stream.Read(byteTexture, 0, (int) stream.Length);
                LoadImage(texture, byteTexture, false);
                return texture;
            } catch {
                System.Console.WriteLine("Error loading texture from resources: " + path);
            }
            return null;
        }

        public static Texture2D loadTextureFromDisk(string path) {
            try {          
                if (File.Exists(path))     {
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                    byte[] byteTexture = File.ReadAllBytes(path);
                    LoadImage(texture, byteTexture, false);
                    return texture;
                }
            } catch {
                System.Console.WriteLine("Error loading texture from disk: " + path);
            }
            return null;
        }

        internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
        internal static d_LoadImage iCall_LoadImage;
        private static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable) {
            if (iCall_LoadImage == null)
                iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
            var il2cppArray = (Il2CppStructArray<byte>) data;
            return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
        }

        // download and extract ffmpeg binary
        public static void downloadFFmpeg() {
            bool foundFFmpegInPath = Environment.GetEnvironmentVariable("PATH").Split(';').Any(p => File.Exists(p + "\\ffmpeg.exe"));
            if (foundFFmpegInPath) return;

            string applicationPath = Path.GetDirectoryName(Application.dataPath);
            string zipPath = applicationPath + "\\ffmpeg.zip";
            string exePath = applicationPath + "\\ffmpeg.exe";
            if (File.Exists(exePath)) return;  // FFmpeg is already there!

            // Download zip if not already there
            if (!File.Exists(zipPath)) {
                string downloadUri = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";
                using (var client = new WebClient()) {
                    client.DownloadFile(downloadUri, zipPath);
                }
            }
            // Extract ffmpeg.exe (works only with current windows 10 systems)
            Process tar = new Process();
            tar.StartInfo.FileName = "tar";
            tar.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            tar.StartInfo.CreateNoWindow = true;
            tar.StartInfo.Arguments = $"-xf \"{zipPath}\" --strip-components=2 *\\ffmpeg.exe";
            tar.Start();
        }

        // use ffmpeg batch file to render all files in a folder to raw files in a subfolder
            public static void renderAudioToRaw() {
            string applicationPath = Path.GetDirectoryName(Application.dataPath);

            downloadFFmpeg();  // make sure we have ffmpeg...

            // create batch file to run - will execute ffmpeg as needed
            string command = @" if not exist Sound mkdir Sound
                                if not exist Sound\raw mkdir Sound\raw
                                FOR /r . %%a in (Sound\*.*) DO (
                                ffmpeg.exe -i ""%%a"" -f s32le -ar 48000 -acodec pcm_s32le -y ""%%~pa\raw\%%~na.raw"" 
                                ) ";

            string batPath = applicationPath + "\\sound.bat";
            File.WriteAllText(batPath, command);

            Process cmd = new Process();
            cmd.StartInfo.FileName = batPath;
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.Start();
        }

        public static AudioClip createAudioClip(byte[] data, string clipName, int maxLength=-1) {
            int length = data.Length / 4;
            if (maxLength > 0 && length > maxLength) {
                length = maxLength;
            }
            
            float[] samples = new float[length]; // 4 bytes per sample
            int offset;
            for (int i = 0; i < samples.Length; i++) {
                offset = i * 4;
                samples[i] = (float)BitConverter.ToInt32(data, offset) / Int32.MaxValue;
            }
            int channels = 2;
            int sampleRate = 48000;
            AudioClip audioClip = AudioClip.Create(clipName, samples.Length, channels, sampleRate, false);
            audioClip.SetData(samples, 0);
            return audioClip;
        }

        public static AudioClip loadAudioClipFromDisk(string path, int maxLength) {
            string filePath = Path.GetDirectoryName(Application.dataPath).Replace('\\', '/') + @"/Sound/raw/" + path;
            if (File.Exists(filePath)) {
                byte[] data = File.ReadAllBytes(filePath);
                return createAudioClip(data, "disk_clip", maxLength);
            }
            return null;
        }

        public static AudioClip loadAudioClipFromResources(string path, string clipName = "UNNAMED_TOR_AUDIO_CLIP")
        {
            // must be "raw (headerless) 2-channel signed 32 bit pcm (le) (can e.g. use Audacity® to export)"
            try {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var byteAudio = new byte[stream.Length];
                _ = stream.Read(byteAudio, 0, (int)stream.Length);
                return createAudioClip(byteAudio, clipName);
            } catch {
                System.Console.WriteLine("Error loading AudioClip from resources: " + path);
            }
            return null;

            /* Usage example:
            AudioClip exampleClip = Helpers.loadAudioClipFromResources("TheEpicRoles.Resources.exampleClip.raw");
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(exampleClip, false, 0.8f);
            */
        }

        public static PlayerControl playerById(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == id)
                    return player;
            return null;
        }
        
        public static Dictionary<byte, PlayerControl> allPlayersById()
        {
            Dictionary<byte, PlayerControl> res = new Dictionary<byte, PlayerControl>();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                res.Add(player.PlayerId, player);
            return res;
        }

        // Handle vampire bite and vampire shifting bad player on body report or meeting button
        public static void handleKillOnBodyReport() {
            MessageWriter writer;

            // Vampire: Try to murder the bitten player
            MurderAttemptResult vampireBite = checkMuderAttempt(Vampire.vampire, Vampire.bitten, true);
            if (vampireBite == MurderAttemptResult.PerformKill) {
                Helpers.uncheckedMurderPlayer(Vampire.vampire, Vampire.bitten, false);
            }
            // Shifter: Kill if shifted player is a bad role and shifter wasn't already killed by vampire bite.
            if (Shifter.shifter != null && Shifter.diesBeforeMeeting && !Shifter.shifter.Data.IsDead && Shifter.futureShift != null && Shifter.checkTargetIsBad(Shifter.futureShift) &&
                (Vampire.bitten == null || vampireBite != MurderAttemptResult.PerformKill || Vampire.bitten != Shifter.shifter))
            {
                Helpers.uncheckedMurderPlayer(Shifter.shifter, Shifter.shifter, false);

                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShifterKilledDueBadShift, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.shifterKilledDueBadShift();
            }

            // Vampire: Reset bitten (regardless whether the kill was successful or not)
            writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VampireSetBitten, Hazel.SendOption.Reliable, -1);
            writer.Write(byte.MaxValue);
            writer.Write(byte.MaxValue);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.vampireSetBitten(byte.MaxValue, byte.MaxValue);
        }

        public static void refreshRoleDescription(PlayerControl player) {
            if (player == null) return;

            List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(player); 

            var toRemove = new List<PlayerTask>();
            foreach (PlayerTask t in player.myTasks) {
                var textTask = t.gameObject.GetComponent<ImportantTextTask>();
                if (textTask != null) {
                    var info = infos.FirstOrDefault(x => textTask.Text.StartsWith(x.name));
                    if (info != null)
                        infos.Remove(info); // TextTask for this RoleInfo does not have to be added, as it already exists
                    else
                        toRemove.Add(t); // TextTask does not have a corresponding RoleInfo and will hence be deleted
                }
            }   

            foreach (PlayerTask t in toRemove) {
                t.OnRemove();
                player.myTasks.Remove(t);
                UnityEngine.Object.Destroy(t.gameObject);
            }

            // Add TextTask for remaining RoleInfos
            foreach (RoleInfo roleInfo in infos) {
                var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
                task.transform.SetParent(player.transform, false);

                if (roleInfo.name == "Jackal") {
                    var getSidekickText = Jackal.canCreateSidekick ? " and recruit a Sidekick" : "";
                    task.Text = cs(roleInfo.color, $"{roleInfo.name}: Kill everyone{getSidekickText}");  
                } else {
                    task.Text = cs(roleInfo.color, $"{roleInfo.name}: {roleInfo.shortDescription}");  
                }

                player.myTasks.Insert(0, task);
            }
        }

        public static bool isLighterColor(int colorId) {
            return CustomColors.lighterColors.Contains(colorId);
        }

        public static bool isCustomServer() {
            if (DestroyableSingleton<ServerManager>.Instance == null) return false;
            StringNames n = DestroyableSingleton<ServerManager>.Instance.CurrentRegion.TranslateName;
            return n != StringNames.ServerNA && n != StringNames.ServerEU && n != StringNames.ServerAS;
        }

        public static bool hasFakeTasks(this PlayerControl player) {
            return (player == Jester.jester || player == Jackal.jackal || player == Sidekick.sidekick || player == Arsonist.arsonist || player == Vulture.vulture || Jackal.formerJackals.Contains(player));
        }

        public static bool canBeErased(this PlayerControl player) {
            return (player != Jackal.jackal && player != Sidekick.sidekick && !Jackal.formerJackals.Contains(player));
        }

        public static void clearAllTasks(this PlayerControl player) {
            if (player == null) return;
            for (int i = 0; i < player.myTasks.Count; i++) {
                PlayerTask playerTask = player.myTasks[i];
                playerTask.OnRemove();
                UnityEngine.Object.Destroy(playerTask.gameObject);
            }
            player.myTasks.Clear();
            
            if (player.Data != null && player.Data.Tasks != null)
                player.Data.Tasks.Clear();
        }

        public static void setSemiTransparent(this PoolablePlayer player, bool value) {
            float alpha = value ? 0.25f : 1f;
            foreach (SpriteRenderer r in player.gameObject.GetComponentsInChildren<SpriteRenderer>())
                r.color = new Color(r.color.r, r.color.g, r.color.b, alpha);
            player.NameText.color = new Color(player.NameText.color.r, player.NameText.color.g, player.NameText.color.b, alpha);
        }

        public static string GetString(this TranslationController t, StringNames key, params Il2CppSystem.Object[] parts) {
            return t.GetString(key, parts);
        }

        public static string cs(Color c, string s) {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
        }
 
        private static byte ToByte(float f) {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static KeyValuePair<byte, int> MaxPair(this Dictionary<byte, int> self, out bool tie) {
            tie = true;
            KeyValuePair<byte, int> result = new KeyValuePair<byte, int>(byte.MaxValue, int.MinValue);
            foreach (KeyValuePair<byte, int> keyValuePair in self)
            {
                if (keyValuePair.Value > result.Value)
                {
                    result = keyValuePair;
                    tie = false;
                }
                else if (keyValuePair.Value == result.Value)
                {
                    tie = true;
                }
            }
            return result;
        }

        public static bool hidePlayerName(PlayerControl source, PlayerControl target) {
            if (Camouflager.camouflageTimer > 0f) return true; // No names are visible
            else if (!MapOptions.hidePlayerNames) return false; // All names are visible
            else if (source == null || target == null) return true;
            else if (source == target) return false; // Player sees his own name
            else if (source.Data.Role.IsImpostor && (target.Data.Role.IsImpostor || target == Spy.spy)) return false; // Members of team Impostors see the names of Impostors/Spies
            else if ((source == Lovers.lover1 || source == Lovers.lover2) && (target == Lovers.lover1 || target == Lovers.lover2)) return false; // Members of team Lovers see the names of each other
            else if ((source == Jackal.jackal || source == Sidekick.sidekick) && (target == Jackal.jackal || target == Sidekick.sidekick || target == Jackal.fakeSidekick)) return false; // Members of team Jackal see the names of each other
            else if (Deputy.knowsSheriff && (source == Sheriff.sheriff || source == Deputy.deputy) && (target == Sheriff.sheriff || target == Deputy.deputy)) return false; // Sheriff & Deputy see the names of each other
            return true;
        }

        public static void setDefaultLook(this PlayerControl target) {
            target.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
        }

        public static void setLook(this PlayerControl target, String playerName, int colorId, string hatId, string visorId, string skinId, string petId) {
            target.RawSetColor(colorId);
            target.RawSetVisor(visorId);
            target.RawSetHat(hatId, colorId);
            target.RawSetName(hidePlayerName(PlayerControl.LocalPlayer, target) ? "" : playerName);

            PlayerPhysics playerPhysics = target.MyPhysics;
            SkinViewData nextSkin = DestroyableSingleton<HatManager>.Instance.GetSkinById(skinId).viewData.viewData;

            AnimationClip clip = null;
            var spriteAnim = playerPhysics.Skin.animator;
            var currentPhysicsAnim = playerPhysics.Animator.GetCurrentAnimation();
            if (currentPhysicsAnim == playerPhysics.Skin.skin.RunAnim) clip = nextSkin.RunAnim;
            else if (currentPhysicsAnim == playerPhysics.Skin.skin.SpawnAnim) clip = nextSkin.SpawnAnim;
            else if (currentPhysicsAnim == playerPhysics.Skin.skin.EnterVentAnim) clip = nextSkin.EnterVentAnim;
            else if (currentPhysicsAnim == playerPhysics.Skin.skin.ExitVentAnim) clip = nextSkin.ExitVentAnim;
            else if (currentPhysicsAnim == playerPhysics.Skin.skin.IdleAnim) clip = nextSkin.IdleAnim;
            else clip = nextSkin.IdleAnim;
            float progress = playerPhysics.Animator.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            playerPhysics.Skin.skin = nextSkin;
            spriteAnim.Play(clip, 1f);
            spriteAnim.m_animator.Play("a", 0, progress % 1);
            spriteAnim.m_animator.Update(0f);

            if (target.CurrentPet) UnityEngine.Object.Destroy(target.CurrentPet.gameObject);
            target.CurrentPet = UnityEngine.Object.Instantiate<PetBehaviour>(DestroyableSingleton<HatManager>.Instance.GetPetById(petId).viewData.viewData);
            target.CurrentPet.transform.position = target.transform.position;
            target.CurrentPet.Source = target;
            target.CurrentPet.Visible = target.Visible;
            PlayerControl.SetPlayerMaterialColors(colorId, target.CurrentPet.rend);
        }

        public static bool roleCanUseVents(this PlayerControl player) {
            bool roleCouldUse = false;
            if (Engineer.engineer != null && Engineer.engineer == player)
                roleCouldUse = true;
            else if (Jackal.canUseVents && Jackal.jackal != null && Jackal.jackal == player)
                roleCouldUse = true;
            else if (Sidekick.canUseVents && Sidekick.sidekick != null && Sidekick.sidekick == player)
                roleCouldUse = true;
            else if (Spy.canEnterVents && Spy.spy != null && Spy.spy == player)
                roleCouldUse = true;
            else if (Vulture.canUseVents && Vulture.vulture != null && Vulture.vulture == player)
                roleCouldUse = true;
            else if (player.Data?.Role != null && player.Data.Role.CanVent)  {
                if (Janitor.janitor != null && Janitor.janitor == PlayerControl.LocalPlayer)
                    roleCouldUse = false;
                else if (Mafioso.mafioso != null && Mafioso.mafioso == PlayerControl.LocalPlayer && Godfather.godfather != null && !Godfather.godfather.Data.IsDead)
                    roleCouldUse = false;
                else
                    roleCouldUse = true;
            }
            return roleCouldUse;
        }

        public static MurderAttemptResult checkMuderAttempt(PlayerControl killer, PlayerControl target, bool blockRewind = false) {
            // Modified vanilla checks
            if (AmongUsClient.Instance.IsGameOver) return MurderAttemptResult.SuppressKill;
            if (killer == null || killer.Data == null || killer.Data.IsDead || killer.Data.Disconnected) return MurderAttemptResult.SuppressKill; // Allow non Impostor kills compared to vanilla code
            if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected) return MurderAttemptResult.SuppressKill; // Allow killing players in vents compared to vanilla code

            // Handle blank shot
            if (Pursuer.blankedList.Any(x => x.PlayerId == killer.PlayerId)) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetBlanked, Hazel.SendOption.Reliable, -1);
                writer.Write(killer.PlayerId);
                writer.Write((byte)0);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.setBlanked(killer.PlayerId, 0);

                return MurderAttemptResult.BlankKill;
            }

            // Block impostor shielded kill
            if (Medic.shielded != null && Medic.shielded == target) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.shieldedMurderAttempt();
                return MurderAttemptResult.SuppressKill;
            }

            // Block impostor not fully grown mini kill
            else if (Mini.mini != null && target == Mini.mini && !Mini.isGrownUp()) {
            return MurderAttemptResult.SuppressKill;
            }

            // Block Time Master with time shield kill
            else if (TimeMaster.shieldActive && TimeMaster.timeMaster != null && TimeMaster.timeMaster == target) {
                if (!blockRewind) { // Only rewind the attempt was not called because a meeting startet 
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.TimeMasterRewindTime, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.timeMasterRewindTime();
                }
                return MurderAttemptResult.SuppressKill;
            }

            if (GameStartManagerPatch.guardianShield == "") {
                GameStartManagerPatch.guardianShield = target.Data.PlayerName;
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.SetFirstKill, Hazel.SendOption.Reliable, -1);
                writer.Write(target.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.setFirstKill(target.PlayerId);
            }

            return MurderAttemptResult.PerformKill;
        }

        public static MurderAttemptResult checkMuderAttemptAndKill(PlayerControl killer, PlayerControl target, bool isMeetingStart = false, bool showAnimation = true)  {
            // The local player checks for the validity of the kill and performs it afterwards (different to vanilla, where the host performs all the checks)
            // The kill attempt will be shared using a custom RPC, hence combining modded and unmodded versions is impossible

            MurderAttemptResult murder = checkMuderAttempt(killer, target, isMeetingStart);
            if (murder == MurderAttemptResult.PerformKill) {
                uncheckedMurderPlayer(killer, target, showAnimation);
            }
            return murder;            
        }
        public static void uncheckedMurderPlayer(PlayerControl killer, PlayerControl target, bool showAnimation = true) {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
            writer.Write(killer.PlayerId);
            writer.Write(target.PlayerId);
            writer.Write(showAnimation ? Byte.MaxValue : 0);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.uncheckedMurderPlayer(killer.PlayerId, target.PlayerId, showAnimation ? Byte.MaxValue : (byte)0);
        }
    
        public static void shareGameVersion() {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VersionHandshake, Hazel.SendOption.Reliable, -1);
            writer.Write((byte)TheEpicRolesPlugin.Version.Major);
            writer.Write((byte)TheEpicRolesPlugin.Version.Minor);
            writer.Write((byte)TheEpicRolesPlugin.Version.Build);
            writer.WritePacked(AmongUsClient.Instance.ClientId);
            writer.Write((byte)(TheEpicRolesPlugin.Version.Revision < 0 ? 0xFF : TheEpicRolesPlugin.Version.Revision));
            writer.Write(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray());
            writer.Write(Application.version);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.versionHandshake(TheEpicRolesPlugin.Version.Major, TheEpicRolesPlugin.Version.Minor, TheEpicRolesPlugin.Version.Build, TheEpicRolesPlugin.Version.Revision, Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId, AmongUsClient.Instance.ClientId, Application.version);
        }

        public static List<PlayerControl> getKillerTeamMembers(PlayerControl player) {
            List<PlayerControl> team = new List<PlayerControl>();
            foreach(PlayerControl p in PlayerControl.AllPlayerControls) {
                if (player.Data.Role.IsImpostor && p.Data.Role.IsImpostor && player.PlayerId != p.PlayerId && team.All(x => x.PlayerId != p.PlayerId)) team.Add(p);
                else if (player == Jackal.jackal && p == Sidekick.sidekick) team.Add(p); 
                else if (player == Sidekick.sidekick && p == Jackal.jackal) team.Add(p);
            }
            
            return team;
        }
    }
}
