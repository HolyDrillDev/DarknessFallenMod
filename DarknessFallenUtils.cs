﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI.Chat;

namespace DarknessFallenMod
{
    public static class DarknessFallenUtils
    {
        public const string OreGenerationMessage = "Darkness Fallen Ore Generation";
        public const string SoundsPath = "DarknessFallenMod/Sounds/";

        public static void BeginWithShaderOptions(this SpriteBatch spritebatch)
        {
            spritebatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void BeginWithDefaultOptions(this SpriteBatch spritebatch)
        {
            spritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void DrawProjectileInHBCenter(this Projectile projectile, Color lightColor, bool animated = false, Vector2? offset = null, Vector2? origin = null, Texture2D altTex = null, bool centerOrigin = false, float rotOffset = 0)
        {
            Texture2D texture = altTex ?? TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawOrigin;
            Rectangle? sourceRectangle = null;
            if (animated)
            {
                int frameHeight = texture.Height / Main.projFrames[projectile.type];

                drawOrigin = origin ?? (centerOrigin ? new Vector2(texture.Width / 2, frameHeight / 2) : new Vector2(texture.Width, frameHeight / 2));

                sourceRectangle = new Rectangle(0, frameHeight * projectile.frame + 1, texture.Width, frameHeight);
            }
            else
            {
                drawOrigin = origin ?? (centerOrigin ? texture.Size() * 0.5f : new Vector2(texture.Width, texture.Height / 2));
            }

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            if (offset.HasValue) drawPos += offset.Value.RotatedBy(projectile.rotation);

            Main.EntitySpriteDraw(
                texture,
                drawPos,
                sourceRectangle,
                lightColor,
                projectile.rotation + rotOffset,
                drawOrigin,
                projectile.scale,
                projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0
                );
        }

        public static void DrawAfterImage(this Projectile projectile, Func<float, Color> color, bool transitioning = true, bool animated = false, bool centerOrigin = true, Vector2? origin = null, Vector2 posOffset = default, float rotOffset = 0, Vector2 scaleOffset = default, bool oldRot = true, bool oldPos = true)
        {
            Texture2D tex = TextureAssets.Projectile[projectile.type].Value;

            int frameHeight = tex.Height / Main.projFrames[projectile.type];
            Rectangle? source = animated ? new Rectangle(0, frameHeight * projectile.frame + 1, tex.Width, frameHeight) : null;

            Vector2 drawOrigin = origin ?? (centerOrigin ? new Vector2(tex.Width * 0.5f, frameHeight * 0.5f) : tex.Size() * 0.5f);

            for (int i = 0; i < projectile.oldPos.Length; i++)
            {
                Vector2 pos = oldPos ? projectile.oldPos[i] : projectile.position;

                pos += posOffset;

                Main.EntitySpriteDraw(
                    tex,
                    pos + new Vector2(projectile.width, projectile.height) * 0.5f - Main.screenPosition,
                    source,
                    transitioning ? color.Invoke((float)i / projectile.oldPos.Length) * ((float)(projectile.oldPos.Length - i) / projectile.oldPos.Length) : color.Invoke((float)i / projectile.oldPos.Length),
                    (oldRot ? projectile.oldRot[i] : projectile.rotation) + rotOffset,
                    drawOrigin,
                    projectile.scale * Vector2.One + scaleOffset,
                    projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    0
                    );
            }
        }

        public static void OffsetShootPos(ref Vector2 position, Vector2 velocity, Vector2 offset)
        {
            Vector2 shootOffset = offset.RotatedBy(velocity.ToRotation());
            if (Collision.CanHit(position, 5, 5, position + shootOffset, 5, 5))
            {
                position += shootOffset;
            }
        }

        public enum TooltipLineEffectStyle
        {
            Epileptic
        }

        public static void DrawTooltipLineEffect(DrawableTooltipLine line, int x, int y, TooltipLineEffectStyle effectStyle)
        {
            switch (effectStyle)
            {
                case TooltipLineEffectStyle.Epileptic:
                    EpilepticEffect(line, new Vector2(x, y));
                    break;
            }
        }

        static void EpilepticEffect(DrawableTooltipLine line, Vector2 position)
        {
            float ind = 0.1f;
            for (int i = 0; i < 10; i++)
            {
                //float val = MathF.Abs(MathF.Sin(Main.GameUpdateCount * 0.05f + ind));
                float val = ind;
                ChatManager.DrawColorCodedStringWithShadow(
                    Main.spriteBatch,
                    line.Font,
                    line.Text,
                    position,
                    new Color(Main.rand.NextFloat(), Main.rand.NextFloat(), Main.rand.NextFloat()) * 0.5f * val,
                    0,
                    line.Origin,
                    Vector2.UnitX * val + Vector2.One
                    );
                ind += 0.1f;
            }
            
        }

        public static void ForeachNPCInRange(Vector2 center, float rangeSquared, Action<NPC> predicate)
        {
            Array.ForEach(Main.npc, npc =>
            {
                if (npc.DistanceSQ(center) <= rangeSquared)
                {
                    predicate.Invoke(npc);
                }
            });
        }

        public static void ForeachNPCInRectangle(Rectangle rectangle, Action<NPC> predicate)
        {
            Array.ForEach(Main.npc, npc =>
            {
                if (npc.Hitbox.Intersects(rectangle))
                {
                    predicate.Invoke(npc);
                }
            });
        }

        public static bool TryGetClosestEnemyNPC(Vector2 center, out NPC closest, float rangeSQ = float.MaxValue)
        {
            return TryGetClosestEnemyNPC(center, out closest, npc => false, rangeSQ);
        }

        public static bool TryGetClosestEnemyNPC(Vector2 center, out NPC closest, Func<NPC, bool> condition, float rangeSQ = float.MaxValue)
        {
            closest = null;
            NPC closestCondition = null;
            float minDistCondition = rangeSQ;
            float minDist = rangeSQ;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.CanBeChasedBy()) continue;
                float dist = npc.DistanceSQ(center);

                if (condition(npc) && dist < minDistCondition)
                {
                    closestCondition = npc;
                    minDistCondition = dist;
                }
                else if (dist < minDist)
                {
                    closest = npc;
                    minDist = dist;
                }
            }

            closest = closestCondition ?? closest;

            if (closest is null) return false;
            return true;
        }

