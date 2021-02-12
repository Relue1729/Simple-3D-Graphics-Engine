using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace GraphicsEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly Engine            engine       = new Engine();
        readonly List<Mesh>        renderMeshes = new List<Mesh>();
        readonly Camera            camera       = new Camera();
        readonly InputControl      controller   = new InputControl();
        readonly UILanguageManager langManager  = new UILanguageManager();
        
        int      focusMeshId = 0;
        double   currentFps  = 0;
        DateTime FPSCalculationDate;
        string   meshFileDirectory = "Meshes";

        public MainWindow()
        {
            InitializeComponent();
            DataContext = langManager;
            Dispatcher.UnhandledException += ExceptionHandler;
            AddMeshClick(this, null);
            OnRender();
        }

        void OnRender()
        {
            engine.Render(camera, renderMeshes);
            frontBuffer.Source = engine.GetFrontBuffer();
            ShowDebug();
        }

        void ShowDebug()
        {
            if (renderMeshes.Count() > focusMeshId)
            {
                var now = DateTime.Now;
                currentFps += ((1000.0 / (now - FPSCalculationDate).TotalMilliseconds) - currentFps) * 0.1;
                FPSCalculationDate = now;

                var currentMesh = renderMeshes[focusMeshId];

                Debug.Text = $"{currentMesh.Name}  ID: {focusMeshId}  FPS: {currentFps:0} \n" +
                             $"Position X: {currentMesh.Position.X:0.00} Y: {currentMesh.Position.Y:0.00} Z: {currentMesh.Position.Z:0.00} \n" +
                             $"Rotation X: {currentMesh.Rotation.X:0.00} Y: {currentMesh.Rotation.Y:0.00} Z: {currentMesh.Rotation.Z:0.00}";
            }
            else { Debug.Text = "No mesh to debug"; }
        }

        #region EventHandlers
        void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            controller.MouseControl(renderMeshes[focusMeshId], e);
            OnRender();
        }

        void SpinMeshRenderingHandler(object sender, object e)
        {
            controller.Spin(renderMeshes[focusMeshId], XSlider.Value, YSlider.Value, ZSlider.Value);
            OnRender();
        }

        void ExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Error has occured: " + e.Exception.Message);
        }
        #endregion

        #region Buttons/CheckBoxes
        void AddMeshClick(object sender, RoutedEventArgs e) 
        {
            if (renderMeshes.Count() < 3)
            {
                var defaultMeshPath = Directory.GetFiles(meshFileDirectory)[0];
                renderMeshes.Add(Mesh.LoadFromJson(defaultMeshPath));
                SelectMeshClick(sender, e);
            }
            else { MessageBox.Show($"{langManager.ErrorMessages["TooManyMeshes"]}"); }
        }

        void ChangeMeshClick(object sender, RoutedEventArgs e)
        {
            if (renderMeshes.Count() > 0)
            {
                var allMeshesInDirectory = Directory.GetFiles(meshFileDirectory);
                var currentMeshFileName = $"{renderMeshes[focusMeshId].Name}.json";
                
                var i = Array.IndexOf(allMeshesInDirectory, $"{meshFileDirectory}\\{currentMeshFileName}");
                if (i == -1) { throw new ArgumentException("Mesh file not found in mesh directory"); }
                if (i ==  0) { i = allMeshesInDirectory.Count(); }

                var Position = renderMeshes[focusMeshId].Position;
                var Color    = renderMeshes[focusMeshId].Color;

                renderMeshes[focusMeshId] = Mesh.LoadFromJson(allMeshesInDirectory[--i]);
                renderMeshes[focusMeshId].Position = Position;
                renderMeshes[focusMeshId].Color = Color;
            }
        }

        void RemoveMeshClick(object sender, RoutedEventArgs e) 
        {
            if (renderMeshes.Count() > 0)
            {
                renderMeshes.RemoveAt(focusMeshId);
                SelectMeshClick(sender, e);
            }
            else { MessageBox.Show($"{langManager.ErrorMessages["NoMeshesToRemove"]}"); }
        }

        void SelectMeshClick(object sender, RoutedEventArgs e) 
        {
            if (renderMeshes.Count() - 1 > focusMeshId) { focusMeshId++; }
            else { focusMeshId = 0; }
        }

        void ChangeColorClick(object sender, RoutedEventArgs e)
        {
            if (renderMeshes.Count() > 0)
            {
                int i = Array.IndexOf(ColorPalette.ModelColors, renderMeshes[focusMeshId].Color);

                if (i >= ColorPalette.ModelColors.Count() - 1) { i = -1; }

                renderMeshes[focusMeshId].Color = ColorPalette.ModelColors[++i];
            }
        }

        void DisplayModelClick(object sender, RoutedEventArgs e) => engine.DrawModel     ^= true;

        void DisplayMeshClick (object sender, RoutedEventArgs e) => engine.DrawMeshLines ^= true;

        void SolidColorClick  (object sender, RoutedEventArgs e) => engine.isSolidColor  ^= true;
        
        void SpinMeshClick(object sender, RoutedEventArgs e)
        {
          /*Резкое падение FPS при обновлении переднего буфера через CompositionTarget.Rendering
            и единовременного контроля через MouseMove, так что во время контроля мышью
            обновление буфера только через MouseMove.*/
            if ((bool) SpinMeshCheckbox.IsChecked)
            {
                CompositionTarget.Rendering += SpinMeshRenderingHandler;
                MouseMove -= MouseMoveHandler;
            }
            else
            {
                CompositionTarget.Rendering -= SpinMeshRenderingHandler;
                MouseMove += MouseMoveHandler;
            }
        }

        void RuEnClick(object sender, RoutedEventArgs e)
        {
            if      (langManager.CurrentLanguage == language.En) { langManager.CurrentLanguage = language.Ru; }
            else                                                 { langManager.CurrentLanguage = language.En; }
        }
        #endregion  
    }
}
