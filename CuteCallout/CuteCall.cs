using MelonLoader;
using UnityEngine;
using WebSocketSharp;
using UIExpansionKit.API;
using System.Collections;
using System.Text.RegularExpressions;

namespace CuteCallout
{
    public class CuteCall : MelonMod
    {
        WebSocket ws = new WebSocket("ws://localhost:8080");
        int retry = 0;
        bool fixRich = false;
        UnityEngine.UI.Text AlertText;

        public override void OnApplicationStart()
        {
            ws.Connect();

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.UserQuickMenu).AddSimpleButton("Call him cute!", () =>
            {
                if (ws != null)
                {
                    ws.Send(getOnlyName() + ";" + getOnlyID());
                }
            });

            ws.OnMessage += (sender, e) =>
            {
                AlertText = GameObject.Find("UserInterface/UnscaledUI/HudContent/Hud/AlertTextParent/Text").GetComponent<UnityEngine.UI.Text>();
                AlertText.alignment = TextAnchor.LowerCenter;
                AlertText.supportRichText = true;
                fixRich = true;
                string[] splitArray = e.Data.Split(char.Parse(";"));
                if(getOnlyID() == splitArray[1])
                {
                    VRCUiManager.prop_VRCUiManager_0.Method_Public_Void_String_0("<color=#5432a8>" + splitArray[0]+"</color> "+splitArray[2]);
                }
            };

            ws.OnError += (sender, e) => {
                MelonLogger.Msg("WebSocket Error has occured!");
            };
            ws.OnOpen += (sender, e) => {
                retry = 0;
                MelonLogger.Msg("Connected to server");
            };
            ws.OnClose += (sender, e) =>
            {
                MelonLogger.Msg("WebSocket has been closed!");
                MelonCoroutines.Start(Connecter());
            };
        }

        public override void OnUpdate()
        {
            if (RoomManager.prop_Boolean_0 && fixRich) // fix for richtext not dissapearing
            {
                if (AlertText.color.a < 0.3f && AlertText.supportRichText)
                {
                    AlertText.supportRichText = false;
                    fixRich = false;
                } else if (AlertText.color.a > 0.3f && !AlertText.supportRichText)
                {
                    AlertText.supportRichText = true;
                }
            }
        }

        IEnumerator Connecter() //if called, try to connect and wait
        {
            if (retry < 3)
            {
                retry++;
                yield return new WaitForSeconds(10);
                ws.Connect();
            }
            else
            {
                MelonLogger.Msg("The reconnecting has failed.");
            }
        }

        public string getOnlyID() //returns only instance ID wihout region,nonce etc...
        {
            string inputStr = RoomManager.field_Internal_Static_ApiWorldInstance_0.instanceId;
            if (inputStr.Length > 5)
            {
                return inputStr.Remove(5);
            } else
            {
                return inputStr;
            }
        }
        public string getOnlyName() //returns only name of the player, ommit steam id if present
        {
            string name = GameObject.Find("UserInterface/QuickMenu").GetComponent<QuickMenu>().field_Private_APIUser_0.displayName;
            if(Regex.IsMatch(GetLast(name, 5), " [a-z0-9]{4}")) //ew what's this? a regex that can fail???
            {
                return name.Remove(name.Length - 5);
            } else
            {
                return name;
            }
        }
        public string GetLast(string source, int tail_length)
        {
            if (tail_length >= source.Length)
                return source;
            return source.Substring(source.Length - tail_length);
        }
    }
}
