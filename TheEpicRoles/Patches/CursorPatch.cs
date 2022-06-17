using HarmonyLib;
using UnityEngine;

namespace TheEpicRoles.Patches {
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public class CursorPatch {
        private static void Prefix(MainMenuManager __instance) {
            Sprite sprite = Helpers.loadSpriteFromResources("TheEpicRoles.Resources.Cursor.png", 115f);
            Cursor.SetCursor(sprite.texture, Vector2.zero, CursorMode.Auto);
        }
    }
}
