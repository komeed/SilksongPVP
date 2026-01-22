using System.Linq;
using System.Text;
using GlobalEnums;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SilksongMod
{
    [HarmonyPatch(typeof(UIManager), "SetMenuState")]
    public static class CreateMenuButtons
    {
        private static GameObject hostButton;
        public static GameObject ContinueInviteButton;

        [HarmonyPrefix]
        static void Prefix(UIManager __instance, MainMenuState newState)
        {
            if (newState == MainMenuState.SAVE_PROFILES)
            {
                LobbySwitch.SetActive(true);
            }
            else
            {
                LobbySwitch.SetActive(false);
            }
            SilksongModPlugin.Log.LogInfo("new menu state: " + newState);
            if (newState == MainMenuState.PAUSE_MENU && !LobbyManager.isGlobalLobby)
            {
                
                GameObject continueButton = GameObject.Find("ContinueButton");
                if (continueButton != null)
                {
                    SilksongModPlugin.Log.LogInfo("Found conitnue button!");
                    ContinueInviteButton = CreateHostButton(continueButton);
                }
                else
                {
                    SilksongModPlugin.Log.LogInfo("Couldn't find continue button");
                }
            }

            if (newState != MainMenuState.MAIN_MENU)
            {
                if (hostButton != null)
                {
                    UnityEngine.Object.Destroy(hostButton);
                    hostButton = null;
                }

                return;
            }
            LobbyManager.showingFullMap = false; // safety
            LobbyManager.showingQuickMap = false;
            if (LobbyManager.isGlobalLobby)
            {
                LobbyManager.ClearLobbyID();
                //show lobby after leaving
                LobbyManager.LeaveButtonPressed();
             //   LobbyDisplay.SetPanelActive(true);
                LobbySwitch.SetToggleOff();
                LobbyManager.isGlobalLobby = false; // just to be sure
            }

            if (hostButton != null)
            {
                SilksongModPlugin.Log.LogError("TEST BUTTON SHOULD BE NULL");
                return;
            }

            GameObject obj = GameObject.Find("StartGameButton");
            hostButton = CreateHostButton(obj);

            LobbyManager.UpdateCurrSceneAndSend("MAINMENU");
        }

        public static GameObject CreateHostButton(GameObject obj)
        {
            GameObject hostButton = UnityEngine.Object.Instantiate(obj, obj.transform.parent);
            Object.DontDestroyOnLoad(hostButton);
            Text text = ((Component)hostButton.transform.GetChild(0)).GetComponent<Text>();
            text.text = "Invite To Lobby";
            LobbyManager.DefaultFont = text.font;
            //destroy previous eventtrigger
            Object.Destroy(hostButton.GetComponent<EventTrigger>());
            EventTrigger trig = hostButton.AddComponent<EventTrigger>();
            trig.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();

            var clickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            clickEntry.callback.AddListener(HostButtonPressed);
            trig.triggers.Add(clickEntry);

            var submitEntry = new EventTrigger.Entry { eventID = EventTriggerType.Submit };
            submitEntry.callback.AddListener(HostButtonPressed);
            trig.triggers.Add(submitEntry);
            return hostButton;
        }

        public static void HostButtonPressed(BaseEventData data)
        {
            SilksongModPlugin.LobbyManager.DisplayInvite();
        }
    }

    [HarmonyPatch(typeof(UIManager), "ShowMenu")]
    public static class CreatePauseInviteButton
    {
        private static GameObject inviteButton;
        [HarmonyPostfix]
        public static void Postfix(UIManager __instance, MenuScreen menu)
        {
            Object.Destroy(CreateMenuButtons.ContinueInviteButton); // destroy it first
        }
    }

    [HarmonyPatch(typeof(PauseMenuButton), "OnSubmit")]
    public static class PauseMenuButtonPatch
    {
        private static GameObject inviteButton;

        [HarmonyPrefix]
        public static bool Prefix(PauseMenuButton __instance, BaseEventData eventData)
        {
            if (__instance.pauseButtonType == PauseMenuButton.PauseButtonType.Continue &&
                __instance.gameObject.name == "ContinueButton(Clone)")
            {
                __instance.flashEffect.ResetTrigger("Flash");
                __instance.flashEffect.SetTrigger("Flash");
                __instance.ForceDeselect();
                return false;
            }

            return true;
        }
    }
}