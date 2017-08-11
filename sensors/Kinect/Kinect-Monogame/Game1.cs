using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Timers;



namespace Game1
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Texture to draw
        Texture2D videoTexture;
        Texture2D sceneContent;
        Texture2D images;

        // Active Kinect sensor
        private KinectSensor kinectSensor;
        

        // Reader for color frames
        private ColorFrameReader colorFrameReader;

        // Intermediate storage for receiving frame data from the sensor
        private byte[] colorPixels;

        private CoordinateMapper coordinateMapper = null;

        private BodyFrameReader bodyFrameReader = null;
        private Body[] bodies = null;
        private List<Tuple<JointType, JointType>> bones;

        static WaveGesture _gesture = new WaveGesture();
        private bool mygest = false;

        private SpriteFont font;
        private int score = 0;

        Texture2D circle;
        Texture2D circle2;
        Texture2D mario;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
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
            kinectSensor = KinectSensor.GetDefault();

            // Open the reader for the color frames
            colorFrameReader =
             kinectSensor.ColorFrameSource.OpenReader();


            // get the coordinate mapper
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // Specify a handler for frame arrival
            colorFrameReader.FrameArrived += Reader_ColorFrameArrived;

            bodyFrameReader.FrameArrived += Reader_BodyFrameArrived;

            // Create the ColorFrameDescription using rgba format
            FrameDescription desc = kinectSensor.ColorFrameSource.
             CreateFrameDescription(ColorImageFormat.Rgba);

            // Allocate space to put the pixels to be rendered
            colorPixels = new byte[desc.Width * desc.Height *
             desc.BytesPerPixel];

            _gesture.GestureRecognized += Gesture_GestureRecognized;

            // Open the sensor
            kinectSensor.Open();

            // Create texture large enough to hold the color frame
            videoTexture = new Texture2D(graphics.GraphicsDevice,
             desc.Width, desc.Height);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("MyFont"); // Use the name of your sprite font file here instead of 'Score'.
            circle = Content.Load<Texture2D>("mubamr");
            circle2 = Content.Load<Texture2D>("mubamr");
            mario = Content.Load<Texture2D>("mario2");

            using (var client = new WebClient())
            {
                client.DownloadFile("https://uosassetstore.blob.core.windows.net/assetstoredev/58307358fe5384300ec1f5ce/father%20graduation%20portrait.jpg", "a.mpeg");
            }

            FileStream fileStream = new FileStream("a.mpeg", FileMode.Open);
            images = Texture2D.FromStream(GraphicsDevice, fileStream);
            fileStream.Dispose();
            // TODO: use this.Content to load your game content here
        }

        void Gesture_GestureRecognized(object sender, EventArgs e)
        {
            mygest = true;
            Timer aTimer = new Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 5000;
            aTimer.Enabled = true;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            mygest = false;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            if (colorFrameReader != null)
            {
                colorFrameReader.Dispose();
                colorFrameReader = null;
            }

            if (kinectSensor != null)
            {
                kinectSensor.Close();
                kinectSensor = null;
            }
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

            base.Update(gameTime);
        }



        //Kinect Event Handlers
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    // Copy color frame into the array
                    colorFrame.CopyConvertedFrameDataToArray(
                     colorPixels,
                     ColorImageFormat.Rgba);

                    // Avoid exception when SetData method is used
                    GraphicsDevice.Textures[0] = null;

                    // Put pixel data into a texture
                    videoTexture.SetData(colorPixels);
                }
            }
        }

        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (BodyFrame BodyFrame = e.FrameReference.AcquireFrame())
            {
                if (BodyFrame != null)
                {

                    if (this.bodies == null)
                    {
                        this.bodies = new Body[BodyFrame.BodyCount];
                    }

                    // Copy color frame into the array
                    BodyFrame.GetAndRefreshBodyData(bodies);

                    //if (bodies[0] != null)
                   // {
                        _gesture.Update(bodies[0]);
                    //}
                }
            }
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (videoTexture != null)
            {
                // Draw color video
                spriteBatch.Begin();
                spriteBatch.Draw(videoTexture, new Rectangle(0, 0,
                 graphics.GraphicsDevice.Viewport.Width,
                 graphics.GraphicsDevice.Viewport.Height),
                 Color.White);

                

                ///spriteBatch.Draw(images, new Rectangle(0, 0, 100, 80), Color.White);
                spriteBatch.End();


                if (this.bodies != null)
                {
                    if (bodies[0].IsTracked)
                    {
                        
                        spriteBatch.Begin();
                        /*
                        int width = 100;
                        int height = 80;
                        Texture2D rectangle = new Texture2D(GraphicsDevice, width, height, false, SurfaceFormat.Color);
                        Color[] colorData = new Color[width * height];
                        for (int i = 0; i < width * height; i++)
                        {
                            colorData[i] = Color.White;
                            rectangle.SetData<Color>(colorData);
                        }
                        */

                        

                        double handSeparation =  Math.Sqrt(
                            bodies[0].Joints[JointType.WristRight].Position.X - bodies[0].Joints[JointType.WristLeft].Position.X +
                            bodies[0].Joints[JointType.WristRight].Position.Y - bodies[0].Joints[JointType.WristLeft].Position.Y +
                            bodies[0].Joints[JointType.WristRight].Position.Z - bodies[0].Joints[JointType.WristLeft].Position.Z);



                        var HandPosition = bodies[0].Joints[JointType.WristRight].Position;
                        CameraSpacePoint cameraPoint = HandPosition;


                        ColorSpacePoint colorPoint = coordinateMapper.MapCameraPointToColorSpace(cameraPoint);

                        spriteBatch.DrawString(font, " x:" + colorPoint.X
                                                   + " y:" + colorPoint.Y,
                                                   new Vector2(10, GraphicsDevice.Viewport.Height - 70),
                                                   Color.Red);

                        if (bodies[0].HandRightState == HandState.Closed)
                        {
                            spriteBatch.Draw(mario, new Rectangle((int)colorPoint.X - 5, (int)colorPoint.Y - 5, 50, 100), Color.White);
                        }
                        else
                        {
                            spriteBatch.Draw(circle, new Rectangle((int)colorPoint.X-5, (int)colorPoint.Y-5, 10, 10), Color.White);
                        }

                        


                        var HandPosition2 = bodies[0].Joints[JointType.ElbowRight].Position;
                        CameraSpacePoint cameraPoint2 = HandPosition2;


                        ColorSpacePoint colorPoint2 = coordinateMapper.MapCameraPointToColorSpace(cameraPoint2);
                        spriteBatch.Draw(circle2, new Rectangle((int)colorPoint2.X - 5, (int)colorPoint2.Y - 5, 10, 10), Color.White);



                        spriteBatch.DrawString(font, " x:" + bodies[0].Joints[JointType.WristRight].Position.X 
                                                   + " y:" + bodies[0].Joints[JointType.WristRight].Position.Y
                                                   + " z:" + bodies[0].Joints[JointType.WristRight].Position.Z
                                                   + " sep:" + handSeparation,
                                                   new Vector2(10, GraphicsDevice.Viewport.Height - 20), 
                                                   Color.LimeGreen);

                        if (bodies[0].Joints[JointType.WristRight].Position.X > 0.2)
                        {
                            spriteBatch.DrawString(font, "right",
                                                   new Vector2(GraphicsDevice.Viewport.Width - 50, GraphicsDevice.Viewport.Height/2),
                                                   Color.Black);
                        }
                        if (bodies[0].Joints[JointType.WristRight].Position.X < -0.2)
                        {
                            spriteBatch.DrawString(font, "left",
                                                   new Vector2(10, GraphicsDevice.Viewport.Height / 2),
                                                   Color.Black);
                        }

                        if (bodies[0].Joints[JointType.WristRight].Position.X > -0.1 && 
                            bodies[0].Joints[JointType.WristRight].Position.X < 0.1 && 
                            bodies[0].Joints[JointType.WristRight].Position.Y > 0)
                        {
                            spriteBatch.DrawString(font, "top",
                                                   new Vector2(GraphicsDevice.Viewport.Width/2, 10),
                                                   Color.Black);
                        }

                        if (bodies[0].Joints[JointType.WristRight].Position.X > -0.1 &&
                            bodies[0].Joints[JointType.WristRight].Position.X < 0.1 &&
                            bodies[0].Joints[JointType.WristRight].Position.Y < -0.25)
                        {
                            spriteBatch.DrawString(font, "bottom",
                                                   new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height - 20),
                                                   Color.Black);
                        }

                        if (mygest == true)
                        {
                            spriteBatch.DrawString(font, "you waved",
                                                   new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height - 100),
                                                   Color.Turquoise);
                        }



                        spriteBatch.End();

                       

                    }
                }
            }

            base.Draw(gameTime);
        }
    }
}

