using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Heightmap.Helpers;
using Heightmap.Classes;

namespace Heightmap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // rychlost přibližování/oddalování
        private readonly float scaleSpeedWheel = 1.2f;
        // barva výběrové kružnice a bodů     
        private readonly Brush brush = Brushes.Red; 

        // načtená data
        public HeightMapData Data { get; set; } = new HeightMapData();

        // zobrazení 10 největších hodnot
        public List<Coordinate> MaxValues { get; set; }

        // zobrazení 10 nejmenších hodnot
        public List<Coordinate> MinValues { get; set; }

        // bod pro výpočet posunu   
        public Point FirstPoint { get; set; } = new Point();

        // střed výběru relativní k canvasu
        public Point SelectCenter { get; set; } = new Point();

        // ukládá transformaci pro obnovení 
        public MatrixTransform DefaultTransform { get; set; }

        // true pokud byl zadán střed kružnice
        public bool FirstPointSet { get; set; } = false;

        // střed výběru relativní k mapě
        private Point _selectCenterImg = new Point();
        public Point SelectCenterImg
        {
            get { return _selectCenterImg; }
            set { _selectCenterImg = ImgPointToDataPoint(value); }
        }

        public MainWindow()
        {
            InitializeComponent();
            StoreScale();
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
        }
       
        //  Zoom
        private void MouseScrollImg(object sender, MouseWheelEventArgs e)
        {
            RemoveSelect();
            var zoomIn = e.Delta > 0;
            ViewUtils.ZoomImage(zoomIn, e.GetPosition(ImageWindow), ImageWindow, scaleSpeedWheel);
        }

        private void LeftClickImg(object sender, MouseButtonEventArgs e)
        {
            if (!FirstPointSet)
            {
                SelectCenter = e.GetPosition(canvas);
                SelectCenterImg = e.GetPosition(ImageWindow);                
                CenterX.Content = "X: " + Math.Round(SelectCenterImg.X, 2);
                CenterY.Content = "Y: " + Math.Round(SelectCenterImg.Y, 2);

                RemoveSelect();
                CenterCircleSet(SelectCenter);                
            }
            else 
            {
                var pointInImage = ImgPointToDataPoint(e.GetPosition(ImageWindow));
                SecondSelectSet(e.GetPosition(canvas),pointInImage);                
            }

            FirstPointSet = !FirstPointSet;
        }

        private void RightClickImg(object sender, MouseButtonEventArgs e)
        {
            // odkud posouváme            
            FirstPoint = e.GetPosition(this);
            ImageWindow.CaptureMouse();            
        }
        
        // Posouvání img a výpis hodnoty kurzoru
        private void MouseMoveImg(object sender, MouseEventArgs e)
        {
            // update pozice            
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Point mousePoint = e.GetPosition(this);
                Point res = new Point(FirstPoint.X - mousePoint.X, FirstPoint.Y - mousePoint.Y);
                Canvas.SetLeft(ImageWindow, Canvas.GetLeft(ImageWindow) - res.X);
                Canvas.SetTop(ImageWindow, Canvas.GetTop(ImageWindow) - res.Y);

                Canvas.SetLeft(circle, Canvas.GetLeft(circle) - res.X);
                Canvas.SetTop(circle, Canvas.GetTop(circle) - res.Y);

                Canvas.SetLeft(crossCenter, Canvas.GetLeft(crossCenter) - res.X);
                Canvas.SetTop(crossCenter, Canvas.GetTop(crossCenter) - res.Y);

                Canvas.SetLeft(crossOuter, Canvas.GetLeft(crossOuter) - res.X);
                Canvas.SetTop(crossOuter, Canvas.GetTop(crossOuter) - res.Y);

                FirstPoint = mousePoint;
            }
            // výpis kurzoru
            else
            {
                var mousePoint = ImgPointToDataPoint(e.GetPosition(ImageWindow));

                var x = (int) mousePoint.X;
                var y = (int) mousePoint.Y;                

                var toFind = new Point(x,y);

                var index = y * Data.Ncols + x;
                if (Data.Data != null && index < Data.Data.Count && index >= 0)
                {
                    UpdateInfo(x, y, Data.GetValueAt(toFind));
                }                    
            }
        }

        private void RightUpImg(object sender, MouseButtonEventArgs e)
        {
            ImageWindow.ReleaseMouseCapture(); ;
        }

        // načtení souboru
        private void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelect();

            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*"; 
            var result = openFileDialog.ShowDialog();

            if (result == true)
            {                
                try
                {
                    // zpracování dat
                    Parser.ParseFile(Data,openFileDialog.FileName);
                    // zobrazení dat
                    ImageWindow.Source = ViewUtils.CreateBitmap(Data);
                    // úprava velikosti zobrazení
                    NormalizeScale();
                    ResetPosition();
                    MaxValues = Utils.GetMaxValue(Data.Data, false);
                    MinValues = Utils.GetMaxValue(Data.Data, true);                   

                    listView1.ItemsSource = MaxValues;
                    listView2.ItemsSource = MinValues;

                }
                catch
                {
                    MessageBox.Show("Neplatný formát souboru.\n \n" +
                        "Očekávaný formát:\n" +
                        "ncols počet sloupců matice \n" +
                        "nrows počet řádků matice \n" +
                        "xllcorner x-ová souřadnice levého spodního rohu výškové mapy\n" +
                        "yllcorner y-ová souřadnice levého spodního rohu výškové mapy\n" +
                        "cellsize – rozteč mezi body výškové mapy\n" +
                        "Dále pak následuje matice bodů výškové mapy," +
                        "jejich počet musí být ncols x nrows.\n" +
                        "Jednotlivé hodnoty jsou oddělené mezerama a řádky oddělené odřádkováním.", 
                        "CHYBA"
                    );                    
                }                
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Tlačítkem \"Otevřít soubor\" nahrajte textový soubor.\n\n" +
                "Přibližování/Oddalování otočením kolečka myši.\n\n" +
                "Posun po mapě přidržením pravého tlačítka a posunem myši.\n\n" +
                "Kruhový výběr: \n" +
                "   Výběr středu výběru kliknutím levého tlačítka.\n" +
                "   Výběr poloměru druhým kliknutím levého tlačítka.\n\n" +
                "Výběr možno zrušit kolečkem myši nabo stiskem ESC.",
                "Nápověda"
            );
        }

        #region UTILS
        
        private void CenterCircleSet(Point center)
        {
            // střed výběru
            crossCenter.Stroke = brush;
            Canvas.SetLeft(crossCenter, center.X);
            Canvas.SetTop(crossCenter, center.Y);

            MaxValues.Clear();
            MinValues.Clear();
            listView1.ItemsSource = MaxValues;
            listView2.ItemsSource = MinValues;
        }

        // vykreslení kruhu
        private void SetCircle(int diameter, double radiusImg)
        {
            circle.Stroke = brush;
            circle.Width = diameter;
            circle.Height = diameter;

            Canvas.SetLeft(circle, SelectCenter.X - circle.Width / 2);
            Canvas.SetTop(circle, SelectCenter.Y - circle.Width / 2);

            circleRadius.Content = "Poloměr: " + Math.Round(radiusImg, 2); 
        }

        private void PointOnCircleDraw(Point point)
        {
            crossOuter.Stroke = brush;
            Canvas.SetLeft(crossOuter, point.X);
            Canvas.SetTop(crossOuter, point.Y);
        }

        private void SecondSelectSet(Point pointToDraw, Point pointCoord)
        {
            // kružnice výběru            
            var radiusToDraw = (int)Utils.CalculateDistance(SelectCenter, pointToDraw);
            // průměr pro vykreslování
            var diameterToDraw = radiusToDraw * 2;
            // poloměr pro výpočet souřadnic
            var radiusCoord = Utils.CalculateDistance(SelectCenterImg, pointCoord);

            SetCircle(diameterToDraw, radiusCoord);

            // bod výběru
            PointOnCircleDraw(pointToDraw);

            // získání max a min hodnot
            var dataInCircle = Data.GetCoordsInCircle(SelectCenterImg, radiusCoord);
            MaxValues = Utils.GetMaxValue(dataInCircle, false);
            MinValues = Utils.GetMaxValue(dataInCircle, true);          

            // obnovení listview
            listView1.ItemsSource = MaxValues;
            listView2.ItemsSource = MinValues;
        }

        private void RemoveSelect()
        {
            // zneviditelnění výběru
            circle.Stroke = null;
            crossCenter.Stroke = null;
            crossOuter.Stroke = null;
        }

        // výpis informací o kurzoru              
        private void UpdateInfo(double x, double y, int val)
        {
            LabelX.Content = "X: " + Math.Round(Data.Xllcorner + x * Data.Cellsize,2);
            LabelY.Content = "Y: " + Math.Round(Data.Yllcorner + y * Data.Cellsize,2);
            LabelVal.Content = "Hodnota: " + val;
        }

        private void ResetPosition() 
        {
            Canvas.SetLeft(ImageWindow, 10);
            Canvas.SetTop(ImageWindow, 10);
        }

        private void NormalizeScale()
        {
            ImageWindow.RenderTransform = DefaultTransform;
            ImageWindow.Width = Data.Ncols;
            ImageWindow.Height = Data.Nrows;
            var scaleSpeed = 300f / Data.Nrows;

            ViewUtils.ZoomImage(true, new Point(0, 0), ImageWindow, scaleSpeed);
        }

        private Point ImgPointToDataPoint(Point imgCoords)
        {
            return new Point(imgCoords.X, Data.Nrows - imgCoords.Y);
        }

        private void StoreScale() {
            Matrix mat = ImageWindow.RenderTransform.Value;
            DefaultTransform = new MatrixTransform(mat);
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                RemoveSelect();
            }
        }
        #endregion
    }
}
