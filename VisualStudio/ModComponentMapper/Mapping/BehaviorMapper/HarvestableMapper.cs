﻿using ModComponentAPI;
using System;
using UnityEngine;

namespace ModComponentMapper.ComponentMapper
{
	internal static class HarvestableMapper
	{
		internal static void Configure(ModComponent modComponent) => Configure(modComponent.gameObject);
		internal static void Configure(GameObject prefab)
		{
			ModHarvestableComponent modHarvestableComponent = ModComponentUtils.ComponentUtils.GetComponent<ModHarvestableComponent>(prefab);
			if (modHarvestableComponent is null) return;

			Harvest harvest = ModComponentUtils.ComponentUtils.GetOrCreateComponent<Harvest>(modHarvestableComponent);
			harvest.m_Audio = modHarvestableComponent.Audio;
			harvest.m_DurationMinutes = modHarvestableComponent.Minutes;

			if (modHarvestableComponent.YieldNames.Length != modHarvestableComponent.YieldCounts.Length)
			{
				throw new ArgumentException("YieldNames and YieldCounts do not have the same length on gear item '" + modHarvestableComponent.name + "'.");
			}

			harvest.m_YieldGear = ModComponentUtils.ModUtils.GetItems<GearItem>(modHarvestableComponent.YieldNames, modHarvestableComponent.name);
			harvest.m_YieldGearUnits = modHarvestableComponent.YieldCounts;

			harvest.m_AppliedSkillType = SkillType.None;
			harvest.m_RequiredTools = ModComponentUtils.ModUtils.GetItems<ToolsItem>(modHarvestableComponent.RequiredToolNames, modHarvestableComponent.name);
			harvest.m_GunpowderYield = 0f;
		}
	}
}