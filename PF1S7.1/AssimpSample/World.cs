// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using SharpGL;
using SharpGL.SceneGraph.Core;
using SharpGL.SceneGraph.Quadrics;
using System;

namespace AssimpSample
{
    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        private const string FaceName = "Tahoma";

        Cylinder cylinder;

        /// <summary>
        ///	 Ugao rotacije Meseca
        /// </summary>
        private float m_moonRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije Zemlje
        /// </summary>
        private float m_earthRotation = 0.0f;

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

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.FrontFace(OpenGL.GL_CW);

            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Color(1.0f, 1.1f, 0.0f);

            m_scene.LoadScene();
            m_scene.Initialize();

            cylinder = new Cylinder();
        }

        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(45f, (double)width / height, 0.5f, 100000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.Viewport(0, 0, m_width, m_height);
            gl.LoadIdentity();
            gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            //stub kosa
            gl.PushMatrix();
            gl.Color(0.658824, 0.658824, 0.658824);
            cylinder.CreateInContext(gl);
            cylinder.Height = 4500f;
            cylinder.TopRadius = 75f;
            cylinder.BaseRadius = 75f;
            gl.Translate(0.0f, -2000.0f, -m_sceneDistance);
            gl.Rotate(-90f, 1.0f, 0.0f, 0.0f);
            cylinder.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //nosac kosa levo
            gl.PushMatrix();
            gl.Color(0.658824, 0.658824, 0.658824);
            cylinder.CreateInContext(gl);
            cylinder.Height = 2000f;
            cylinder.TopRadius = 60f;
            cylinder.BaseRadius = 60f;
            gl.Translate(0.0f, 2500.0f, -m_sceneDistance);
            gl.Rotate(-90f, 0.0f, 1.0f, 0.0f);
            cylinder.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //nosac kosa desno
            gl.PushMatrix();
            gl.Color(0.658824, 0.658824, 0.658824);
            cylinder.CreateInContext(gl);
            cylinder.Height = 2000f;
            cylinder.TopRadius = 60f;
            cylinder.BaseRadius = 60f;
            gl.Translate(0.0f, 2500.0f, -m_sceneDistance);
            gl.Rotate(90f, 0.0f, 1.0f, 0.0f);
            cylinder.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //levi stap kosa
            gl.PushMatrix();
            gl.Color(0.658824, 0.658824, 0.658824);
            cylinder.CreateInContext(gl);
            cylinder.Height = 6500f;
            cylinder.TopRadius = 60f;
            cylinder.BaseRadius = 60f;
            gl.Translate(-1940.0f, 2500.0f, -m_sceneDistance);
            gl.Rotate(-90f, 1.0f, 0.0f, 0.0f);
            cylinder.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //desni stap kosa
            gl.PushMatrix();
            gl.Color(0.658824, 0.658824, 0.658824);
            cylinder.CreateInContext(gl);
            cylinder.Height = 6500f;
            cylinder.TopRadius = 60f;
            cylinder.BaseRadius = 60f;
            gl.Translate(1950.0f, 2500.0f, -m_sceneDistance);
            gl.Rotate(-90f, 1.0f, 0.0f, 0.0f);
            cylinder.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            // podloga
            gl.PushMatrix();
            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(0.0f, 0.5f, 0.0f);
            gl.Vertex(7500.0f, -2000.0f, -m_sceneDistance);//gornja
            gl.Vertex(-7500.0f, -2000.0f, -m_sceneDistance);//gornja
            gl.Vertex(-7500.0f, -5000.0f, m_sceneDistance);//donja
            gl.Vertex(7500.0f, -5000.0f, m_sceneDistance);//donja
            gl.End();
            gl.PopMatrix();

            // lopta
            gl.PushMatrix();
            gl.Translate(-2500.0f, -2500.0f, 4500.0f);
            gl.Rotate(-90f, 0.0f, 1.0f, 0.3f);
            m_scene.Draw();
            gl.PopMatrix();

            gl.PushMatrix();
            {
                gl.Viewport(m_width * 23 / 32, m_height * 13 / 16, m_width * 8 / 32, m_height * 3 / 16);
                gl.MatrixMode(OpenGL.GL_PROJECTION);
                gl.LoadIdentity();

                gl.MatrixMode(OpenGL.GL_MODELVIEW);
                gl.LoadIdentity();
                gl.Color(1.0f, 1.1f, 0.0f);
                gl.DrawText3D(FaceName, 10f, 1f, 0f, "");
                gl.DrawText(10, 270, 1.0f, 1.1f, 0.0f, "Tahoma", 10, "Predmet: Racunarska grafika");
                gl.DrawText(10, 270, 1.0f, 1.1f, 0.0f, "Tahoma", 10, "_______________________");
                gl.DrawText(10, 205, 1.0f, 1.1f, 0.0f, "Tahoma", 10, "Sk.god: 2020/21          ");
                gl.DrawText(10, 205, 1.0f, 1.1f, 0.0f, "Tahoma", 10, "_______________________");
                gl.DrawText(10, 140, 1.0f, 1.1f, 0.0f, "Tahoma", 10, "Ime: Ana                   ");
                gl.DrawText(10, 140, 1.0f, 1.1f, 0.0f, "Tahoma", 10, "_______________________");
                gl.DrawText(10, 75, 1.0f, 1.1f, 0.0f, "Tahoma", 10, "Prezime: Atanackovic       ");
                gl.DrawText(10, 75, 1.0f, 1.1f, 0.0f, "Tahoma", 10, "_______________________");
                gl.DrawText(10, 10, 1.0f, 1.1f, 0.0f, "Tahoma", 10, "Sifra zad: 7.1             ");
                gl.DrawText(10, 10, 1.0f, 1.1f, 0.0f, "Tahoma", 10, "_______________________");
            }
            gl.PopMatrix();
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(45f, (double)m_width / m_height, 0.5f, 100000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.FrontFace(OpenGL.GL_CCW);

            gl.Flush();
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
