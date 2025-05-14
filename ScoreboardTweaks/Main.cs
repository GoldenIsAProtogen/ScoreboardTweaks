using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace ScoreboardTweaks
{
    [BepInPlugin(ModConstants.ModConstants.modGUID, ModConstants.ModConstants.modName, ModConstants.ModConstants.modVersion)]
    public class Main : BaseUnityPlugin
    {
        internal static Main m_hInstance = null;
        internal static List<Text> m_listScoreboardTexts = new List<Text>();
        internal static Sprite m_spriteGizmoMuted = null;
        internal static Sprite m_spriteGizmoOriginal = null;
        internal static Material m_materialReportButtons = null;
        public   static List<GorillaScoreBoard> m_listScoreboards = new List<GorillaScoreBoard>();
        internal static void Log(string msg) => m_hInstance.Logger.LogMessage(msg);
        void Awake()
        {
            m_hInstance = this;
            HarmonyPatcher.Patch.Apply();
        }
        void Start()
        {
            foreach (var plugin in Chainloader.PluginInfos.Values)
            {
                try { AccessTools.Method(plugin.Instance.GetType(), "OnScoreboardTweakerStart")?.Invoke(plugin.Instance, new object[] { }); } catch { }
            }

            Texture2D tex = new Texture2D(2, 2);
            var file = new FileInfo(AssemblyDirectory + "/gizmo-speaker-muted.png");
            if (!file.Exists) { Log("MutedGizmo icon file doesn`t exists!"); return; }
            tex.LoadImage(File.ReadAllBytes(file.FullName));

            m_spriteGizmoMuted = Sprite.Create(tex, new Rect(0.0f, 0.0f, 512.0f, 512.0f), new Vector2(0.5f, 0.5f), 100.0f);
            m_spriteGizmoMuted.name = "gizmo-speaker-muted";
        }
        public static void UpdateScoreboardTopText(string roomCode = null)
        {
            if (PhotonNetwork.InRoom) foreach (var txt in m_listScoreboardTexts)
            {
                txt.text = "ROOM ID: " + (!PhotonNetwork.CurrentRoom.IsVisible ? "-PRIVATE-" : (roomCode == null ? PhotonNetwork.CurrentRoom.Name : roomCode)) + "\nPLAYER                    REPORT";
            }
        }

        /* https://stackoverflow.com/questions/52797/how-do-i-get-the-path-of-the-assembly-the-code-is-in */
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }

    //[HarmonyPatch(typeof(GorillaNetworking.PhotonNetworkController))]
    //[HarmonyPatch("AttemptDisconnect", MethodType.Normal)]
    internal class OnRoomDisconnected
    {
        private static void Prefix()
        {
            try { Main.m_listScoreboardTexts.Clear(); } catch { }
        }
    }
}