        public static Vector2[] GetCircularPositions(this Vector2 center, float radius, int amount = 8, float rotation = 0)
        {
            if (amount < 2) return Array.Empty<Vector2>();

            Vector2[] postitions = new Vector2[amount];

            float angle = MathHelper.Pi * 2f / amount;
            angle += rotation;

            for (int i = 0; i < amount; i++)
            {
                Vector2 position = new Vector2(MathF.Cos(angle * i), MathF.Sin(angle * i));
                position *= radius;
                position += center;

                postitions[i] = position;
            }


            return postitions;
        }

        public static void SetTrophy(this ModTile modTile)
        {
            Main.tileFrameImportant[modTile.Type] = true;
            Main.tileLavaDeath[modTile.Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
            TileObjectData.addTile(modTile.Type);

            TileID.Sets.DisableSmartCursor[modTile.Type] = true;
            TileID.Sets.FramesOnKillWall[modTile.Type] = true;
        }

        public static void DropCustomBannerKillCount(this NPC npc, int killCount, int bannerItem)
        {
            if (NPC.killCount[npc.type] % killCount == 0 && !(NPC.killCount[npc.type] % 50 == 0)) Item.NewItem(npc.GetSource_Death(), npc.Hitbox, bannerItem);
        }

        public static void ShakeScreenInRange(float strenght, Vector2 center, float rangeSQ, float desolve = 0.95f)
        {
            foreach (Player player in Main.player)
            {
                if (player.DistanceSQ(center) < rangeSQ)
                {
                    player.GetModPlayer<DarknessFallenPlayer>().ShakeScreen(strenght, desolve);
                }
            }
        }

        public static void BasicAnimation(this Projectile proj, int speed)
        {
            BasicAnimation(proj, speed, 0, 0);
        }

        public static void BasicAnimation(this Projectile proj, int speed, int delay, int delayFrame)
        {
            proj.frameCounter++;
            if (proj.frameCounter >= Main.projFrames[proj.type] * speed + delay)
            {
                proj.frameCounter = 0;
            }

            if (proj.frameCounter > delay) proj.frame = (int)(proj.frameCounter - delay) / speed;
            else proj.frame = delayFrame;
        }

        public static void BasicAnimation(this NPC npc, int frameHeight, int speed)
        {
            BasicAnimation(npc, frameHeight, speed, 0, 0);
        }

        public static void BasicAnimation(this NPC npc, int frameHeight, int speed, int delay, int delayFrame)
        {
            npc.frameCounter++;
            if (npc.frameCounter >= Main.npcFrameCount[npc.type] * speed + delay)
            {
                npc.frameCounter = 0;
            }

            if (npc.frameCounter > delay) npc.frame.Y = ((int)(npc.frameCounter - delay) / speed) * frameHeight;
            else npc.frame.Y = delayFrame * frameHeight;
        }

        public static void NewDustCircular(
            Vector2 center,
            int dustType,
            float radius,
            Vector2 dustVelocity = default,
            int alpha = 0,
            Color color = default,
            float scale = 1, int amount = 8,
            float rotation = 0,
            float speedFromCenter = 0,
            bool? noGravity = null
            )
        {
            
            foreach(Vector2 pos in GetCircularPositions(center, radius, amount, rotation))
            {
                Vector2 velocity = dustVelocity;
                velocity += center.DirectionTo(pos) * speedFromCenter;
                int dust = Dust.NewDust(pos, 0, 0, dustType, velocity.X, velocity.Y, alpha, color, scale);
                Main.dust[dust].noGravity = noGravity ?? Main.dust[dust].noGravity;
            }
        }

        public static void NewGoreCircular(Vector2 center, int goreType, float radius, Vector2 goreVelocity = default, float scale = 1, int amount = 4, float rotation = 0, float speedFromCenter = 0, IEntitySource source = null)
        {
            foreach (Vector2 pos in GetCircularPositions(center, radius, amount, rotation))
            {
                Vector2 velocity = goreVelocity;
                velocity += center.DirectionTo(pos) * speedFromCenter;
                Gore.NewGore(source, pos, velocity, goreType, scale);
            }
        }

        public static void SpawnGoreOnDeath(this NPC npc, float speed = 2.5f, params string[] names)
        {
            if (npc.life <= 0)
            {
                foreach (string name in names)
                {
                    int gore = ModContent.Find<ModGore>(name).Type;
                    Gore.NewGore(npc.GetSource_Death(), npc.position, Main.rand.NextVector2Unit() * speed, gore);
                }
            }
        }

        public static void SpawnGoreOnDeath(this NPC npc, params string[] names)
        {
            if (Main.netMode == NetmodeID.Server) return;

            if (npc.life <= 0)
            {
                foreach (string name in names)
                {
                    int gore = npc.ModNPC.Mod.Find<ModGore>(name).Type;
                    Gore.NewGore(npc.GetSource_Death(), npc.position, Main.rand.NextVector2Unit() * 2.5f, gore);
                }
            }
        }

        public static void ManualFriendlyLocalCollision(this Projectile projectile)
        {
            if (projectile.friendly)
            {
                ForeachNPCInRectangle(projectile.Hitbox, npc =>
                {
                    if (!npc.friendly && npc.active && npc.life > 0 && projectile.localNPCImmunity[npc.whoAmI] <= 0)
                    {
                        npc.StrikeNPC(projectile.damage, projectile.knockBack, (int)(npc.Center.X - projectile.Center.X));
                        projectile.localNPCImmunity[npc.whoAmI] = projectile.localNPCHitCooldown;
                        projectile.penetrate--;
                    }
                });
            }
        }

        public static string GetColored(this string text, Color color)
        {
            return $"[c/{color.Hex3()}:{text}]";
        }

        public static void KillOldestProjectile(this Player player, int projType)
        {
            Projectile oldest = null;
            float minTimeLeft = float.MaxValue;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.type == projType && proj.owner == player.whoAmI && proj.timeLeft < minTimeLeft)
                {
                    oldest = proj;
                    minTimeLeft = proj.timeLeft;
                }
            }

            oldest?.Kill();
        }
        /*
        public static void ResizeBy(this Projectile projectile, int amountW, int amountH)
        {
            projectile.position = projectile.Center;
            projectile.width += amountW;
            projectile.height += amountH;
            projectile.Center = projectile.position;
        }

        public static void ResizeBy(this Projectile projectile, float amount, bool hitbox = true, bool scale = true)
        {
            if (hitbox)
            {
                projectile.position = projectile.Center;
                projectile.width += (int)(projectile.scale * amount);
                projectile.height += (int)(projectile.scale * amount);
                projectile.Center = projectile.position;
            }

            if (scale)
            {
                projectile.scale += amount;
            }
        }*/
    }
}
