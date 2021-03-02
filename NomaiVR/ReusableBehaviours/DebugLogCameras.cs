using UnityEngine;

namespace NomaiVR
{
    internal class DebugLogCameras : MonoBehaviour
    {
        internal void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                LogEnabledCameras();
            }
        }

        public static void LogEnabledCameras()
        {
            Camera[] enabledCameras = Camera.allCameras;

            foreach (Camera camera in enabledCameras)
            {
                Transform disabledParent = null;

                string path = "";
                Transform parent = camera.transform;
                while (parent != null)
                {
                    path = "/" + parent.name + path;
                    parent = parent.parent;

                    if (disabledParent != null && !parent.gameObject.activeSelf)
                        disabledParent = parent;
                }
                Logs.Write($"{path} is {camera.enabled}");
                if (disabledParent != null)
                    Logs.Write($"Found disabled parent {disabledParent.name}");
            }
        }
    }
}
