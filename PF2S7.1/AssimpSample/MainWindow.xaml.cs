using Microsoft.Win32;
using SharpGL.SceneGraph;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AssimpSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #region Atributi

        /// <summary>
        ///	 Instanca OpenGL "sveta" - klase koja je zaduzena za iscrtavanje koriscenjem OpenGL-a.
        /// </summary>
        World m_world = null;

        public static double heightOfGoal = 6500.0;
        public ObservableCollection<double> HeightOfGoal
        {
            get;
            set;
        }

        public static double scaling = 0.7;
        public ObservableCollection<double> Scaling
        {
            get;
            set;
        }

        public static double speedOfRotation = 10.0;
        public ObservableCollection<double> SpeedOfRotation
        {
            get;
            set;
        }

        #endregion Atributi

        #region Konstruktori

        public MainWindow()
        {
            HeightOfGoal = new ObservableCollection<double>();
            HeightOfGoal.Add(4000.0);
            HeightOfGoal.Add(4500.0);
            HeightOfGoal.Add(5000.0);
            HeightOfGoal.Add(5500.0);
            HeightOfGoal.Add(6000.0);
            HeightOfGoal.Add(6500.0);

            Scaling = new ObservableCollection<double>();
            Scaling.Add(0.5);
            Scaling.Add(0.6);
            Scaling.Add(0.7);
            Scaling.Add(0.8);
            Scaling.Add(0.9);
            Scaling.Add(1.0);

            SpeedOfRotation = new ObservableCollection<double>();
            SpeedOfRotation.Add(2.0);
            SpeedOfRotation.Add(3.0);
            SpeedOfRotation.Add(5.0);
            SpeedOfRotation.Add(10.0);
            SpeedOfRotation.Add(20.0);
            SpeedOfRotation.Add(30.0);

            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.DataContext = this;
            try
            {
                m_world = new World(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models\\Football"), "football.obj", (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
            }
            catch (Exception e)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                this.Close();
            }
        }

        #endregion Konstruktori

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F2: this.Close(); break;
                case Key.D:
                    if (!m_world.JumpStop && m_world.RotationX < 20)
                        m_world.RotationX += 5.0f;
                    break;
                case Key.E:
                    if (!m_world.JumpStop && m_world.RotationX > -10)
                        m_world.RotationX -= 5.0f;
                    break;
                case Key.F:
                    if (!m_world.JumpStop)
                        m_world.RotationY -= 5.0f;
                    break;
                case Key.S:
                    if (!m_world.JumpStop)
                        m_world.RotationY += 5.0f;
                    break;
                case Key.Add:
                    if (!m_world.JumpStop)
                        m_world.SceneDistance -= 50;
                    break;
                case Key.Subtract:
                    if (!m_world.JumpStop)
                        m_world.SceneDistance += 50;
                    break;
                case Key.V:
                    if (!m_world.JumpStop)
                        m_world.KickBall();
                    break;
                case Key.F3:
                    OpenFileDialog opfModel = new OpenFileDialog();
                    bool result = (bool)opfModel.ShowDialog();
                    if (result)
                    {

                        try
                        {
                            World newWorld = new World(Directory.GetParent(opfModel.FileName).ToString(), Path.GetFileName(opfModel.FileName), (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
                            m_world.Dispose();
                            m_world = newWorld;
                            m_world.Initialize(openGLControl.OpenGL);
                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta:\n" + exp.Message, "GRESKA", MessageBoxButton.OK);
                        }
                    }
                    break;
            }
        }

        private void HeightOfGoal_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (heightOfGoalCombo.SelectedIndex.Equals(-1))
                m_world.yGoal = 6500.0;
            else if (heightOfGoalCombo.SelectedItem.Equals(4000.0) && !m_world.JumpStop)
            {
                int idx = HeightOfGoal.IndexOf(4000.0);
                m_world.yGoal = HeightOfGoal[idx];
            }
            else if (heightOfGoalCombo.SelectedItem.Equals(4500.0) && !m_world.JumpStop)
            {
                int idx = HeightOfGoal.IndexOf(4500.0);
                m_world.yGoal = HeightOfGoal[idx];
            }
            else if (heightOfGoalCombo.SelectedItem.Equals(5000.0) && !m_world.JumpStop)
            {
                int idx = HeightOfGoal.IndexOf(5000.0);
                m_world.yGoal = HeightOfGoal[idx];
            }
            else if (heightOfGoalCombo.SelectedItem.Equals(5500.0) && !m_world.JumpStop)
            {
                int idx = HeightOfGoal.IndexOf(5500.0);
                m_world.yGoal = HeightOfGoal[idx];
            }
            else if (heightOfGoalCombo.SelectedItem.Equals(6000.0) && !m_world.JumpStop)
            {
                int idx = HeightOfGoal.IndexOf(6000.0);
                m_world.yGoal = HeightOfGoal[idx];
            }
            else if (heightOfGoalCombo.SelectedItem.Equals(6500.0) && !m_world.JumpStop)
            {
                int idx = HeightOfGoal.IndexOf(6500.0);
                m_world.yGoal = HeightOfGoal[idx];
            }
        }

        private void Scaling_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (scalingCombo.SelectedIndex.Equals(-1))
                m_world.sizeOfBall = 0.7;
            else if (scalingCombo.SelectedItem.Equals(0.5) && !m_world.JumpStop)
            {
                int idx = Scaling.IndexOf(0.5);
                m_world.sizeOfBall = Scaling[idx];
            }
            else if (scalingCombo.SelectedItem.Equals(0.6) && !m_world.JumpStop)
            {
                int idx = Scaling.IndexOf(0.6);
                m_world.sizeOfBall = Scaling[idx];
            }
            else if (scalingCombo.SelectedItem.Equals(0.7) && !m_world.JumpStop)
            {
                int idx = Scaling.IndexOf(0.7);
                m_world.sizeOfBall = Scaling[idx];
            }
            else if (scalingCombo.SelectedItem.Equals(0.8) && !m_world.JumpStop)
            {
                int idx = Scaling.IndexOf(0.8);
                m_world.sizeOfBall = Scaling[idx];
            }
            else if (scalingCombo.SelectedItem.Equals(0.9) && !m_world.JumpStop)
            {
                int idx = Scaling.IndexOf(0.9);
                m_world.sizeOfBall = Scaling[idx];
            }
            else if (scalingCombo.SelectedItem.Equals(1.0) && !m_world.JumpStop)
            {
                int idx = Scaling.IndexOf(1.0);
                m_world.sizeOfBall = Scaling[idx];
            }
        }

        private void SpeedOfRotation_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (speedOfRotationCombo.SelectedIndex.Equals(-1))
                m_world.rotateBall = 10.0;
            else if (speedOfRotationCombo.SelectedItem.Equals(2.0) && !m_world.JumpStop)
            {
                int idx = SpeedOfRotation.IndexOf(2.0);
                speedOfRotation = SpeedOfRotation[idx];
                m_world.rotateBall = speedOfRotation;
            }
            else if (speedOfRotationCombo.SelectedItem.Equals(3.0) && !m_world.JumpStop)
            {
                int idx = SpeedOfRotation.IndexOf(3.0);
                speedOfRotation = SpeedOfRotation[idx];
                m_world.rotateBall = speedOfRotation;
            }
            else if (speedOfRotationCombo.SelectedItem.Equals(5.0) && !m_world.JumpStop)
            {
                int idx = SpeedOfRotation.IndexOf(5.0);
                speedOfRotation = SpeedOfRotation[idx];
                m_world.rotateBall = speedOfRotation;
            }
            else if (speedOfRotationCombo.SelectedItem.Equals(10.0) && !m_world.JumpStop)
            {
                int idx = SpeedOfRotation.IndexOf(10.0);
                speedOfRotation = SpeedOfRotation[idx];
                m_world.rotateBall = speedOfRotation;
            }
            else if (speedOfRotationCombo.SelectedItem.Equals(20.0) && !m_world.JumpStop)
            {
                int idx = SpeedOfRotation.IndexOf(20.0);
                speedOfRotation = SpeedOfRotation[idx];
                m_world.rotateBall = speedOfRotation;
            }
            else if (speedOfRotationCombo.SelectedItem.Equals(30.0) && !m_world.JumpStop)
            {
                int idx = SpeedOfRotation.IndexOf(30.0);
                speedOfRotation = SpeedOfRotation[idx];
                m_world.rotateBall = speedOfRotation;
            }
        }

        private void KickBall_Click(object sender, RoutedEventArgs e)
        {
            m_world.KickBall();
        }

        private void SetBallToStart_Click(object sender, RoutedEventArgs e)
        {
            m_world.Timer2.Stop();
            m_world.BallHeight = -100f;
            m_world.Pos[0] = 0f;
            m_world.Pos[1] = m_world.BallHeight;
            m_world.Pos[2] = -200;

            m_world.BallGoingUp = true;
            m_world.JumpStop = false;
            m_world.RotateBall = 0;
        }
    }
}
