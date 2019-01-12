#region Copyright & License Information
/*
 * Copyright 2007-2018 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Traits.Render
{
	[Desc("Default trait for rendering sprite-based actors.")]
	public class D2WithSpriteBodyInfo : PausableConditionalTraitInfo, Requires<RenderSpritesInfo>
	{
		[Desc("Animation to play when the actor is created."), SequenceReference]
		public readonly string StartSequence = null;

		[Desc("Animation to play when the actor is idle."), SequenceReference]
		public readonly string Sequence = "idle";

		[Desc("Identifier used to assign modifying traits to this sprite body.")]
		public readonly string Name = "body";

		public override object Create(ActorInitializer init) { return new D2WithSpriteBody(init, this); }
	}

	public class D2WithSpriteBody : PausableConditionalTrait<D2WithSpriteBodyInfo>, INotifyDamageStateChanged, INotifyBuildComplete, IAutoMouseBounds
	{
		public readonly Animation DefaultAnimation;
		readonly RenderSprites rs;
		readonly Animation boundsAnimation;

		public D2WithSpriteBody(ActorInitializer init, D2WithSpriteBodyInfo info)
			: this(init, info, () => 0) { }

		protected D2WithSpriteBody(ActorInitializer init, D2WithSpriteBodyInfo info, Func<int> baseFacing)
			: base(info)
		{
			rs = init.Self.Trait<RenderSprites>();

			Func<bool> paused = () => IsTraitPaused &&
				DefaultAnimation.CurrentSequence.Name == NormalizeSequence(init.Self, Info.Sequence);

			DefaultAnimation = new Animation(init.World, rs.GetImage(init.Self), baseFacing, paused);
			rs.Add(new AnimationWithOffset(DefaultAnimation, null, () => IsTraitDisabled));

			// Cache the bounds from the default sequence to avoid flickering when the animation changes
			boundsAnimation = new Animation(init.World, rs.GetImage(init.Self), baseFacing, paused);
			boundsAnimation.PlayRepeating(info.Sequence);

			if (info.StartSequence != null)
				PlayCustomAnimation(init.Self, info.StartSequence,
					() => PlayCustomAnimationRepeating(init.Self, info.Sequence));
			else
				DefaultAnimation.PlayRepeating(NormalizeSequence(init.Self, info.Sequence));
		}

		public string NormalizeSequence(Actor self, string sequence)
		{
			return RenderSprites.NormalizeSequence(DefaultAnimation, self.GetDamageState(), sequence);
		}

		protected virtual void OnBuildComplete(Actor self)
		{
			DefaultAnimation.PlayRepeating(NormalizeSequence(self, Info.Sequence));
		}

		// TODO: Get rid of INotifyBuildComplete in favor of using the condition system
		void INotifyBuildComplete.BuildingComplete(Actor self)
		{
			OnBuildComplete(self);
		}

		public void PlayCustomAnimation(Actor self, string name, Action after = null)
		{
			DefaultAnimation.PlayThen(NormalizeSequence(self, name), () =>
			{
				CancelCustomAnimation(self);
				if (after != null)
					after();
			});
		}

		public void PlayCustomAnimationRepeating(Actor self, string name)
		{
			var sequence = NormalizeSequence(self, name);
			DefaultAnimation.PlayThen(sequence, () => PlayCustomAnimationRepeating(self, sequence));
		}

		public void PlayCustomAnimationBackwards(Actor self, string name, Action after = null)
		{
			DefaultAnimation.PlayBackwardsThen(NormalizeSequence(self, name), () =>
			{
				CancelCustomAnimation(self);
				if (after != null)
					after();
			});
		}

		public void CancelCustomAnimation(Actor self)
		{
			DefaultAnimation.PlayRepeating(NormalizeSequence(self, Info.Sequence));
		}

		protected virtual void DamageStateChanged(Actor self)
		{
			if (DefaultAnimation.CurrentSequence != null)
				DefaultAnimation.ReplaceAnim(NormalizeSequence(self, DefaultAnimation.CurrentSequence.Name));
		}

		void INotifyDamageStateChanged.DamageStateChanged(Actor self, AttackInfo e)
		{
			DamageStateChanged(self);
		}

		Rectangle IAutoMouseBounds.AutoMouseoverBounds(Actor self, WorldRenderer wr)
		{
			return boundsAnimation.ScreenBounds(wr, self.CenterPosition, WVec.Zero, rs.Info.Scale);
		}
	}
}
