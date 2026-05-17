using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SilkShock
{
    // TODO - adjust the plugin guid as needed
    [BepInAutoPlugin(id: "io.github.aesteralias.silkshock")]
    public partial class SilkShockPlugin : BaseUnityPlugin
    {


        private void Awake()
        {
            // Put your initialization logic here
            Socket.OnOpen += Socket_OnOpen;
            
            Harmony h = new("Aesterias-SilkShock");
            h.PatchAll();

            Connect();

            Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        }

        private void Socket_OnOpen(object sender, EventArgs e)
        {
            Socket.Send(Exported_Default);
        }

        private const string Exported_Default = "{\"Event_Name\":\"\", \"Register_Events\":[{\"Name\":\"Damage\",\"Group\":\"Silksong\",\"Description\":\"On taking damage, scaling with damage taken\",\"Registered_Event\":true,\"Label_X\":\"Masks\",\"Min_X\":1,\"Max_X\":9,\"X_Precision\":0,\"Values\":{\"intensity\":{\"Setup\":24,\"Value_Type\":0,\"String_Output\":\"\",\"Static_Value\":20,\"Dynamic_Value\":{\"Min_X\":1,\"Max_X\":9,\"Min_Y\":0,\"Max_Y\":100,\"Keyframes\":[{\"Input\":1,\"Output\":20,\"Interpolation\":0},{\"Input\":3,\"Output\":40,\"Interpolation\":0}],\"T_Name\":\"\",\"Group\":\"\",\"Description\":\"\",\"Label_X\":\"Masks\",\"X_Precision\":0,\"Label_Y\":\"Intensity\",\"Y_Precision\":0,\"Clamp_Min\":0,\"Clamp_Max\":100}},\"duration\":{\"Setup\":24,\"Value_Type\":1,\"String_Output\":\"\",\"Static_Value\":1.5,\"Dynamic_Value\":{\"Min_X\":1,\"Max_X\":9,\"Min_Y\":0.1,\"Max_Y\":15,\"Keyframes\":[{\"Input\":1,\"Output\":1,\"Interpolation\":0},{\"Input\":3,\"Output\":3,\"Interpolation\":0}],\"T_Name\":\"\",\"Group\":\"\",\"Description\":\"\",\"Label_X\":\"Masks\",\"X_Precision\":0,\"Label_Y\":\"Duration\",\"Y_Precision\":1,\"Clamp_Min\":0.1,\"Clamp_Max\":15}},\"delay\":{\"Setup\":0,\"Value_Type\":2,\"String_Output\":\"\",\"Static_Value\":3,\"Dynamic_Value\":null},\"chance\":{\"Setup\":0,\"Value_Type\":3,\"String_Output\":\"\",\"Static_Value\":100,\"Dynamic_Value\":null},\"failure\":{\"Setup\":7,\"Value_Type\":4,\"String_Output\":\"\",\"Static_Value\":10,\"Dynamic_Value\":null}},\"Ids\":[],\"Selection\":0,\"Command\":0},{\"Name\":\"Death\",\"Group\":\"Silksong/\",\"Description\":\"On death scaling with how much damage killed you.\",\"Registered_Event\":true,\"Label_X\":\"Damage\",\"Min_X\":1,\"Max_X\":9,\"X_Precision\":0,\"Values\":{\"intensity\":{\"Setup\":24,\"Value_Type\":0,\"String_Output\":\"\",\"Static_Value\":50,\"Dynamic_Value\":{\"Min_X\":1,\"Max_X\":9,\"Min_Y\":0,\"Max_Y\":100,\"Keyframes\":[{\"Input\":1,\"Output\":40,\"Interpolation\":0},{\"Input\":3,\"Output\":70,\"Interpolation\":0}],\"T_Name\":\"\",\"Group\":\"\",\"Description\":\"\",\"Label_X\":\"Damage\",\"X_Precision\":0,\"Label_Y\":\"Intensity\",\"Y_Precision\":0,\"Clamp_Min\":0,\"Clamp_Max\":100}},\"duration\":{\"Setup\":24,\"Value_Type\":1,\"String_Output\":\"\",\"Static_Value\":4,\"Dynamic_Value\":{\"Min_X\":1,\"Max_X\":9,\"Min_Y\":0.1,\"Max_Y\":15,\"Keyframes\":[{\"Input\":1,\"Output\":4,\"Interpolation\":0},{\"Input\":3,\"Output\":8,\"Interpolation\":0}],\"T_Name\":\"\",\"Group\":\"\",\"Description\":\"\",\"Label_X\":\"Damage\",\"X_Precision\":0,\"Label_Y\":\"Duration\",\"Y_Precision\":1,\"Clamp_Min\":0.1,\"Clamp_Max\":15}},\"delay\":{\"Setup\":0,\"Value_Type\":2,\"String_Output\":\"\",\"Static_Value\":3,\"Dynamic_Value\":null},\"chance\":{\"Setup\":0,\"Value_Type\":3,\"String_Output\":\"\",\"Static_Value\":100,\"Dynamic_Value\":null},\"failure\":{\"Setup\":7,\"Value_Type\":4,\"String_Output\":\"\",\"Static_Value\":10,\"Dynamic_Value\":null}},\"Ids\":[],\"Selection\":0,\"Command\":0},{\"Name\":\"Bonk\",\"Group\":\"Silksong/\",\"Description\":\"When hard colliding with terrain.\",\"Registered_Event\":false,\"Label_X\":null,\"Min_X\":null,\"Max_X\":null,\"X_Precision\":null,\"Values\":{\"intensity\":{\"Setup\":24,\"Value_Type\":0,\"String_Output\":\"\",\"Static_Value\":15,\"Dynamic_Value\":null},\"duration\":{\"Setup\":24,\"Value_Type\":1,\"String_Output\":\"\",\"Static_Value\":0.3,\"Dynamic_Value\":null},\"delay\":{\"Setup\":0,\"Value_Type\":2,\"String_Output\":\"\",\"Static_Value\":3,\"Dynamic_Value\":null},\"chance\":{\"Setup\":0,\"Value_Type\":3,\"String_Output\":\"\",\"Static_Value\":100,\"Dynamic_Value\":null},\"failure\":{\"Setup\":7,\"Value_Type\":4,\"String_Output\":\"\",\"Static_Value\":10,\"Dynamic_Value\":null}},\"Ids\":[],\"Selection\":0,\"Command\":0}]}";
        private static readonly WebSocket Socket = new("ws://localhost:4569/Aes-DynamicShock/Event");
        private static long next_attempt;
        private static void Connect()
        {
            if (Socket.ReadyState != WebSocketState.Open)
            {
                if (DateTime.UtcNow.Ticks > next_attempt)
                {
                    next_attempt = DateTime.UtcNow.AddSeconds(10).Ticks;
                    Task.Run(() => Socket.Connect());
                }
            }
        }
        private static void Send(string msg)
        {
            if (Socket.ReadyState == WebSocketState.Open)
            {
                Socket.Send(msg);
            }
            else
            {
                Connect();
            }
        }


        [HarmonyPatch(typeof(PlayerData),nameof(PlayerData.TakeHealth))]
        class Take_Damge
        {
            static void Prefix(int amount, int ___health)
            {
                if (amount > 0)
                {
                    if (___health - amount <= 0)
                    {
                        Send($"{{\"Event_Name\":\"Silksong/Death\", \"Input\":{amount}}}");
                    }
                    else
                    {
                        Send($"{{\"Event_Name\":\"Silksong/Damage\", \"Input\":{amount}}}");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(DeliveryQuestItem), nameof(DeliveryQuestItem.TakeHit))]
        class Bonk
        {
            [HarmonyPatch([])]
            static void Prefix()
            {
                Send("{\"Event_Name\":\"Silksong/Bonk\"}");
            }
        }
    }

}
