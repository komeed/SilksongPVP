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

        [HarmonyPrefix]
        static void Prefix(UIManager __instance, MainMenuState newState)
        {
            if (newState != MainMenuState.MAIN_MENU)
            {
                if (hostButton != null)
                {
                    UnityEngine.Object.Destroy(hostButton);
                    hostButton = null;
                }
                return;
            }

            if (hostButton != null)
            {
                SilksongModPlugin.Log.LogError("TEST BUTTON SHOULD BE NULL");
                return;
            }
            GameObject obj = GameObject.Find("StartGameButton");
            CreateHostButton(obj);
            //CreateJoinButton();
 
            LobbyManager.UpdateCurrSceneAndSend("MAINMENU");
        }

        public static GameObject CreateHostButton(GameObject obj)
        {
            hostButton = UnityEngine.Object.Instantiate(obj, obj.transform.parent);
            Object.DontDestroyOnLoad(hostButton);
            ((Component)hostButton.transform.GetChild(0)).GetComponent<Text>().text = "Invite To Lobby";
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
           // GlobalHost.HostEnabled = true;
            //SilksongModPlugin.Log.LogInfo($"HOST ENABLED?? {GlobalHost.HostEnabled}");
            SilksongModPlugin.LobbyManager.DisplayInvite();
        }
    }

    [HarmonyPatch(typeof(UIManager), "ShowMenu")]
    public static class CreatePauseInviteButton
    {
        private static GameObject inviteButton;
        [HarmonyPrefix]
        public static void Prefix(UIManager __instance, MenuScreen menu)
        {
            if (menu == __instance.pauseMenuScreen)
            {
                SilksongModPlugin.Log.LogInfo($"Opening Pause menu! from menu: {menu.gameObject.name}");
                if (inviteButton == null)
                {
                    SilksongModPlugin.Log.LogInfo("invite button doesn't exist, let me create it!");
                }
                else
                {
                    SilksongModPlugin.Log.LogInfo("invite button exists, but it's possible that it's wrong. lets see if it's wrong");
                }
                GameObject continueButton = menu.gameObject.transform.Find("Container/Controls/ContinueButton").gameObject;
                if (continueButton != null)
                {
                    inviteButton = CreateMenuButtons.CreateHostButton(continueButton);
                }
                else
                {
                    SilksongModPlugin.Log.LogError("couldn't find continue button. OOPS");
                }
            }
            else
            {
                SilksongModPlugin.Log.LogInfo("Showing menu that isn't pausemenu");
            }
        }
        static void PrintChildrenRecursive(Transform parent, int depth)
        {
            string indent = new string(' ', depth * 2);
            SilksongModPlugin.Log.LogInfo($"{indent}{parent.name}");

            foreach (Transform child in parent)
            {
                PrintChildrenRecursive(child, depth + 1);
            }
        }
    }
}