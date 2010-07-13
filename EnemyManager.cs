﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Typocalypse
{
    public class EnemyManager:GameComponent
    {
        //TODO:Refactor magic numbers
        private const int YPosition = -50;
        private readonly Random randGenerator;
        private int generateInterval = 3000;
        private TimeSpan lastTimeGenerated;
        public static readonly List<Enemy> enemies = new List<Enemy>();
        private readonly Player player;
        public EnemyInputManager InputManager{ get; set;}
        public event EventHandler PlayerHit;
        public bool IsActive { get; set; }
        private List<string> wordList;
        public int DifficultyBias { get; set; }
        private int checkpointInterval = 10;
        private int nextCheckpointScore;

        public EnemyManager(Game game, Player player, List<string> wordList):base(game)
        {
            randGenerator = new Random();
            this.player = player;
            IsActive = true;
            InputManager = new EnemyInputManager(wordList, game);
            this.wordList = wordList;
            DifficultyBias = 20;
            nextCheckpointScore = checkpointInterval;
        }

        Enemy GenerateEnemy()
        {
            return new Enemy(Game, GeneratePosition(), MathHelper.ToRadians(0), 0.9f, "zombie", 0.5f, "dev",player);
        }

        Vector2 GeneratePosition()
        {
            return new Vector2(randGenerator.Next(Game.GraphicsDevice.PresentationParameters.BackBufferWidth), YPosition);
        }

        public override void Update(GameTime gameTime)
        {
            //TODO:refactor below if statements
            if (!IsActive)
            {
                return;
            }
            if (lastTimeGenerated.TotalMilliseconds == 0)
            {
                lastTimeGenerated = gameTime.TotalGameTime;
            }
            if (gameTime.TotalGameTime.TotalMilliseconds - lastTimeGenerated.TotalMilliseconds < generateInterval)
            {
                return;
            }
            lastTimeGenerated = gameTime.TotalGameTime;
            var newEnemy = GenerateEnemy();
            newEnemy.TextBox = InputManager.RegisterEnemy(newEnemy, DifficultyBias);
            wordList.Remove(newEnemy.TextBox.Text);
            enemies.Add(newEnemy);
            Game.Components.Add(newEnemy);
            newEnemy.Dead += (s, e) =>
                                 {
                                     enemies.Remove((Enemy) s);
                                     player.EnemyKilled(10);
                                 };
            if (player.Score >= nextCheckpointScore)
            {
                nextCheckpointScore += checkpointInterval;
                DifficultyBias -= 1;
            }
            base.Update(gameTime);
        }

        public void PlayerIsHit() 
        {
            IsActive = false;

            foreach (var enemy in enemies)
            {
                enemy.IsActive = false;
            }
        }
    }
}