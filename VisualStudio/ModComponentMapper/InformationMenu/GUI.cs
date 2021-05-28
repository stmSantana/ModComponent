﻿using System.Collections.Generic;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using Il2Cpp = Il2CppSystem.Collections.Generic;

namespace ModComponentMapper.InformationMenu
{
	internal class GUI : MonoBehaviour
	{

		static GUI()
		{
			UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<GUI>();
		}

		public GUI(System.IntPtr ptr) : base(ptr) { }

		internal readonly Dictionary<string, ModTab> modTabs = new Dictionary<string, ModTab>();
		internal ModTab currentTab = null;
		internal int selectedIndex = 0;
		internal string previousMod = null;

		internal ConsoleComboBox modSelector;
		internal UIPanel scrollPanel;
		internal Transform scrollPanelOffset;
		internal GameObject scrollBar;
		internal UISlider scrollBarSlider;

		[HideFromIl2Cpp]
		internal void Build()
		{
			Transform contentArea = transform.Find("GameObject");

			DestroyOldSettings(contentArea);
			modSelector = CreateModSelector(contentArea);
			scrollPanel = CreateScrollPanel(contentArea);
			scrollPanelOffset = CreateOffsetTransform(scrollPanel);
			scrollBar = CreateScrollBar(scrollPanel);
			scrollBarSlider = scrollBar.GetComponentInChildren<UISlider>(true);
		}

		[HideFromIl2Cpp]
		internal static void DestroyOldSettings(Transform contentArea)
		{
			int settingsCount = contentArea.childCount;
			for (int i = settingsCount - 1; i >= 2; --i)
			{
				Transform setting = contentArea.GetChild(i);
				setting.parent = null;
				Destroy(setting.gameObject);
			}
		}

		[HideFromIl2Cpp]
		internal ConsoleComboBox CreateModSelector(Transform contentArea)
		{
			ConsoleComboBox modSelector = contentArea.Find("Quality").GetComponent<ConsoleComboBox>();
			modSelector.items.Clear();

			// Make the combobox a lot larger
			int xOffset = 200;
			Vector3 offset = new Vector3(xOffset, 0);

			Transform transform = modSelector.transform;
			transform.localPosition -= offset;
			transform.Find("Button_Increase").localPosition += offset;
			transform.Find("Label_Value").localPosition += offset / 2f;
			Transform background = transform.Find("Console_Background");
			background.localPosition += offset / 2f;
			background.GetComponent<UISprite>().width += xOffset;

			EventDelegate.Set(modSelector.onChange, new System.Action(() => SelectMod(modSelector.value)));
			return modSelector;
		}

		[HideFromIl2Cpp]
		internal static UIPanel CreateScrollPanel(Transform contentArea)
		{
			UIPanel panel = NGUITools.AddChild<UIPanel>(contentArea.gameObject);
			panel.gameObject.name = "ScrollPanel";
			panel.baseClipRegion = new Vector4(0, 0, 2000, 520);
			panel.clipOffset = new Vector2(500, -260);
			panel.clipping = UIDrawCall.Clipping.SoftClip;
			panel.depth = 100;
			return panel;
		}

		[HideFromIl2Cpp]
		internal static Transform CreateOffsetTransform(UIPanel scrollPanel)
		{
			GameObject offset = NGUITools.AddChild(scrollPanel.gameObject);
			offset.name = "Offset";
			return offset.transform;
		}

		[HideFromIl2Cpp]
		internal GameObject CreateScrollBar(UIPanel scrollPanel)
		{
			// Terrible code in this method. I've given up trying to fix their scroll bar layout
			Panel_CustomXPSetup xpModePanel = InterfaceManager.m_Panel_CustomXPSetup;
			GameObject scrollbarParent = xpModePanel.m_Scrollbar;
			GameObject scrollbarPrefab = scrollbarParent.transform.GetChild(0).gameObject;
			GameObject scrollbar = NGUITools.AddChild(gameObject, scrollbarPrefab);
			scrollbar.name = "Scrollbar";
			scrollbar.transform.localPosition = new Vector2(415, -40);

			int height = (int)scrollPanel.height;
			UISlider slider = scrollbar.GetComponentInChildren<UISlider>(true);
			slider.backgroundWidget.GetComponent<UISprite>().height = height;
			slider.foregroundWidget.GetComponent<UISprite>().height = height;
			scrollbar.transform.Find("glow").GetComponent<UISprite>().height = height + 44;

			EventDelegate.Set(slider.onChange, new System.Action(() => OnScroll(slider, true)));

			return scrollbar;
		}

		internal void Update()
		{
			if (currentTab is null)
				return;
			if (InputManager.GetEscapePressed(InterfaceManager.m_Panel_OptionsMenu))
			{
				InterfaceManager.m_Panel_OptionsMenu.OnCancel();
				return;
			}

			InterfaceManager.m_Panel_OptionsMenu.UpdateMenuNavigationGeneric(ref selectedIndex, currentTab.menuItems);
			EnsureSelectedSettingVisible();
			UpdateDescriptionLabel();

			if (currentTab.scrollBarHeight > 0)
			{
				float scroll = InputManager.GetAxisScrollWheel(InterfaceManager.m_Panel_OptionsMenu);
				float scrollAmount = 60f / currentTab.scrollBarHeight;
				if (scroll < 0)
				{
					scrollBarSlider.value += scrollAmount;
				}
				else if (scroll > 0)
				{
					scrollBarSlider.value -= scrollAmount;
				}
			}
		}

		[HideFromIl2Cpp]
		internal void OnScroll(UISlider slider, bool playSound)
		{
			scrollPanelOffset.localPosition = new Vector2(0, slider.value * (currentTab?.scrollBarHeight ?? 0));
			if (playSound)
			{
				GameAudioManager.PlayGUIScroll();
			}
		}

