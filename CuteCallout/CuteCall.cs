using MelonLoader;
using UnityEngine;
using WebSocketSharp;
using UIExpansionKit.API;
using System.Collections;
using System.Text.RegularExpressions;
using System;

namespace CuteCallout
{
    public class CuteCall : MelonMod
    {
        WebSocket ws;
        int retry = 0;
        UnityEngine.UI.Text AlertText;
        bool modEnabled = true;
        bool gotPing = true;
        object PingPongCor;
        internal static MelonPreferences_Entry<bool> modEnabledPref;
        internal static MelonPreferences_Entry<string> WSIP;
        public static GameObject uixButton;

        public override void OnApplicationStart()
        {
            //melonpreferences and UIEK stuff
            var category = MelonPreferences.CreateCategory("CuteCallout", "CuteCallout Settings");
            modEnabledPref = category.CreateEntry("modEnabledPref", true, is_hidden: true);
            WSIP = category.CreateEntry("WSIP", "localhost:8950");
            modEnabled = modEnabledPref.Value;
            ws = new WebSocket("ws://"+WSIP.Value);

            if (modEnabled)
            {
                MelonCoroutines.Start(Connecter());
                PingPongCor = MelonCoroutines.Start(PingPong());
            }

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.UserQuickMenu).AddSimpleButton("Call him cute!",
                new Action(() => CallOutButton()),
                new Action<GameObject>((gameObject) => { uixButton = gameObject; }));

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SettingsMenu).AddSimpleButton("Toogle Callout", ToogleMod);

            ws.OnMessage += (sender, e) => //if websocket recieved message, display it
            {
                AlertText = GameObject.Find("UserInterface/UnscaledUI/HudContent/Hud/AlertTextParent/Text").GetComponent<UnityEngine.UI.Text>();
                AlertText.alignment = TextAnchor.LowerCenter;
                AlertText.supportRichText = true;
                string[] splitArray = e.Data.Split(char.Parse(";"));
                if(getOnlyID() == splitArray[1])
                {
                    VRCUiManager.prop_VRCUiManager_0.Method_Public_Void_String_0("<color=#5432a8>" + splitArray[0]+"</color>"+splitArray[2]);
                } else if (e.Data == "Ping;Pong!")
                {
                    gotPing = true;
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
                if(modEnabled)
                {
                    MelonCoroutines.Start(Connecter());
                }
            };
        }

        public void CallOutButton()
        {
            uixButton.SetActive(modEnabled); //maybe move to "after ui init"
            if (modEnabled && retry == 0)
            {
                ws.Send(getOnlyName() + ";" + getOnlyID());
            }
            else
            {
                MelonLogger.Msg("Mod is disabled or not connected to server");
            }
        }

        public override void OnUpdate()
        {
            if (AlertText != null && modEnabled) // fix for richtext not dissapearing
            {
                if (AlertText.color.a < 0.2f && AlertText.supportRichText)
                {
                    AlertText.supportRichText = false;
                }
                else if (AlertText.color.a > 0.2f && !AlertText.supportRichText)
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
                ws.ConnectAsync(); //async so it doesn't freeze the game
            }
            else
            {
                MelonLogger.Msg("The reconnecting has failed.");
            }
        }

        IEnumerator PingPong() //checks if the connection is ok, ws.OnClose might not fire sometimes
        {
            while(true)
            {
                yield return new WaitForSeconds(120);
                if (modEnabled && retry == 0) //mod is enabled and we are not already trying to reconnect
                {
                    if (gotPing)
                    {
                        gotPing = false;
                        ws.Send("Ping;Pong?");
                    } else
                    {
                        MelonLogger.Msg("Didn't get pinged back, something is wrong... Trying to reconnect...");
                        ws.Close();
                    }
                }
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
            if(Regex.IsMatch(GetLast(name, 5), " [a-z0-9]{4}")) //ew what's this? a regex that can fail??? hell nah
            {
                return name.Remove(name.Length - 5);
            } else
            {
                return name;
            }
        }
        public string GetLast(string source, int tail_length) //if called, get X last charactes from string
        {
            if (tail_length >= source.Length)
                return source;
            return source.Substring(source.Length - tail_length);
        }

        public void ToogleMod() //toggles mod...
        {
            if(modEnabled)
            {
                modEnabled = false;
                MelonCoroutines.Stop(PingPongCor);
                ws.Close();
            } else
            {
                modEnabled = true;
                MelonCoroutines.Start(Connecter());
                PingPongCor = MelonCoroutines.Start(PingPong());
            }
            uixButton.SetActive(modEnabled); //toggles the "Call him out" button
            modEnabledPref.Value = modEnabled;
            MelonPreferences.Save();
        }
    }
}
