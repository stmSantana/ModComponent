﻿namespace ModComponentAPI
{
	public class ModResearchComponent : ModComponent
	{
		public SkillType SkillType = SkillType.None;
		public int TimeRequirementHours = 5;
		public int SkillPoints = 10;
		public int NoBenefitAtSkillLevel = 4;
		public string ReadAudio = "Play_ResearchBook";

		public ModResearchComponent(System.IntPtr intPtr) : base(intPtr) { }
	}
}