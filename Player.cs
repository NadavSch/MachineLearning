using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using GameCode.GameSystem.Input;
using static GameCode.GameSystem.Input.InputManager;
using GameCode.Utility;
using GameCode.WorldStructure;
using GameCode.GameSystem.Camera;
using GameCode.TwoDPhysics;

namespace GameCode.Player
{
    public class SPlayer : IPosition
    {
        private BoxSensor sensor;
        private Vector2 velocity;
        public Vector2 position;
        private float maxSpeed;
        private float maxBoostSpeed;
        private float acc;
        private float dec;
        private float boostAcc;
        private float airDec;
        private float airAcc;
        private float gravity;
        private float weakGravity;
        private float strongGravity;
        private float wallGravity;
        private float jumpPower;
        private float maxFall;
        private float skid;
        private bool isBoost;
        private bool isJumping;
        private bool canJump;
        private bool isWalled;
        private ButtonsByID currentInput;
        private ButtonsByID lastInput;
        private int moveDir;
        private bool cancelJump;
        private int width;
        private int height;
        private PlayerSoul ps;
        private Vector2 dirVector;

        public Vector2 CPosition => (position + new Vector2(32, 32));
        public Vector2 Velocity => velocity;

        public SPlayer()
        {
            velocity = Vector2.Zero;
            position = new Vector2(10, 200);
            sensor = new BoxSensor(new RectF(10, 200, 64, 64),null);
            acc = GLOBAL.PLAYER_ACC;
            dec = GLOBAL.PLAYER_DEC;
            airDec = GLOBAL.PLAYER_AIR_DEC;
            airAcc = GLOBAL.PLAYER_AIR_ACC;
            boostAcc = 3300.0f;
            maxSpeed = 500.0f;
            maxBoostSpeed = 800.0f;
            wallGravity = 1000.0f;
            weakGravity = 3000.0f;
            strongGravity = 12000.0f;
            gravity = weakGravity;
            jumpPower = GLOBAL.PLAYER_JUMP_POWER;
            wallGravity = GLOBAL.PLAYER_WALLGRAVITY;
            isBoost = false;
            isJumping = false;
            cancelJump = false;
            canJump = true;
            width = 64;
            height = 64;
            ps = new PlayerSoul();
            dirVector = new Vector2();
            maxFall = jumpPower;
            isWalled = false;
        }
        public void SetParams(float acc=0f, float dec = 0f, float airDec = 0f, float airAcc = 0f, float jumpPower = 0f,float wallGravity=0f)
        {
            this.acc = acc == 0f? this.acc:acc;
            this.dec = dec == 0f ? this.dec : dec;
            this.airDec = airDec == 0f ? this.airDec : airDec;
            this.airAcc = airAcc == 0f ? this.airAcc : airAcc;
            this.jumpPower = jumpPower == 0f ? this.jumpPower : jumpPower;
            this.wallGravity = wallGravity == 0f ? this.wallGravity : wallGravity;
        }
        public void Ps_setParam(float acc = 0f, float dec = 0f, float airDec = 0f, float airAcc = 0f, float throwPower = 0f, float time = 0f, float gravity = 0f,float ratio=0f)
        {
            ps.setParams(acc, dec, airDec, airAcc, throwPower, time, gravity,ratio);
        }
        public float GetJumpPower()
        {
            return jumpPower;
        }
        internal void Draw(SpriteBatch sb)
        {
            sb.Draw(ColorFrame.Box,position, new Rectangle(0,0, 64, 64), Color.Blue);
            ps.Draw(sb);
            return;
        }
        internal void HandleInput(ButtonsByID buttonsByID)
        {
            currentInput = buttonsByID;
        }
        internal void LoadContent(ContentManager content)
        {
            GLOBAL.world.AddPlayer(this);
            return;
        }
        private bool isGrounded()
        {
            if (velocity.Y < 0) return false;
            return sensor.isGrounded;
        }
        private void CheckMaxSpeed()
        {
            float curMaxSpeed;
            if (isBoost)
                curMaxSpeed = maxBoostSpeed;
            else
                curMaxSpeed = maxSpeed;
            if(velocity.X > 0)
            {
                if (velocity.X > curMaxSpeed) velocity.X = curMaxSpeed;
            }
            else if(velocity.X < 0)
            {
                if (velocity.X < -curMaxSpeed) velocity.X = -curMaxSpeed;
            }
            if(Velocity.Y > 0)
            {
                if (Velocity.Y > maxFall) velocity.Y = maxFall;
            }
        }
        private void ApplyGravity(float dt)
        {
            if (isGrounded())
            {
                canJump = true;
                isJumping = false;
                velocity.Y = 0;
                return;
            }

            velocity.Y += gravity * dt;
        }
        private void ApplyJump()
        {
            if (isJumping)
                velocity.Y = -jumpPower;
            if (isGrounded()) isJumping = false;
        }
        private void SetVelocity(float dt)
        {
            if (moveDir == 0 && velocity.X != 0)
            {
                int decDir = velocity.X > 0 ? -1 : 1;
                if (isGrounded())
                    velocity.X += decDir * dec * dt;
                else
                    velocity.X += decDir * airDec * dt;
                if (decDir < 0 && velocity.X < 0 || decDir > 0 && velocity.X > 0)
                    velocity.X = 0;
                if (Math.Abs(velocity.X) < 0.01) velocity.X = 0;
            }
            else
            {
                if (isGrounded())
                    if (!isBoost)
                        velocity.X += moveDir * acc * dt;
                    else
                        velocity.X += moveDir * boostAcc * dt;
                else
                    velocity.X += moveDir * airAcc * dt;
            }
        }
        internal void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Get Grounded,Celling,BlockedR,BlockedL from BoxSensor.CheckCollision
            StartInput(); //Select what to do
            SetVelocity(dt);
            CheckMaxSpeed();
            ApplyGravity(dt);
            ApplyJump();
            ApplyAndCheckVelocity(dt);
            sensor.SetLocation(position);
            if (!ps.isOut)
                SetPsPosition();
            ps.Update(gameTime);
            return;
        }

