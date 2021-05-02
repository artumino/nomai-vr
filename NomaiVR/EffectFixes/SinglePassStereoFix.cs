using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace NomaiVR
{
    internal class SinglePassStereoFix : NomaiVRModule<NomaiVRModule.EmptyBehaviour, SinglePassStereoFix.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => AllScenes;

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                Prefix<OWCamera>("RebuildGrabPass", nameof(PreRebuildGrabPass));
            }

            private static bool PreRebuildGrabPass(OWCamera __instance)
            {
                if (!__instance._mainCamera.stereoEnabled)
                    return true;

                if (__instance._grabPassCommandBuffer == null)
                {
                    __instance.InitializeGrabPass();
                }

                var desc = UnityEngine.XR.XRSettings.eyeTextureDesc;
                desc.colorFormat = __instance._mainCamera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.ARGB32;
                desc.depthBufferBits = 0;
                desc.enableRandomWrite = false;
                desc.memoryless = RenderTextureMemoryless.None;
                __instance._grabPassCommandBuffer.GetTemporaryRT(__instance._propID_OpaqueGrabTex, desc, FilterMode.Bilinear);
                __instance._grabPassCommandBuffer.CopyTexture(BuiltinRenderTextureType.CurrentActive, __instance._propID_OpaqueGrabTex);
                return false;
            }
        }
    }
}
