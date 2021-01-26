using SharpGL;
using SharpGL.SceneGraph.Core;
using SharpGL.SceneGraph.Quadrics;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Threading;

namespace AssimpSample
{
    public class World : IDisposable
    {
        #region Atributi

        private const string FaceName = "Tahoma";

        Cylinder cylinder;

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 14000.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        private enum TextureObjects { Grass = 0, WhitePlastic };
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;

        /// <summary>
        ///	 Identifikatori OpenGL tekstura
        /// </summary>
        private uint[] m_textures = null;

        /// <summary>
        ///	 Putanje do slika koje se koriste za teksture
        /// </summary>
        private string[] m_textureFiles = { "..//..//images//grass.jpg", "..//..//images//whiteplastic.jpg" };

        private float ballHeight;
        private bool ballGoingUp;
        private DispatcherTimer timer1;
        private DispatcherTimer timer2;
        public bool jumpStop;
        private float[] pos;
        public double yGoal;
        public double rotateBall;
        public double sizeOfBall;

        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        public DispatcherTimer Timer1 { get => timer1; set => timer1 = value; }
        public DispatcherTimer Timer2 { get => timer2; set => timer2 = value; }
        public bool JumpStop { get => jumpStop; set => jumpStop = value; }
        public float[] Pos { get => pos; set => pos = value; }
        public double YGoal { get => yGoal; set => yGoal = value; }
        public double SizeOfBall { get => sizeOfBall; set => sizeOfBall = value; }
        public double RotateBall { get => rotateBall; set => rotateBall = value; }
        public float BallHeight { get => ballHeight; set => ballHeight = value; }
        public bool BallGoingUp { get => ballGoingUp; set => ballGoingUp = value; }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Color(1.0f, 1.1f, 0.0f);

            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_AMBIENT_AND_DIFFUSE);
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_NORMALIZE);

            InitializeTexture(gl);
            InitializeMovingBall();

            SizeOfBall = MainWindow.scaling;
            YGoal = MainWindow.heightOfGoal;
            m_scene.LoadScene();
            m_scene.Initialize();

            cylinder = new Cylinder();
        }

        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(45f, (double)width / height, 0.5f, 100000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();
        }

        public void Draw(OpenGL gl)
        {
            GenerateGeneral(gl);
            GenerateGoal(gl);
            GenerateBase(gl);
            GenerateBall(gl);
            GenerateText(gl);
            gl.Flush();
        }

        public void KickBall()
        {
            BallHeight = -100f;
            Pos[0] = 0f;
            Pos[1] = BallHeight;
            Pos[2] = -200;
            JumpStop = true;

            Timer1.Stop();
            Timer2 = new DispatcherTimer();
            Timer2.Interval = TimeSpan.FromMilliseconds(1);
            Timer2.Tick += new EventHandler(UpdateAnimation2);
            Timer2.Start();
        }

        private void InitializeTexture(OpenGL gl)
        {
            m_textures = new uint[2];
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);

            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);

                Bitmap image = new Bitmap(m_textureFiles[i]);
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                BitmapData imageData = image.LockBits(rect, ImageLockMode.ReadOnly,
                                                      PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);

                image.UnlockBits(imageData);
                image.Dispose();
            }
        }

        private void InitializeMovingBall()
        {
            Timer1 = new DispatcherTimer();
            Timer1.Interval = TimeSpan.FromMilliseconds(10);
            Timer1.Tick += new EventHandler(UpdateAnimation1);
            Timer1.Start();
            Timer2 = new DispatcherTimer();
            Pos = new float[3];
            BallHeight = -100f;
            Pos[0] = 0f;
            Pos[1] = BallHeight;
            Pos[2] = -200;

            BallGoingUp = true;
            JumpStop = false;
            RotateBall = 0;
        }

        private void UpdateAnimation1(object sender, EventArgs e)
        {
        }

        private void UpdateAnimation2(object sender, EventArgs e)
        {
            RotateBall = 0;
            if (Pos[2] <= 3000)
            {
                if (Pos[1] >= 1100)
                {
                    Pos[0] -= 150;
                    Pos[1] += 120;
                    Pos[2] += 120;
                }
                else
                {
                    Pos[0] -= 340;
                    Pos[1] += 25;
                    Pos[2] += 10;
                }
            }
            else
            {
                BallHeight = -100f;
                Pos[0] = 0f;
                Pos[1] = BallHeight;
                Pos[2] = -200;
            }
        }

        private void GenerateGeneral(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(45f, (double)m_width / m_height, 0.5f, 100000f);
            gl.LookAt(0f, 0f, m_sceneDistance + 1000, 0f, 0f, m_sceneDistance, 0f, 1f, 0f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.Enable(OpenGL.GL_AUTO_NORMAL);

            gl.PushMatrix();
            GenerateLight(gl);

            gl.PushMatrix();
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);
            gl.PushMatrix();

            gl.PushMatrix();
            gl.Viewport(0, 0, m_width, m_height);
        }

        private void GenerateLight(OpenGL gl)
        {
            GeneratePointLight(gl);
            GenerateReflector(gl);
        }

        private static void GeneratePointLight(OpenGL gl)
        {
            gl.PushMatrix();
            float[] light0pos = new float[] { 1300f, 700.0f, 0, 1.0f };
            float[] light0ambient = new float[] { 0.4f, 0.4f, 0.4f, 1.0f };
            float[] light0diffuse = new float[] { 0.7f, 0.7f, 0.7f, 1.0f };
            float[] light0specular = new float[] { 0.8f, 0.8f, 0.8f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.PopMatrix();
        }

        private void GenerateReflector(OpenGL gl)
        {
            gl.PushMatrix();
            float[] light1ambient = new float[] { 0.1f, 0f, 0f, 1.0f };
            float[] light1diffuse = { 1.0f, 0.0f, 0.0f, 1.0f };
            float[] light1specular = new float[] { 1.0f, 1.0f, 0f, 1.0f };
            float[] light1direction = new float[] { 0f, -1f, 0f, 0f };

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, light1specular);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, light1ambient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, light1diffuse);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, light1direction);

            float[] light0pos = new float[] { 0, 3300, -200, 1.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, light0pos);

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 30.0f);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Enable(OpenGL.GL_LIGHT1);
            gl.PopMatrix();
        }

        private void GenerateGoal(OpenGL gl)
        {
            GenerateMiddleSideOfGoal(gl);
            GenerateLeftPartOfBaseForGoal(gl);
            GenerateRightPartOfBaseForGoal(gl);
            GenerateLeftSideOfGoal(gl);
            GenerateRightSideOfGoal(gl);
        }

        private void GenerateMiddleSideOfGoal(OpenGL gl)
        {
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.WhitePlastic]);
            gl.PushMatrix();
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.Color(0.658824, 0.658824, 0.658824);
            cylinder.CreateInContext(gl);
            cylinder.Height = 4500;
            cylinder.TopRadius = 75f;
            cylinder.BaseRadius = 75f;
            cylinder.TextureCoords = true;
            cylinder.NormalGeneration = Normals.Smooth;
            cylinder.NormalOrientation = Orientation.Outside;
            gl.Translate(0.0f, -2000.0f + YGoal - 6500f, -m_sceneDistance);
            gl.Rotate(-90f, 1.0f, 0.0f, 0.0f);
            cylinder.Render(gl, RenderMode.Render);
            gl.PopMatrix();
        }

        private void GenerateLeftPartOfBaseForGoal(OpenGL gl)
        {
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.WhitePlastic]);
            gl.PushMatrix();
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            cylinder.CreateInContext(gl);
            cylinder.Height = 2000;
            cylinder.TopRadius = 60f;
            cylinder.BaseRadius = 60f;
            cylinder.TextureCoords = true;
            cylinder.NormalGeneration = Normals.Smooth;
            cylinder.NormalOrientation = Orientation.Outside;
            gl.Translate(0.0f, 2500.0f + YGoal - 6500f, -m_sceneDistance);
            gl.Rotate(-90f, 0.0f, 1.0f, 0.0f);
            cylinder.Render(gl, RenderMode.Render);
            gl.PopMatrix();
        }

        private void GenerateRightPartOfBaseForGoal(OpenGL gl)
        {
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.WhitePlastic]);
            gl.PushMatrix();
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            cylinder.CreateInContext(gl);
            cylinder.Height = 2000;
            cylinder.TopRadius = 60f;
            cylinder.BaseRadius = 60f;
            cylinder.TextureCoords = true;
            cylinder.NormalGeneration = Normals.Smooth;
            cylinder.NormalOrientation = Orientation.Outside;
            gl.Translate(0.0f, 2500.0f + YGoal - 6500f, -m_sceneDistance);
            gl.Rotate(90f, 0.0f, 1.0f, 0.0f);
            cylinder.Render(gl, RenderMode.Render);
            gl.PopMatrix();
        }

        private void GenerateLeftSideOfGoal(OpenGL gl)
        {
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.WhitePlastic]);
            gl.PushMatrix();
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            cylinder.CreateInContext(gl);
            cylinder.Height = 6500;
            cylinder.TopRadius = 60f;
            cylinder.BaseRadius = 60f;
            cylinder.TextureCoords = true;
            cylinder.NormalGeneration = Normals.Smooth;
            cylinder.NormalOrientation = Orientation.Outside;
            gl.Translate(-1940.0f, 2500.0f + YGoal - 6500f, -m_sceneDistance);
            gl.Rotate(-90f, 1.0f, 0.0f, 0.0f);
            cylinder.Render(gl, RenderMode.Render);
            gl.PopMatrix();
        }

        private void GenerateRightSideOfGoal(OpenGL gl)
        {
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.WhitePlastic]);
            gl.PushMatrix();
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            cylinder.CreateInContext(gl);
            cylinder.Height = 6500;
            cylinder.TopRadius = 60f;
            cylinder.BaseRadius = 60f;
            cylinder.TextureCoords = true;
            cylinder.NormalGeneration = Normals.Smooth;
            cylinder.NormalOrientation = Orientation.Outside;
            gl.Translate(1950.0f, 2500.0f + YGoal - 6500f, -m_sceneDistance);
            gl.Rotate(-90f, 1.0f, 0.0f, 0.0f);
            cylinder.Render(gl, RenderMode.Render);
            gl.PopMatrix();
        }

        private void GenerateBase(OpenGL gl)
        {
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Grass]);
            gl.PushMatrix();
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();
            gl.Scale(2, 2, 2);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(0.0f, 0.50f, 0.0f);

            gl.Normal(FindFaceNormal(7500.0f, -2000.0f, -m_sceneDistance, -7500.0f, -2000.0f, -m_sceneDistance,
                -7500.0f, -5000.0f, m_sceneDistance));
            // gl.Normal(FindFaceNormal(-7500.0f, -2000.0f, -m_sceneDistance,
            //  -7500.0f, -5000.0f, m_sceneDistance, 7500.0f, -5000.0f, m_sceneDistance));
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(7500.0f, -2000.0f, -m_sceneDistance); //gornja
            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(-7500.0f, -2000.0f, -m_sceneDistance); //gornja
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-7500.0f, -5000.0f, m_sceneDistance); //donja
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(7500.0f, -5000.0f, m_sceneDistance); //donja
            gl.End();
            gl.PopMatrix();
        }

        public float[] FindFaceNormal(float x1, float y1, float z1, float x2, float y2,
            float z2, float x3, float y3, float z3)
        {
            float[] normal = new float[3];
            normal[0] = (y1 - y2) * (z2 - z3) - (y2 - y3) * (z1 - z2);
            normal[1] = (x2 - x3) * (z1 - z2) - (x1 - x2) * (z2 - z3);
            normal[2] = (x1 - x2) * (y2 - y3) - (x2 - x3) * (y1 - y2);
            float len = (float)(Math.Sqrt((normal[0] * normal[0]) + (normal[1] * normal[1]) + (normal[2] * normal[2])));

            if (len == 0.0f)
            {
                len = 1.0f;
            }

            normal[0] /= len;
            normal[1] /= len;
            normal[2] /= len;

            return normal;
        }

        private void GenerateBall(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Translate(-2000.0f, -2500.0f, 2000.0f);
            gl.Rotate(-90f, 0.0f, 1.0f, 0.3f);

            GenerateMovementOfBall(gl);

            gl.Scale(SizeOfBall, SizeOfBall, SizeOfBall);
            gl.Color(1.0, 1.0, 1.0);
            m_scene.Draw();
            gl.PopMatrix();
        }

        private void GenerateMovementOfBall(OpenGL gl)
        {
            if (!JumpStop)
            {
                if (BallGoingUp)
                {
                    BallHeight += 20f;
                    Pos[0] = 0;
                    Pos[1] = BallHeight;
                    if (BallHeight >= 500f)
                        BallGoingUp = false;
                }
                else
                {
                    BallHeight -= 20f;
                    Pos[0] = 0;
                    Pos[1] = BallHeight;
                    if (BallHeight == -100f)
                        BallGoingUp = true;
                }
                gl.Translate(Pos[0], Pos[1], Pos[2]);
                GenerateRotationForBall(gl);
            }
            else
                gl.Translate(Pos[0], Pos[1], Pos[2]);
        }

        private void GenerateRotationForBall(OpenGL gl)
        {
            if (RotateBall > 30)
                RotateBall = 0.0f;
            else
                RotateBall += MainWindow.speedOfRotation;

            gl.Rotate(RotateBall, 1, 0, 0);
        }

        private void GenerateText(OpenGL gl)
        {
            gl.PushMatrix();
            {
                gl.Viewport(m_width * 23 / 32, m_height * 13 / 16, m_width * 8 / 32, m_height * 3 / 16);
                gl.MatrixMode(OpenGL.GL_PROJECTION);
                gl.LoadIdentity();

                gl.MatrixMode(OpenGL.GL_MODELVIEW);
                gl.LoadIdentity();
                gl.Color(1.0f, 1.1f, 0.0f);
                gl.DrawText3D(FaceName, 10f, 1f, 0f, "");
                gl.DrawText(350, 360, 1.0f, 1.1f, 0.0f, FaceName, 10, "Predmet: Racunarska grafika");
                gl.DrawText(350, 360, 1.0f, 1.1f, 0.0f, FaceName, 10, "_______________________");
                gl.DrawText(350, 295, 1.0f, 1.1f, 0.0f, FaceName, 10, "Sk.god: 2020/21          ");
                gl.DrawText(350, 295, 1.0f, 1.1f, 0.0f, FaceName, 10, "_______________________");
                gl.DrawText(350, 230, 1.0f, 1.1f, 0.0f, FaceName, 10, "Ime: Ana                   ");
                gl.DrawText(350, 230, 1.0f, 1.1f, 0.0f, FaceName, 10, "_______________________");
                gl.DrawText(350, 165, 1.0f, 1.1f, 0.0f, FaceName, 10, "Prezime: Atanackovic       ");
                gl.DrawText(350, 165, 1.0f, 1.1f, 0.0f, FaceName, 10, "_______________________");
                gl.DrawText(350, 100, 1.0f, 1.1f, 0.0f, FaceName, 10, "Sifra zad: 7.1             ");
                gl.DrawText(350, 100, 1.0f, 1.1f, 0.0f, FaceName, 10, "_______________________");
            }
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(45f, (double)m_width / m_height, 0.5f, 100000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.FrontFace(OpenGL.GL_CCW);
            gl.PopMatrix();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