        private void ApplyAndCheckVelocity(float dt)
        {
            Vector2 collisionPos = sensor.CheckCollision(position, velocity * dt);
            if (sensor.EmergencyState)
                velocity = Vector2.Zero;
            if (sensor.isCelling)
                velocity.Y = 0;
            if (velocity.Y > 0 && ((sensor.isWalledL) || (sensor.isWalledR)))
            {
                if (!isWalled)
                    velocity.Y = 0;
                gravity = wallGravity;
                isWalled = true;
            }
            else
            {
                gravity = weakGravity;
                isWalled = false;
            }
            if (sensor.isWalledL || sensor.isWalledR)
                velocity.X = 0;
            if (collisionPos != Vector2.Zero)
                Console.WriteLine("{0} : isGrounded={1} | isCelling={2} | isBlockedR={3} | isBlockedL={4}", collisionPos, sensor.isGrounded, sensor.isCelling, sensor.isWalledR, sensor.isWalledL);
            if (collisionPos == Vector2.Zero)
                position += velocity * dt;
            else
            {
                position = collisionPos;
            }
        }

        private void StartInput()
        {
            moveDir = 0;
            if (!currentInput[GMButtons.Y])
            {
                if (currentInput[GMButtons.Left])
                    moveDir -= 1;
                if (currentInput[GMButtons.Right])
                    moveDir += 1;
            }
            cancelJump = true;
            if (currentInput[GMButtons.A])
            {
                if (canJump)
                {
					ApplyJump();
                }
            }

            if (currentInput[GMButtons.R2])
                isBoost = true;
            else
                isBoost = false;

            if (currentInput[GMButtons.Y] && !ps.isOut)
            {
                SetPsPosition();
                ps.setThrown(currentInput.L_AnalogVector * GLOBAL.SOUL_THROW_FORCE);
            }

            if (currentInput[GMButtons.X] && ps.isOut)
            {
                ps.isOut = false;
                RectF fixedPos = ps.TryExpand(10);
                if (!fixedPos.IsEmpty)
                {
                    position = fixedPos.Location;
                    velocity.Y = velocity.Y > 0 ? 0 : velocity.Y;
                    velocity = Vector2.Lerp(velocity, ps.velocity, 0.2f);
                    velocity.Y = velocity.Y>0?0:velocity.Y;
                }
            }
            lastInput = currentInput;
            return;
        }
        public IPosition GetSoulIPosition()
        {
            return ps;
        }
        private void SetPsPosition()
        {
            float x = position.X + (GLOBAL.PLAYER_SIZE / 2) - (GLOBAL.SOUL_SIZE / 2);
            float y = position.Y + (GLOBAL.PLAYER_SIZE / 2) - (GLOBAL.SOUL_SIZE / 2);
            ps.position.X = x;
            ps.position.Y = y;
            ps.velocity = Vector2.Zero;
        }
    }
}
