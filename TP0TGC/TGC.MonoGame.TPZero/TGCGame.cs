using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Content.Models;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Clase principal del juego.
    /// </summary>
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";

        private GraphicsDeviceManager Graphics { get; }
        private CityScene City { get; set; }
        private Model CarModel { get; set; }
        private Matrix CarWorld { get; set; }
        private Matrix View { get; set; }
        private FollowCamera FollowCamera { get; set; }
        private Matrix Projection { get; set; }
        
        //Referencias del auto
        private Matrix carRotation { get; set; }
        private Vector3 carPosition { get; set; }
        private Vector3 carFront {  get; set; }

        //Velocidades del auto
        private float velocidad = 0f;
        private float velocidadVertical = 0f;
        private const float velocidadMaxima = 500f;
        private const float velocidadMinima = -200f;
        private const float aceleracion = 100f;
        private const float velocidadRotacion = 0.02f;
        private const float velocidadInicialSalto = 25f;
        private const float gravedad = 98f;

        private bool enTierra = true;
        
        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Se encarga de la configuracion y administracion del Graphics Device.
            Graphics = new GraphicsDeviceManager(this);

            // Carpeta donde estan los recursos que vamos a usar.
            Content.RootDirectory = "Content";

            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }

        /// <summary>
        ///     Llamada una vez en la inicializacion de la aplicacion.
        ///     Escribir aca todo el codigo de inicializacion: Todo lo que debe estar precalculado para la aplicacion.
        /// </summary>
        protected override void Initialize()
        {
            // Enciendo Back-Face culling.
            // Configuro Blend State a Opaco.
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            GraphicsDevice.RasterizerState = rasterizerState;
            GraphicsDevice.BlendState = BlendState.Opaque;

            // Configuro las dimensiones de la pantalla.
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 200;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 200;
            Graphics.ApplyChanges();


            // Creo una camara para seguir a nuestro auto.
            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);

            // Configuro la matriz de mundo del auto.
            CarWorld = Matrix.Identity;

         // Inicializo la posicion y la matrix
            carPosition = Vector3.Zero;
            carFront = Vector3.Forward;
            carRotation = Matrix.Identity;
            
            

            base.Initialize();
        }

        /// <summary>
        ///     Llamada una sola vez durante la inicializacion de la aplicacion, luego de Initialize, y una vez que fue configurado GraphicsDevice.
        ///     Debe ser usada para cargar los recursos y otros elementos del contenido.
        /// </summary>
        protected override void LoadContent()
        {
            // Creo la escena de la ciudad.
            City = new CityScene(Content);
              

            // La carga de contenido debe ser realizada aca.
            CarModel = Content.Load<Model>(ContentFolder3D + "scene/car");
            
            base.LoadContent();
        }

        /// <summary>
        ///     Es llamada N veces por segundo. Generalmente 60 veces pero puede ser configurado.
        ///     La logica general debe ser escrita aca, junto al procesamiento de mouse/teclas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            //Caputo el tiempo del ultimo update.
            var tiempo = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            
            // Caputo el estado del teclado.
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                // Salgo del juego.
                Exit();
            }

            // La logica debe ir aca.
            if (keyboardState.IsKeyDown(Keys.None))
            {
                velocidad = 0f;
            }
            if(keyboardState.IsKeyDown(Keys.A))
            {
                carRotation *= Matrix.CreateRotationY(velocidadRotacion);
                carFront = Vector3.Transform(Vector3.Forward,carRotation);
            }
            if( keyboardState.IsKeyDown(Keys.D))
            {
                carRotation *= Matrix.CreateRotationY(-velocidadRotacion);
                carFront = Vector3.Transform(Vector3.Forward,carRotation);
            }
            if (keyboardState.IsKeyDown(Keys.W))
            {
                velocidad += aceleracion * tiempo;
                if (velocidad >= velocidadMaxima) velocidad = velocidadMaxima; 
                carPosition += carFront * velocidad * tiempo;
            }

            if (keyboardState.IsKeyDown((Keys)Keys.S))
            {
                velocidad -= aceleracion * tiempo;
                if (velocidad <= velocidadMinima) velocidad = velocidadMinima;
                carPosition += carFront * velocidad * tiempo;
            } 
            
            if(carPosition.Y<=0f  & keyboardState.IsKeyDown(Keys.Space))
            {
                velocidadVertical = velocidadInicialSalto;
                carPosition += Vector3.Up * velocidadVertical;
            } else if (carPosition.Y > 0f)
            {
                velocidadVertical -= gravedad * tiempo;
                carPosition += Vector3.Up * velocidadVertical;
            } 

            if(carPosition.Y < 0)
            {
                Vector3 margen = Vector3.Zero;
                margen.Y = 0 - carPosition.Y;
                carPosition += carFront * velocidad * tiempo + margen;

            } else {
                carPosition += carFront * velocidad * tiempo;
            }

            CarWorld = carRotation * Matrix.CreateTranslation(carPosition);



            // Actualizo la camara, enviandole la matriz de mundo del auto.
            FollowCamera.Update(gameTime, CarWorld);

            base.Update(gameTime);
        }


        /// <summary>
        ///     Llamada para cada frame.
        ///     La logica de dibujo debe ir aca.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Limpio la pantalla.
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Dibujo la ciudad.
            City.Draw(gameTime, FollowCamera.View, FollowCamera.Projection);


            // El dibujo del auto debe ir aca.
            CarModel.Draw(CarWorld, FollowCamera.View, FollowCamera.Projection);
            
            base.Draw(gameTime);
        }

        /// <summary>
        ///     Libero los recursos cargados.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos cargados dessde Content Manager.

            Content.Unload();

            base.UnloadContent();
        }

        public Matrix CalcularMatrizOrientacion(float scale, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            var matWorld = Matrix.CreateScale(scale * 0.1f);

            // determino la orientacion
            var Dir = p1 - p0;
            Dir.Normalize();
            var Tan = p2 - p0;
            Tan.Normalize();
            var VUP = Vector3.Cross(Tan, Dir);
            VUP.Normalize();
            Tan = Vector3.Cross(VUP, Dir);
            Tan.Normalize();

            var V = VUP;
            var U = Tan;

            var Orientacion = new Matrix();
            Orientacion.M11 = U.X;
            Orientacion.M12 = U.Y;
            Orientacion.M13 = U.Z;
            Orientacion.M14 = 0;

            Orientacion.M21 = V.X;
            Orientacion.M22 = V.Y;
            Orientacion.M23 = V.Z;
            Orientacion.M24 = 0;

            Orientacion.M31 = Dir.X;
            Orientacion.M32 = Dir.Y;
            Orientacion.M33 = Dir.Z;
            Orientacion.M34 = 0;

            Orientacion.M41 = 0;
            Orientacion.M42 = 0;
            Orientacion.M43 = 0;
            Orientacion.M44 = 1;
            matWorld = matWorld * Orientacion;

            // traslado
            matWorld = matWorld * Matrix.CreateTranslation(p0);
            return matWorld;
        }
    }
    

}