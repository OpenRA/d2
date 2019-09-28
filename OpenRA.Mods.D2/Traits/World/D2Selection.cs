#region Copyright & License Information
/*
 * Copyright 2007-2019 The d2 mod Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Traits
{
	public class D2SelectionInfo : ITraitInfo, ILobbyOptions
	{
		[Translate]
		[Desc("Descriptive label for the selection checkbox in the lobby.")]
		public readonly string CheckboxLabel = "Single Selection";

		[Translate]
		[Desc("Tooltip description for the selection checkbox in the lobby.")]
		public readonly string CheckboxDescription = "Allow to select only one unit at a time";

		[Desc("Default value of the selection checkbox in the lobby.")]
		public readonly bool CheckboxEnabled = false;

		[Desc("Prevent the selection enabled state from being changed in the lobby.")]
		public readonly bool CheckboxLocked = false;

		[Desc("Whether to display the selection checkbox in the lobby.")]
		public readonly bool CheckboxVisible = true;

		[Desc("Display order for the selection checkbox in the lobby.")]
		public readonly int CheckboxDisplayOrder = 0;

		IEnumerable<LobbyOption> ILobbyOptions.LobbyOptions(Ruleset rules)
		{
			yield return new LobbyBooleanOption("singleselection", CheckboxLabel, CheckboxDescription,
				CheckboxVisible, CheckboxDisplayOrder, CheckboxEnabled, CheckboxLocked);
		}

		public object Create(ActorInitializer init) { return new D2Selection(init.World, this); }
	}

	public class D2Selection : Selection
	{
		readonly D2SelectionInfo info;
		readonly World world;

		bool initialized = false;
		bool singleSelection = true;
		public bool SingleSelection { get { return singleSelection; } }

		public D2Selection(World world, D2SelectionInfo info)
			: base(new SelectionInfo())
		{
			this.world = world;
			this.info = info;
		}

		void Init()
		{
			if (!initialized)
			{
				var gs = world.LobbyInfo.GlobalSettings;
				singleSelection = gs.OptionOrDefault("singleselection", info.CheckboxEnabled);
				initialized = true;
			}
		}

		public override void Add(Actor a)
		{
			Init();

			if (SingleSelection)
				Clear();

			base.Add(a);
		}

		public override void Combine(World world, IEnumerable<Actor> newSelection, bool isCombine, bool isClick)
		{
			Init();

			if (SingleSelection)
			{
				isClick = true;
				isCombine = false;
			}

			base.Combine(world, newSelection, isCombine, isClick);
		}
	}
}
