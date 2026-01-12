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
        private static GameObject joinButton;

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
 
            LobbyManager.CurrScene = "MAINMENU"; // set scene to main menu
            LobbyManager.DeActivateHornets(); // ensure all hornets are not active in main menu
        }

        public static void CreateHostButton(GameObject obj)
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
        }
        
        public static void HostButtonPressed(BaseEventData data)
        {
           // GlobalHost.HostEnabled = true;
            //SilksongModPlugin.Log.LogInfo($"HOST ENABLED?? {GlobalHost.HostEnabled}");
            SilksongModPlugin.LobbyManager.DisplayInvite();
        }

        private static void CreateJoinButton()
        {
            GameObject obj = GameObject.Find("StartGameButton");
            //SilksongModPlugin.Log.LogInfo($"TOPMOST PARENT: {topmostParent.name}");
            joinButton = UnityEngine.Object.Instantiate(obj, obj.transform.parent);
            
            ((Component)joinButton.transform.GetChild(0)).GetComponent<Text>().text = "Join";
            Component[] textComponents = joinButton.transform.GetChild(0).GetComponents<Component>();
            string textR = string.Join(", ", textComponents.Select(c => c.ToString()));
            SilksongModPlugin.Log.LogInfo("Text Components: " + textR);

            EventTrigger trig = joinButton.GetComponent<EventTrigger>();
            UnityEngine.Object.Destroy(trig);
        }
    }
}