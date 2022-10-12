﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace DarknessFallenMod.Items.MeleeWeapons.MoltenUchigatana
{
    public class MoltenUchigatanaProjectile : ModProjectile
    {
        public override string Texture => "DarknessFallenMod/Items/MeleeWeapons/MoltenUchigatana/MoltenUchigatana";

        Player Player => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 14;
        }

        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;

            Projectile.knockBack = 8;

            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 9999;
            Projectile.ownerHitCheck = true;
            Projectile.penetrate = -1;
            Projectile.MaxUpdates = 3;

            Projectile.usesLocalNPCImmunity = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * Player.itemAnimationMax - 10;
            Projectile.rotation = Projectile.velocity.ToRotation() - (MathHelper.Pi * Player.direction * swingDirection);
            swingDirection = -swingDirection;
        }

        static int swingDirection = 1;
        Vector2 rotatedDirection;
        public override void AI()
        {
            // PLAYER AI
            if (Player.ItemAnimationEndingOrEnded)
            {
                //Main.NewText(MoltenUchigatana.speedMultiplier);
                //Main.NewText(Player.itemAnimationMax);
                if (MoltenUchigatana.speedMultiplier < MoltenUchigatana.maxSpeedMult) MoltenUchigatana.speedMultiplier += Player.itemAnimationMax / 240f;
                else MoltenUchigatana.speedMultiplier = MoltenUchigatana.maxSpeedMult;

                Projectile.Kill();
                return;
            }

            Player.heldProj = Projectile.whoAmI;
            Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
            Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

            // PROJECTILE AI
            Projectile.Center = Player.RotatedRelativePoint(Player.MountedCenter);

            float swingBy = MathF.Pow((float)Player.itemAnimation * MoltenUchigatana.speedMultiplier / (Player.itemAnimationMax * MoltenUchigatana.speedMultiplier), 4) * 0.2f * MoltenUchigatana.speedMultiplier;
            Projectile.rotation += swingBy * Player.direction * swingDirection;

            rotatedDirection = Projectile.rotation.ToRotationVector2();

            Projectile.direction = Player.direction;
            Projectile.spriteDirection = Projectile.direction;

            // FX
            if (swingBy > 0.01f) SpawnSpinDust(1, -13);
        }

        float bladeLenght => TextureAssets.Projectile[Type].Value.Width * 1.414213562373095f;
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Player.MountedCenter, Player.MountedCenter + rotatedDirection * bladeLenght))
            {
                SpawnSpinDust(14);
                return true;
            }
            return false;
        }

        void SpawnSpinDust(int amount, float speed = 10)
        {
            for (int i = 0; i < amount; i++)
            {
                Vector2 vel = rotatedDirection.RotatedBy(MathHelper.PiOver2 * Player.direction) * speed * Main.rand.NextFloat(0.75f, 1.25f);

                Dust.NewDustDirect(
                    Projectile.Center + rotatedDirection * ((float)i / amount) * bladeLenght + Main.rand.NextFloat(-bladeLenght, bladeLenght) * rotatedDirection / amount, 
                    0,
                    0, 
                    DustID.AmberBolt, 
                    vel.X, 
                    vel.Y ,
                    Scale: Main.rand.NextFloat(0.4f, 1f),
                    Alpha: Main.rand.Next(0, 120)
                    ).noGravity = true;

                Dust.NewDustDirect(
                    Projectile.Center + rotatedDirection * ((float)i / amount) * bladeLenght + Main.rand.NextFloat(-bladeLenght, bladeLenght) * rotatedDirection / amount,
                    0,
                    0,
                    DustID.FireflyHit,
                    vel.X,
                    vel.Y,
                    Scale: Main.rand.NextFloat(0.4f, 1f),
                    Alpha: Main.rand.Next(0, 120)
                    ).noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 texSize = tex.Size();

            float drawRotOffset = (Player.direction == -1 ? -MathHelper.Pi - MathHelper.PiOver4 : MathHelper.PiOver4);
            Vector2 drawPosOffset = new Vector2(0f, Projectile.gfxOffY) + rotatedDirection * bladeLenght * 0.5f;

            // After Image
            Main.spriteBatch.BeginReset(DarknessFallenUtils.BeginType.Shader, DarknessFallenUtils.BeginType.Default, s =>
            {
                Projectile.DrawAfterImage(
                    prog => Color.Lerp(Color.Yellow, Color.Black, prog) * 0.15f,
                    rotOffset: i => drawRotOffset,
                    posOffset: i => -Projectile.Center + Player.MountedCenter + Projectile.oldRot[i].ToRotationVector2() * bladeLenght * 0.5f,
                    altTex: ModContent.Request<Texture2D>(Texture + "AfterImage").Value
                    );
            });

            // Blade
            Main.EntitySpriteDraw(
                tex,
                Player.MountedCenter - Main.screenPosition + drawPosOffset,
                null,
                lightColor,
                Projectile.rotation + drawRotOffset,
                texSize * 0.5f,
                1,
                Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0
                );

            return false;
        }
    }
}
