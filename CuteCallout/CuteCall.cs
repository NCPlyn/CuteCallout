using System;
using System.Linq;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using UnhollowerRuntimeLib.XrefScans;
using System.Reflection;
using System.Collections;
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
                VRCUiManager.prop_VRCUiManager_0.Method_Public_Void_String_0(e.Data);
            };
        }

        public void doStuff()
        {
            if (ws != null)
            {
                var showName = GameObject.Find("UserInterface/QuickMenu").GetComponent<QuickMenu>().field_Private_APIUser_0.displayName;
                if (showName != null)
                {
                    ws.Send(showName);
                }
            }
        }
    }
}
