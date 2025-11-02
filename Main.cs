using MelonLoader;
using MinHud;
using UnityEngine;
using TMPro;
using GHPC.Camera;
using UnityEngine.UI;
using GHPC.State;
using System.Collections;

[assembly: MelonInfo(typeof(Mod), "ATLAS' MinHud", "1.0.0", "ATLAS")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace MinHud
{
    internal class MinHudManager : MonoBehaviour 
    {
        private GameObject weapon_text;
        private GameObject drivetrain_text;
        private GameObject compass;
        private CameraManager cam_manager;
        private GHPC.Camera.BufferedCameraFollow buf_camera;
        private float cd = 0.02f;

        void Awake() 
        {
            Transform hud = GameObject.Find("_APP_GHPC_").transform.Find("UIHUDCanvas");
            weapon_text = hud.Find("weapons text").gameObject;
            drivetrain_text = hud.Find("drivetrain text").gameObject;
            compass = hud.Find("Compass").gameObject;
            cam_manager = CameraManager.Instance;
            buf_camera = cam_manager.CameraFollow;
        }

        void Update() 
        {
            if (!Mod.hide_in_gunner_view.Value) return;

            cd -= Time.deltaTime;

            if (cd > 0) return;

            bool in_3p = cam_manager._currentCamSlot == null
                || cam_manager._currentCamSlot.GetInstanceID() == cam_manager._allFreeLookCamSlots[0].GetInstanceID()
                || buf_camera.IsInFreeFlyMode;

            if (!Mod.only_hide_compass.Value)
            {
                weapon_text.SetActive(in_3p);
                drivetrain_text.SetActive(in_3p);
            }
            compass.SetActive(in_3p);

            cd = 0.02f;
        }
    }

    public class Mod : MelonMod
    {
        internal static MelonPreferences_Entry<bool> hide_in_gunner_view;
        internal static MelonPreferences_Entry<bool> only_hide_compass;

        private static IEnumerator Execute(GameState _)
        {
            GameObject app = GameObject.Find("_APP_GHPC_");

            if (app.GetComponent<MinHudManager>() != null) yield break;

            Transform hud = GameObject.Find("_APP_GHPC_").transform.Find("UIHUDCanvas");
            hud.Find("VersionTag/Version").gameObject.SetActive(false);
            hud.Find("weapons text/azimuth HUD").gameObject.SetActive(false);
            hud.Find("TimescaleValue").GetComponent<TextMeshProUGUI>().enabled = false;
            hud.Find("tooltip toggle text").GetComponent<Text>().enabled = false;
            app.AddComponent<MinHudManager>();

            yield break;
        }

        public override void OnInitializeMelon()
        {
            MelonPreferences_Category cfg = MelonPreferences.CreateCategory("MinHud");
            hide_in_gunner_view = cfg.CreateEntry<bool>("Hide HUD in gunner view", true);
            only_hide_compass = cfg.CreateEntry<bool>("Only hide compass (gunner view)", true);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (!GameObject.Find("_APP_GHPC_")) return;
            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Execute), GameStatePriority.Lowest);
        }
    }
}
