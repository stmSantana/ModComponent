﻿using HarmonyLib;
using ModComponent.Mapper;

namespace ModComponent.Patches
{
	[HarmonyPatch(typeof(GameManager), "Awake")]
	internal static class GameManager_Awake
	{
		private static void Postfix()
		{
			//
			//Need to be called after GameManager is initialized
			//

			AlternativeToolManager.ProcessList();
		}
	}
}