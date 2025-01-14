using System.Text;
using BepInEx;
using UnityEngine;

namespace Rebug
{
    [BepInPlugin("Lofiat.Rebug", "Rebug", "1.0.0")]
    public class Rebug : BaseUnityPlugin
    {
        private void Start()
        {
            DebugHudStats.Instance.builder = new StringBuilder();
            DebugHudStats.Instance.gameObject.SetActive(true);
            DebugHudStats.Instance.fpsWarning.transform.localPosition = new Vector3(0, 35, 30);
        }
    }
}