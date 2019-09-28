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

using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Primitives;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.D2.Widgets.Logic
{
	public class D2IngameActorInfoDisplayLogic : ChromeLogic
	{
		readonly World world;
		readonly WorldRenderer worldRenderer;

		readonly D2PanelWidget panel;
		readonly LabelWidget label;
		readonly D2SpriteWidget preview;
		readonly D2ProgressBarWidget health;
		readonly LabelWidget dmgLabel;

		readonly D2LineWidget separator;
		readonly LabelWidget title;
		readonly LabelWidget line1a;
		readonly LabelWidget line1b;
		readonly LabelWidget line2a;
		readonly LabelWidget line2b;

		int selectionHash;
		Actor[] selectedActors = { };

		[ObjectCreator.UseCtor]
		public D2IngameActorInfoDisplayLogic(Widget widget, World world, WorldRenderer worldRenderer)
		{
			this.world = world;
			this.worldRenderer = worldRenderer;

			var textColor = Color.FromArgb(71, 71, 55);

			panel = widget.GetOrNull<D2PanelWidget>("PANEL");

			label = widget.GetOrNull<LabelWidget>("NAME");
			if (label != null)
			{
				label.Align = TextAlign.Center;
				label.TextColor = textColor;
				label.Font = "MediumBold";
			}

			preview = widget.GetOrNull<D2SpriteWidget>("ICON");

			health = widget.GetOrNull<D2ProgressBarWidget>("HEALTH");

			dmgLabel = widget.GetOrNull<LabelWidget>("DMG");
			if (dmgLabel != null)
			{
				dmgLabel.Text = "DMG";
				dmgLabel.Align = TextAlign.Center;
				dmgLabel.TextColor = textColor;
				dmgLabel.Font = "MediumBold";
			}

			title = widget.GetOrNull<LabelWidget>("TITLE");
			if (title != null)
			{
				title.Align = TextAlign.Center;
				title.TextColor = textColor;
				title.Font = "MediumBold";
			}

			separator = widget.GetOrNull<D2LineWidget>("SEPARATOR");

			line1a = widget.GetOrNull<LabelWidget>("LINE1A");
			if (line1a != null)
			{
				line1a.TextColor = textColor;
				line1a.Font = "MediumBold";
			}

			line1b = widget.GetOrNull<LabelWidget>("LINE1B");
			if (line1b != null)
			{
				line1b.Align = TextAlign.Right;
				line1b.TextColor = textColor;
				line1b.Font = "MediumBold";
			}

			line2a = widget.GetOrNull<LabelWidget>("LINE2A");
			if (line2a != null)
			{
				line2a.TextColor = textColor;
				line2a.Font = "MediumBold";
			}

			line2b = widget.GetOrNull<LabelWidget>("LINE2B");
			if (line2b != null)
			{
				line2b.Align = TextAlign.Right;
				line2b.TextColor = textColor;
				line2b.Font = "MediumBold";
			}
		}

		public override void Tick()
		{
			base.Tick();

			UpdateStateIfNecessary();
		}

		void HideExtraInfo()
		{
			if (title == null || separator == null || line1a == null || line1b == null || line2a == null || line2b == null)
				return;

			title.Visible = false;
			separator.Visible = false;
			line1a.Visible = false;
			line1b.Visible = false;
			line2a.Visible = false;
			line2b.Visible = false;
		}

		void UpdateSpiceInfo(Actor actor)
		{
			if (title == null || separator == null || line1a == null || line1b == null || line2a == null || line2b == null)
				return;

			var stores = actor.TraitsImplementing<StoresResources>();
			if (stores.Any())
			{
				var store = stores.FirstOrDefault();
				if (store != null)
				{
					title.Text = "SPICE";
					title.Visible = true;

					separator.Visible = true;

					line1a.Text = "HOLDS:";
					line1a.Visible = true;
					line1b.GetText = () => store.Stored.ToString();
					line1b.Visible = true;

					line2a.Text = "MAX:";
					line2a.Visible = true;
					line2b.GetText = () => (store as IStoreResources).Capacity.ToString();
					line2b.Visible = true;
				}
			}
		}

		void UpdatePowerInfo(Actor actor)
		{
			if (title == null || separator == null || line1a == null || line1b == null || line2a == null || line2b == null)
				return;

			var powers = actor.TraitsImplementing<Power>();
			if (powers.Any())
			{
				var power = powers.FirstOrDefault(t => !t.IsTraitDisabled);
				if (power != null)
				{
					if (power.Info.Amount > 0)
					{
						title.Text = "POWER INFO";
						title.Visible = true;

						separator.Visible = true;

						line1a.Text = "NEEDED:";
						line1a.Visible = true;
						line1b.GetText = () => (power.PlayerPower.PowerProvided != 0 ? power.PlayerPower.PowerDrained * power.GetEnabledPower() / power.PlayerPower.PowerProvided : 0).ToString();
						line1b.Visible = true;

						line2a.Text = "OUTPUT:";
						line2a.Visible = true;
						line2b.GetText = () => power.GetEnabledPower().ToString();
						line2b.Visible = true;
					}
				}
			}
		}

		void UpdateStateIfNecessary()
		{
			if (selectionHash == world.Selection.Hash)
				return;

			selectedActors = world.Selection.Actors
				.Where(a => a.IsInWorld && !a.IsDead)
				.ToArray();

			if (selectedActors.Length == 1)
			{
				var actor = selectedActors[0];
				if (label != null)
				{
					var tooltip = actor.TraitsImplementing<Tooltip>();
					if (tooltip.Any())
						label.Text = tooltip.FirstOrDefault(t => !t.IsTraitDisabled).Info.Name;
					else
						label.Text = actor.Info.Name;
				}

				var faction = world.LocalPlayer.Faction.Name;
				var rsi = actor.Info.TraitInfo<RenderSpritesInfo>();
				var icon = new Animation(world, rsi.GetImage(actor.Info, world.Map.Rules.Sequences, faction));
				var bi = actor.Info.TraitInfo<BuildableInfo>();
				icon.Play(bi.Icon);

				if (preview != null)
				{
					preview.Sprite = icon.Image;
					preview.Palette = worldRenderer.Palette(bi.IconPalette);
					preview.Scale = 3.0f;
					preview.Offset = float2.Zero;
				}

				if (health != null)
				{
					var healthTrait = actor.TraitsImplementing<Health>();
					if (healthTrait.Any())
					{
						health.GetPercentage = () =>
						{
							var h = healthTrait.FirstOrDefault();
							return (h != null) ? h.HP * 100 / h.MaxHP : 0;
						};
						health.Visible = true;
					}
					else
					{
						health.GetPercentage = () => 0;
						health.Visible = false;
					}
				}

				if (dmgLabel != null)
					dmgLabel.Visible = true;

				HideExtraInfo();
				UpdateSpiceInfo(actor);
				UpdatePowerInfo(actor);
			}
			else
			{
				if (preview != null)
					preview.Sprite = null;
				if (label != null)
					label.Text = "";
				if (health != null)
					health.Visible = false;
				if (dmgLabel != null)
					dmgLabel.Visible = false;

				HideExtraInfo();
			}

			selectionHash = world.Selection.Hash;
		}
	}
}
