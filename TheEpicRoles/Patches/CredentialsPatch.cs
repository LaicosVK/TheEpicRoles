using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheEpicRoles.Patches {
    [HarmonyPatch]
    public static class CredentialsPatch {
        public static string terColor               = "#00ffd9";
        public static string torColor               = "#fcce03";
        public static string fullCredentials        = $"<size=130%><color={terColor}>The Epic Roles</color></size> <size=50%>v{TheEpicRolesPlugin.Version.ToString()}\nRemodded by <color={terColor}>LaicosVK</color>, <color={terColor}>Nova</color> & <color={terColor}>DasMonschta</color>\nGraphics by <color={terColor}>moep424</color></size>";
        public static string mainMenuCredentials    = $"Remodded by <color={terColor}>LaicosVK</color>, <color={terColor}>Nova</color> & <color={terColor}>DasMonschta</color>\nGraphics by <color={terColor}>moep424</color>";
        public static string torCredentials         = $"<size=40%><color={torColor}>Original Mod by github.com/Eisbison/TheOtherRoles</color></size>";

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        private static class VersionShowerPatch
        {
            static void Postfix(VersionShower __instance) {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo == null) return;

                var credentials = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(__instance.text);
                credentials.transform.position = new Vector3(0, 0, 0);
                credentials.SetText($"v{TheEpicRolesPlugin.Version.ToString()}\n<size=1f%>\n</size>{mainMenuCredentials}\n<size=1%>\n</size>\n{torCredentials}\n");
                credentials.alignment = TMPro.TextAlignmentOptions.Center;
                credentials.fontSize *= 0.75f;
                credentials.SetOutlineThickness(0);
                credentials.transform.SetParent(amongUsLogo.transform);
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        private static class PingTrackerPatch
        {
            private static GameObject modStamp;
            static void Prefix(PingTracker __instance) {
                
                if (modStamp == null) {
                    modStamp = new GameObject("ModStamp");
                    var rend = modStamp.AddComponent<SpriteRenderer>();
                    rend.sprite = TheEpicRolesPlugin.GetModStamp();
                    rend.color = new Color(1, 1, 1, 0.5f);
                    modStamp.transform.parent = __instance.transform.parent;
                    modStamp.transform.localScale *= 0.6f;
                }
                float offset = (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) ? 0.75f : 0.75f;
                modStamp.transform.position = HudManager.Instance.MapButton.transform.position + Vector3.down * offset;

                // changed position of friends list button
                var objects = GameObject.FindObjectsOfType<FriendsListButton>();
                if (objects == null) return;
                objects[0].transform.localPosition = new Vector3(1.6f, -0.75f, objects[0].transform.localPosition.z);
            }

            static void Postfix(PingTracker __instance)
            {
                __instance.text.alignment = TMPro.TextAlignmentOptions.TopRight;
                __instance.text.SetOutlineThickness(0);
                __instance.text.text = $"{fullCredentials}\n{__instance.text.text}";
                if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started || PlayerControl.LocalPlayer.Data.IsDead || (!(PlayerControl.LocalPlayer == null) && (PlayerControl.LocalPlayer == Lovers.lover1 || PlayerControl.LocalPlayer == Lovers.lover2)))
                
                    __instance.transform.localPosition = new Vector3(3.45f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                else
                    __instance.transform.localPosition = new Vector3(4.25f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                __instance.enabled = false;
                __instance.enabled = true;
            }
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static class LogoPatch
        {
            public static SpriteRenderer renderer;
            public static Sprite bannerSprite;
            public static Sprite horseBannerSprite;
            private static PingTracker instance;
            static void Postfix(PingTracker __instance)
            {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo != null)
                {
                    amongUsLogo.transform.localScale *= 0.6f;
                    amongUsLogo.transform.position += Vector3.up * 0.25f;
                }

                var torLogo = new GameObject("bannerLogo_TER");
                torLogo.transform.position = Vector3.up;
                renderer = torLogo.AddComponent<SpriteRenderer>();
                loadSprites();
                renderer.sprite = Helpers.loadSpriteFromResources("TheEpicRoles.Resources.Banner.png", 300f);

                instance = __instance;
                loadSprites();
                renderer.sprite = MapOptions.enableHorseMode ? horseBannerSprite : bannerSprite;
            }

            public static void loadSprites()
            {
                if (bannerSprite == null) bannerSprite = Helpers.loadSpriteFromResources("TheEpicRoles.Resources.Banner.png", 300f);
                if (horseBannerSprite == null) horseBannerSprite = Helpers.loadSpriteFromResources("TheEpicRoles.Resources.BannerHorse.png", 300f);
            }

            public static void updateSprite()
            {
                loadSprites();
                if (renderer != null)
                {
                    float fadeDuration = 1f;
                    instance.StartCoroutine(Effects.Lerp(fadeDuration, new Action<float>((p) =>
                    {
                        renderer.color = new Color(1, 1, 1, 1 - p);
                        if (p == 1)
                        {
                            renderer.sprite = MapOptions.enableHorseMode ? horseBannerSprite : bannerSprite;
                            instance.StartCoroutine(Effects.Lerp(fadeDuration, new Action<float>((p) =>
                            {
                                renderer.color = new Color(1, 1, 1, p);
                            })));
                        }
                    })));
                }
            }
        }
    }
}


