using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

namespace TheEpicRoles.Objects {

    class Slap {
        public static List<Slap> slaps = new List<Slap>();

        public GameObject slap; 
        public static Sprite[] boxAnimationSprites = new Sprite[15];

        public static Sprite getBoxAnimationSprite(int index) {
            index = Mathf.Clamp(index, 0, boxAnimationSprites.Length - 1);
            if (boxAnimationSprites[index] == null)
                boxAnimationSprites[index] = (Helpers.loadSpriteFromResources($"TheEpicRoles.Resources.SlapAnimation.trickster_box_00{(index + 1):00}.png", 175f));
            return boxAnimationSprites[index];
        }



        public Slap(Vector2 p) {
            slap = new GameObject("Slap");
            Vector3 position = new Vector3(p.x, p.y, PlayerControl.LocalPlayer.transform.position.z + -0.01f);
            slap.transform.position = position;
            slap.transform.SetParent(PlayerControl.LocalPlayer.transform);

            var boxRenderer = slap.AddComponent<SpriteRenderer>();         

            GameObject sprite = PlayerControl.LocalPlayer.transform.GetChild(0).gameObject;
            SpriteRenderer playerSprite = sprite.GetComponent<SpriteRenderer>();

            boxRenderer.flipX = !playerSprite.flipX;

            if (boxRenderer.flipX) boxRenderer.transform.position += new Vector3(0.4f, 0f, 0f);
            else boxRenderer.transform.position += new Vector3(-0.4f, 0f, 0f);

            HudManager.Instance.StartCoroutine(Effects.Lerp(1f, new Action<float>((p) => {
                
                    boxRenderer.sprite = getBoxAnimationSprite((int)(p * boxAnimationSprites.Length));
                    if (p == 1f) boxRenderer.sprite = getBoxAnimationSprite(0);
                    if ((int)(p * boxAnimationSprites.Length) == 15) {
                        GameObject.Destroy(slap);
                        return;
                    }
                
            })));            
        }
    }
}