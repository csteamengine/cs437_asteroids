using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace asteroids
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteBatch spriteBatch;
        Model SpaceShip;
        Model Skybox;
        // The aspect ratio determines how to scale 3d to 2d projection.
        float aspectRatio;

        Vector3 xwingPosition = new Vector3(8, 1, -3);
        Quaternion xwingRotation = Quaternion.Identity;
        float gameSpeed = 1.0f;
        float boost = 1.0f;

        Matrix viewMatrix;
        Matrix projectionMatrix;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            graphics.PreferredBackBufferWidth = 500;
            graphics.PreferredBackBufferHeight = 500;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Window.Title = "Asteroids";


            base.Initialize();
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Skybox = Content.Load<Model>("skybox");
            // TODO: use this.Content to load your game content here
            SpaceShip = Content.Load<Model>("ship");
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private void UpdateCamera()
        {
            Vector3 campos = new Vector3(0, .1f, 1.1f);
            campos = Vector3.Transform(campos, Matrix.CreateFromQuaternion(xwingRotation));
            campos += xwingPosition;

            Vector3 camup = new Vector3(0, 1, 0);
            camup = Vector3.Transform(camup, Matrix.CreateFromQuaternion(xwingRotation));

            viewMatrix = Matrix.CreateLookAt(campos, xwingPosition, camup);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 0.2f, 500.0f);
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            // TODO: Add your update logic here
            ProcessKeyboard(gameTime);
            float moveSpeed = gameTime.ElapsedGameTime.Milliseconds / 500.0f * gameSpeed * boost;
            MoveForward(ref xwingPosition, xwingRotation, moveSpeed);

            UpdateCamera();
            base.Update(gameTime);
        }

        private void MoveForward(ref Vector3 position, Quaternion rotationQuat, float speed)
        {
            Vector3 addVector = Vector3.Transform(new Vector3(0, 0, -1), rotationQuat);
            position += addVector * speed;
        }

        private void ProcessKeyboard(GameTime gameTime)
        {
            float leftRightRot = 0;

            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            turningSpeed *= 1.6f * gameSpeed;
            KeyboardState keys = Keyboard.GetState();
            if (keys.IsKeyDown(Keys.Right))
                leftRightRot += turningSpeed;
            if (keys.IsKeyDown(Keys.Left))
                leftRightRot -= turningSpeed;

            float upDownRot = 0;
            if (keys.IsKeyDown(Keys.Up))
                upDownRot += turningSpeed;
            if (keys.IsKeyDown(Keys.Down))
                upDownRot -= turningSpeed;

            if (keys.IsKeyDown(Keys.Space))
            {
                boost = 4.0f;
            }
            else
            {
                boost = 1.0f;
            }

            Quaternion additionalRot = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), leftRightRot) * Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), upDownRot);
            xwingRotation *= additionalRot;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);
            DrawSkybox();
            DrawModel();

            base.Draw(gameTime);
        }

        private void DrawSkybox()
        {
            Matrix worldMatrix = Matrix.CreateScale(0.0005f, 0.0005f, 0.0005f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(xwingPosition);
            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[Skybox.Bones.Count];
            Skybox.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in Skybox.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
        }

        private void DrawModel()
        {
            Matrix worldMatrix = Matrix.CreateScale(0.0005f, 0.0005f, 0.0005f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateFromQuaternion(xwingRotation) * Matrix.CreateTranslation(xwingPosition);
            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[SpaceShip.Bones.Count];
            SpaceShip.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in SpaceShip.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
        }
    }
}
