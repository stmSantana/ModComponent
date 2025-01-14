﻿using HarmonyLib;
using ModComponent.AssetLoader;
using System.Reflection;
using UnityEngine;

namespace ModComponent.Patches
{
	static class ResourcePatches
	{
		// Hinterland loads assets by calling Resources.Load which ignores external AssetBundles
		// so we need to patch Resources.Load to redirect specific calls to load from the AssetBundle instead
		// Most of the gear items are in the resources
		[HarmonyPatch]
		internal static class Resources_Load
		{
			static MethodBase TargetMethod()
			{
				MethodInfo[] methods = typeof(Resources).GetMethods();
				foreach (MethodInfo m in methods)
				{
					if (m.Name == "Load" && m.ReturnType == typeof(Object) && !m.IsGenericMethod && m.GetParameters().Length == 1)
					{
						return m;
					}
				}
				Logger.LogError("Resources.Load not found for patch.");
				return null;
			}
			internal static bool Prefix(ref string path, ref Object __result)
			{
				if (!ModAssetBundleManager.IsKnownAsset(path)) return true;

				__result = ModAssetBundleManager.LoadAsset(path);
				if (__result == null) Logger.LogWarning("Resources.Load failed to load the external asset");
				return false;
			}
		}

		//Hinterland stores many of its assets in asset bundles
		//This allows us to enable external asset loading in key locations
		//For example, paperdoll textures are loaded from asset bundles
		[HarmonyPatch(typeof(AssetBundle), "LoadAsset", new System.Type[] { typeof(string), typeof(Il2CppSystem.Type) })]
		internal static class AssetBundle_LoadAsset
		{
			private static bool Prefix(ref string name, ref Object __result)
			{
				if (!ModAssetBundleManager.IsKnownAsset(name)) return true;

				__result = ModAssetBundleManager.LoadAsset(name);
				if (__result == null) Logger.LogWarning($"AssetBundle.LoadAsset failed to load the external asset '{name}'");
				return false;
			}
		}

		//Just for testing
		/*[HarmonyPatch(typeof(UnityEngine.AssetBundle), "LoadAssetAsync", new System.Type[] { typeof(string), typeof(Il2CppSystem.Type) })]
        internal static class AssetBundle_LoadAssetAsync
        {
            private static void Postfix(string name, Il2CppSystem.Type type,AssetBundle __instance)
            {
                Implementation.Log("Tried to asyncronously load '{0}' of type '{1}' from '{2}'", name, type.ToString(),__instance.name);
            }
        }

        [HarmonyPatch(typeof(UnityEngine.AssetBundle), "LoadAllAssets", new System.Type[] { typeof(Il2CppSystem.Type) })]
        internal static class AssetBundle_LoadAllAssets
        {
            private static void Postfix(Il2CppSystem.Type type, AssetBundle __instance, UnhollowerBaseLib.Il2CppReferenceArray<UnityEngine.Object> __result)
            {
                Implementation.Log("Tried to load all assets of type '{0}' from '{1}'", type.ToString(), __instance.name);
                foreach(var obj in __result)
                {
                    Implementation.Log("Loaded '{0}'", obj.name);
                }
            }
        }*/
	}
}