		[HideFromIl2Cpp]
		internal void SelectMod(string modName)
		{
			if (currentTab != null)
			{
				currentTab.uiGrid.gameObject.active = false;
			}

			selectedIndex = 0;
			currentTab = modTabs[modName];
			currentTab.uiGrid.gameObject.active = true;

			ResizeScrollBar(currentTab);
			EnsureSelectedSettingVisible();
		}

		[HideFromIl2Cpp]
		internal void UpdateDescriptionLabel()
		{
			GameObject setting = currentTab.menuItems[selectedIndex];
			DescriptionHolder description = setting.GetComponent<DescriptionHolder>();

			if (description is null)
				return;

			UILabel descriptionLabel = InterfaceManager.m_Panel_OptionsMenu.m_OptionDescriptionLabel;
			descriptionLabel.text = description.Text;
			descriptionLabel.transform.parent = setting.transform;
			descriptionLabel.transform.localPosition = new Vector3(655, 0);
			descriptionLabel.gameObject.SetActive(true);
		}

		[HideFromIl2Cpp]
		internal void EnsureSelectedSettingVisible()
		{
			if (Utils.GetMenuMovementVertical(InterfaceManager.m_Panel_OptionsMenu, true, false) == 0f)
				return;

			if (selectedIndex == 0)
			{
				scrollBarSlider.value = 0;
				return;
			}

			GameObject setting = currentTab.menuItems[selectedIndex];
			float settingY = -setting.transform.localPosition.y;
			float scrollPanelTop = scrollPanelOffset.localPosition.y + GUIBuilder.gridCellHeight;
			float scrollPanelBottom = scrollPanelOffset.localPosition.y + scrollPanel.height - GUIBuilder.gridCellHeight;

			if (settingY < scrollPanelTop)
			{
				scrollBarSlider.value += (settingY - scrollPanelTop) / currentTab.scrollBarHeight;
				GameAudioManager.PlayGUIScroll();
			}
			else if (settingY > scrollPanelBottom)
			{
				scrollBarSlider.value += (settingY - scrollPanelBottom) / currentTab.scrollBarHeight;
				GameAudioManager.PlayGUIScroll();
			}
		}

		[HideFromIl2Cpp]
		internal void Enable(Panel_OptionsMenu parentMenu)
		{
			GameAudioManager.PlayGUIButtonClick();
			parentMenu.SetTabActive(gameObject);
		}

		internal void OnEnable()
		{
			ModComponentMenu.SetPagesVisible(true);

			if (modSelector.items.Count > 0)
			{
				modSelector.items.Sort();

				string modToSelect = modSelector.items.Contains(previousMod) ? previousMod : modSelector.items[0];
				modSelector.value = modToSelect;
				SelectMod(modToSelect);
			}
		}

		internal void OnDisable()
		{
			ModComponentMenu.SetPagesVisible(false);

			previousMod = modSelector?.value;
			foreach (ModTab tab in modTabs.Values)
			{
				tab.requiresConfirmation = false;
			}
			SetConfirmButtonVisible(false);
		}

		[HideFromIl2Cpp]
		internal void SetConfirmButtonVisible(bool value)
		{
			InterfaceManager.m_Panel_OptionsMenu.m_SettingsNeedConfirmation = value;
		}

		[HideFromIl2Cpp]
		internal ModTab CreateModTab(string modName)
		{
			UIGrid grid = CreateUIGrid(modName);
			Il2Cpp.List<GameObject> menuItems = new Il2Cpp.List<GameObject>();
			menuItems.Add(modSelector.gameObject);
			ModTab modTab = new ModTab(grid, menuItems);

			grid.onReposition = new System.Action(() => ResizeScrollBar(modTab));

			modTabs.Add(modName, modTab);
			return modTab;
		}

		[HideFromIl2Cpp]
		internal void AddModSelector(string modName)
		{
			modSelector.items.Add(modName);
		}

		[HideFromIl2Cpp]
		internal void RemoveModSelector(string modName)
		{
			modSelector.items.Remove(modName);
		}

		[HideFromIl2Cpp]
		internal UIGrid CreateUIGrid(string modName)
		{
			UIGrid uiGrid = NGUITools.AddChild<UIGrid>(scrollPanelOffset.gameObject);
			uiGrid.gameObject.name = "Mod settings grid (" + modName + ")";
			uiGrid.gameObject.SetActive(false);
			uiGrid.arrangement = UIGrid.Arrangement.Vertical;
			uiGrid.cellHeight = GUIBuilder.gridCellHeight;
			uiGrid.hideInactive = true;
			return uiGrid;
		}

		[HideFromIl2Cpp]
		internal void ResizeScrollBar(ModTab modTab)
		{
			int childCount = modTab.uiGrid.GetChildList().Count;

			if (modTab == currentTab)
			{
				float absoluteVal = scrollBarSlider.value * modTab.scrollBarHeight;

				float height = childCount * GUIBuilder.gridCellHeight;
				modTab.scrollBarHeight = height - scrollPanel.height;

				ScrollbarThumbResizer thumbResizer = scrollBarSlider.GetComponent<ScrollbarThumbResizer>();
				thumbResizer.SetNumSteps((int)scrollPanel.height, (int)height);

				scrollBarSlider.value = Mathf.Clamp01(absoluteVal / Mathf.Max(1, modTab.scrollBarHeight));
				OnScroll(scrollBarSlider, false);
			}
			else
			{
				modTab.scrollBarHeight = childCount * GUIBuilder.gridCellHeight - scrollPanel.height;
			}

			scrollBar.SetActive(currentTab.scrollBarHeight > 0);
		}
	}
}
