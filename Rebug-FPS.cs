using System.Reflection;
using System.Text;
using BepInEx;
using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR;

namespace Rebug_FPS
{
    [BepInPlugin("Lofiat.Graze.Rebug-FPS", "Rebug-FPS", "1.1.0")]
    public class Rebug : BaseUnityPlugin
    {
        XRDisplaySubsystem? xrDisplay;
        public static float maxFps;
        public static float halfFps;
        private void Start()
        {
            new Harmony("Rebug").PatchAll(Assembly.GetExecutingAssembly());
            GorillaTagger.OnPlayerSpawned(delegate
            {
                xrDisplay = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRDisplaySubsystem>();
                DebugHudStats.Instance.builder = new StringBuilder();
                DebugHudStats.Instance.gameObject.SetActive(true);
                DebugHudStats.Instance.fpsWarning.transform.localPosition = new Vector3(0, 35, 30);
                xrDisplay.TryGetDisplayRefreshRate(out float refresh);
                maxFps = refresh +1;
                halfFps = refresh / 2;
            });
        }
    }

    [HarmonyPatch(typeof(DebugHudStats))]
    [HarmonyPatch("Update", MethodType.Normal)]
    static class ShowMoreFpsPatch
    {
        static bool Prefix(DebugHudStats __instance)
        {
            bool flag = ControllerInputPoller.SecondaryButtonPress((XRNode)4);
            if (flag != __instance.buttonDown)
            {
                __instance.buttonDown = flag;
                if (!__instance.buttonDown)
                {
                    if (!((Component)(object)__instance.text).gameObject.activeInHierarchy)
                    {
                        ((Component)(object)__instance.text).gameObject.SetActive(value: true);
                        __instance.showLog = false;
                    }
                    else if (!__instance.showLog)
                    {
                        __instance.showLog = true;
                    }
                    else
                    {
                        ((Component)(object)__instance.text).gameObject.SetActive(value: false);
                    }
                }
            }

            if (__instance.firstAwake == 0f)
            {
                __instance.firstAwake = Time.time;
            }

            if (__instance.updateTimer < __instance.delayUpdateRate)
            {
                __instance.updateTimer += Time.deltaTime;
                return false;
            }
            else
            {
                __instance.builder.Clear();
                __instance.builder.Append("v: ");
                __instance.builder.Append(GorillaComputer.instance.version);
                __instance.builder.Append(":");
                __instance.builder.Append(GorillaComputer.instance.buildCode);
                int num = Mathf.RoundToInt(1f / Time.smoothDeltaTime);
                if (num < Rebug.halfFps)
                {
                    __instance.lowFps++;
                }
                else
                {
                    __instance.lowFps = 0;
                }

                ((Component)(object)__instance.fpsWarning).gameObject.SetActive(__instance.lowFps > 5);
                num = Mathf.Min(num, (int)Rebug.maxFps);
                __instance.builder.Append((num < Rebug.halfFps) ? " - <color=\"red\">" : " - <color=\"white\">");
                __instance.builder.Append(num);
                __instance.builder.AppendLine(" fps</color>");
                if (GorillaComputer.instance != null)
                {
                    __instance.builder.AppendLine(GorillaComputer.instance.GetServerTime().ToString());
                }
                else
                {
                    __instance.builder.AppendLine("Server Time Unavailable");
                }

                GroupJoinZone groupZone = GorillaTagger.Instance.offlineVRRig.zoneEntity.GroupZone;
                if (groupZone != __instance.lastGroupJoinZone)
                {
                    __instance.zones = groupZone.ToString();
                    __instance.lastGroupJoinZone = groupZone;
                }

                if (NetworkSystem.Instance.IsMasterClient)
                {
                    __instance.builder.Append("H");
                }

                if (NetworkSystem.Instance.InRoom)
                {
                    if (NetworkSystem.Instance.SessionIsPrivate)
                    {
                        __instance.builder.Append("Pri ");
                    }
                    else
                    {
                        __instance.builder.Append("Pub ");
                    }
                }
                else
                {
                    __instance.builder.Append("DC ");
                }

                __instance.builder.Append("Z: <color=\"orange\">");
                __instance.builder.Append(__instance.zones);
                __instance.builder.AppendLine("</color>");
                float magnitude = Player.Instance.AveragedVelocity.magnitude;
                __instance.builder.Append(Mathf.RoundToInt(magnitude));
                __instance.builder.AppendLine(" m/s");
                if (__instance.showLog)
                {
                    __instance.builder.AppendLine();
                    for (int i = 0; i < __instance.logMessages.Count; i++)
                    {
                        __instance.builder.AppendLine(__instance.logMessages[i]);
                    }
                }

                __instance.text.text = __instance.builder.ToString();
                __instance.updateTimer = 0f;
                return false; 
            }
        }
    }
}