using System;
using MelonLoader;
using UnityEngine;
using WebSocketSharp;
using UIExpansionKit.API;

namespace CuteCallout
{
    public class CuteCall : MelonMod
    {
        WebSocket ws;

        public override void OnApplicationStart()
        {
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.UserQuickMenu).AddSimpleButton("Call him cute!", () =>
            {
                doStuff();
            });

            ws = new WebSocket("ws://localhost:8080");
            ws.Connect();

            ws.OnMessage += (sender, e) =>
            {
                UnityEngine.UI.Text obj = GameObject.Find("UserInterface/UnscaledUI/HudContent/Hud/AlertTextParent/Text").GetComponent<UnityEngine.UI.Text>();
                obj.alignment = TextAnchor.LowerCenter;
                string[] splitArray = e.Data.Split(char.Parse(";"));
                if(RoomManager.field_Internal_Static_ApiWorldInstance_0.instanceId == splitArray[1])
                {
                    VRCUiManager.prop_VRCUiManager_0.Method_Public_Void_String_0(splitArray[0]+splitArray[2]);
                }
            };

            ws.OnError += (sender, e) => {
                MelonLogger.Msg("WebSocket Error has occured!");
            };
            ws.OnClose += (sender, e) => {
                MelonLogger.Msg("WebSocket has been closed!");
            };
        }

        public void doStuff()
        {
            if (ws != null)
            {
                var showName = GameObject.Find("UserInterface/QuickMenu").GetComponent<QuickMenu>().field_Private_APIUser_0.displayName;
                if (showName != null)
                {
                    var worldInstID = RoomManager.field_Internal_Static_ApiWorldInstance_0.instanceId;
                    ws.Send(showName + ";" + worldInstID);
                }
            }
        }
    }
}
