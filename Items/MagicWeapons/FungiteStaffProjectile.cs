﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria.Audio;
using System.Runtime.CompilerServices;

namespace DarknessFallenMod.Items.MagicWeapons
{
    // This Example show how to implement simple homing projectile
    // Can be tested with ExampleCustomAmmoGun
    public class FungiteStaffProjectile : ModProjectile
    {
        public object dust { get; private set; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fungite Staff"); // Name of the projectile. It can be appear in chat
            Main.projFrames[Projectile.type] = 5; //number of frames in the animation;
        }

        // Setting the default parameters of the projectile
        // You can check most of Fields and Properties here https://github.com/tModLoader/tModLoader/wiki/Projectile-Class-Documentation
        public override void SetDefaults()
        {
            Projectile.width = 24; // The width of projectile hitbox
            Projectile.height = 12; // The height of projectile hitbox
            Projectile.light = 0.75f;
            Projectile.aiStyle = 0; // The ai style of the projectile (0 means custom AI). For more please reference the source code of Terraria
            Projectile.DamageType = DamageClass.Magic; // What type of damage does this projectile affect?
            Projectile.friendly = true; // Can the projectile deal damage to enemies?
            Projectile.hostile = false; // Can the projectile deal damage to the player?
            Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?
            Projectile.light = 0.6f; // How much light emit around the projectile
            Projectile.tileCollide = true; // Can the projectile collide with tiles?
            Projectile.timeLeft = 600; // The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
        }


        // Custom AI

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Dig);
            return true;
        }

        public void AnimateProjectile() // Call this every frame, for example in the AI method.
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 3) // This will change the sprite every 8 frames (0.13 seconds). Feel free to experiment.
            {
                Projectile.frame++;
                Projectile.frame %= 5; // Will reset to the first frame if you've gone through them all.
                Projectile.frameCounter = 0;
            }
        }
        public override void Kill(int timeLeft) //this is caled whenever the projectile expires (only once);
        {
            for (int i = 0; i <= 10; i++) //repeats 50 times;
            {
                Random x = new Random();
                int X = x.Next(-60, 60); //these 2 lines create a random number between -60 and 60
                Random y = new Random();
                int Y = y.Next(-60, 60);  //these 2 lines create another random number between -60 and 60

                Dust.NewDust(new Vector2(Projectile.position.X + 5 + X, Projectile.position.Y + 5 + Y), 8, 8, 3);
            }
        }
    }
}