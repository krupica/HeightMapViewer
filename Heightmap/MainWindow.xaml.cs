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

        // střed výběru relativní k souřadnicím
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
       
        /// <summary>
        /// Zachytí pohyb kolečka myši, zavolá funkci pro přiblížení.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseScrollImg(object sender, MouseWheelEventArgs e)
        {
            RemoveSelect();
            var zoomIn = e.Delta > 0;
            ViewUtils.ZoomImage(zoomIn, e.GetPosition(ImageWindow), ImageWindow, scaleSpeedWheel);
        }

        /// <summary>
        /// První kliknutí vybere střed výběru a zruší předchozí výber.
        /// Druhé kliknutí výběr druhého bodu, volání funkce pro ošetření vytváření bodu a kružnice.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeftClickImg(object sender, MouseButtonEventArgs e)
        {
            if (!FirstPointSet)
            {
                SelectCenter = e.GetPosition(Canvas);
                SelectCenterImg = e.GetPosition(ImageWindow);                
                CenterX.Content = "X: " + Math.Round(SelectCenterImg.X, 2);
                CenterY.Content = "Y: " + Math.Round(SelectCenterImg.Y, 2);

                RemoveSelect();
                CenterCircleSet(SelectCenter);                
            }
            else 
            {
                var pointInImage = ImgPointToDataPoint(e.GetPosition(ImageWindow));
                SecondSelectSet(e.GetPosition(Canvas),pointInImage);                
            }

            FirstPointSet = !FirstPointSet;
        }

        /// <summary>
        /// Uloží se pozice odkud se má obraz posouvat.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RightClickImg(object sender, MouseButtonEventArgs e)
        {
            // odkud posouváme            
            FirstPoint = e.GetPosition(this);
            ImageWindow.CaptureMouse();            
        }
        
        
        /// <summary>
        /// Při drižení pravého tlačítka posouvá výběrové elementy a obraz.
        /// Jinak vypisuje na obrazovku informace o kurzoru.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseMoveImg(object sender, MouseEventArgs e)
        {
            // update pozice            
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Point mousePoint = e.GetPosition(this);
                Point res = new Point(FirstPoint.X - mousePoint.X, FirstPoint.Y - mousePoint.Y);
                Canvas.SetLeft(ImageWindow, Canvas.GetLeft(ImageWindow) - res.X);
                Canvas.SetTop(ImageWindow, Canvas.GetTop(ImageWindow) - res.Y);

                Canvas.SetLeft(Circle, Canvas.GetLeft(Circle) - res.X);
                Canvas.SetTop(Circle, Canvas.GetTop(Circle) - res.Y);

                Canvas.SetLeft(CrossCenter, Canvas.GetLeft(CrossCenter) - res.X);
                Canvas.SetTop(CrossCenter, Canvas.GetTop(CrossCenter) - res.Y);

                Canvas.SetLeft(CrossOuter, Canvas.GetLeft(CrossOuter) - res.X);
                Canvas.SetTop(CrossOuter, Canvas.GetTop(CrossOuter) - res.Y);

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

        /// <summary>
        /// Ukončení posouvání.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RightUpImg(object sender, MouseButtonEventArgs e)
        {
            ImageWindow.ReleaseMouseCapture(); ;
        }

        /// <summary>
        /// Zruší výberové okno, otevře výběr souboru, jeho zpracování, 
        /// vykreslení na obrazovku a zobrazení maximálních hodnot celé mapy.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                    ListView1.ItemsSource = MaxValues;
                    ListView2.ItemsSource = MinValues;

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

        /// <summary>
        /// Kontrola stisku ESC, zruší výběr.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                RemoveSelect();
            }
        }

        /// <summary>
        /// Vypíše nápovědu pro uživatele
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        
        /// <summary>
        /// Zobrazí střed výběru, resetuje okna hraničních hodnot.
        /// </summary>
        /// <param name="center"></param>
        private void CenterCircleSet(Point center)
        {
            // střed výběru
            CrossCenter.Stroke = brush;
            Canvas.SetLeft(CrossCenter, center.X);
            Canvas.SetTop(CrossCenter, center.Y);

            MaxValues.Clear();
            MinValues.Clear();
            ListView1.ItemsSource = MaxValues;
            ListView2.ItemsSource = MinValues;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="diameter"></param>
        /// <param name="radiusImg"></param>
        private void SetCircle(int diameter, double radiusImg)
        {
            Circle.Stroke = brush;
            Circle.Width = diameter;
            Circle.Height = diameter;

            Canvas.SetLeft(Circle, SelectCenter.X - Circle.Width / 2);
            Canvas.SetTop(Circle, SelectCenter.Y - Circle.Width / 2);

            circleRadius.Content = "Poloměr: " + Math.Round(radiusImg, 2); 
        }

        /// <summary>
        /// Vykreslí druhý bod výběru oblasti.
        /// </summary>
        /// <param name="point"></param>
        private void PointOnCircleDraw(Point point)
        {
            CrossOuter.Stroke = brush;
            Canvas.SetLeft(CrossOuter, point.X);
            Canvas.SetTop(CrossOuter, point.Y);
        }

        /// <summary>
        /// Vypočítá velikost kružnice pro vykreslení a pro získání hodnot.
        /// Nechá vykreslit kružnici a bod na ní.
        /// Získání hraničních hodnot.
        /// </summary>
        /// <param name="pointToDraw"></param>
        /// <param name="pointCoord"></param>
        private void SecondSelectSet(Point pointToDraw, Point pointCoord)
        {
            // poloměr kružnice výběru            
            var radiusToDraw = (int)Utils.CalculateDistance(SelectCenter, pointToDraw);
            // poloměr pro výpočet souřadnic
            var radiusCoord = Utils.CalculateDistance(SelectCenterImg, pointCoord);

            SetCircle(radiusToDraw*2, radiusCoord);

            // bod výběru
            PointOnCircleDraw(pointToDraw);

            // získání max a min hodnot
            var dataInCircle = Data.GetCoordsInCircle(SelectCenterImg, radiusCoord);
            MaxValues = Utils.GetMaxValue(dataInCircle, false);
            MinValues = Utils.GetMaxValue(dataInCircle, true);          

            // obnovení listview
            ListView1.ItemsSource = MaxValues;
            ListView2.ItemsSource = MinValues;
        }

        /// <summary>
        /// Zneviditelní výběrové okno.
        /// </summary>
        private void RemoveSelect()
        {
            // zneviditelnění výběru
            Circle.Stroke = null;
            CrossCenter.Stroke = null;
            CrossOuter.Stroke = null;
        }

        /// <summary>
        /// Převede souřadnice kurzoru na souřadnice relativní k měřítku a pozici mapy.
        /// Vypíše hodnotu v daném bodě.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="val"></param>
        private void UpdateInfo(double x, double y, int val)
        {
            LabelX.Content = "X: " + Math.Round(Data.Xllcorner + x * Data.Cellsize,2);
            LabelY.Content = "Y: " + Math.Round(Data.Yllcorner + y * Data.Cellsize,2);
            LabelVal.Content = "Hodnota: " + val;
        }

        /// <summary>
        /// Nastavení pozice obrazu na základní hodnoty.
        /// </summary>
        private void ResetPosition() 
        {
            Canvas.SetLeft(ImageWindow, 10);
            Canvas.SetTop(ImageWindow, 10);
        }

        /// <summary>
        /// Obnovení původního roztažení obrazu,
        /// obraz je následně roztažen v závislosti na velikosti dat na standardní velikost.
        /// </summary>
        private void NormalizeScale()
        {
            ImageWindow.RenderTransform = DefaultTransform;
            ImageWindow.Width = Data.Ncols;
            ImageWindow.Height = Data.Nrows;
            var scaleSpeed = 300f / Data.Nrows;

            ViewUtils.ZoomImage(true, new Point(0, 0), ImageWindow, scaleSpeed);
        }

        /// <summary>
        /// Překlopení souřadnice y, aby měla správnou orientaci.
        /// </summary>
        /// <param name="imgCoords"></param>
        /// <returns></returns>
        private Point ImgPointToDataPoint(Point imgCoords)
        {
            return new Point(imgCoords.X, Data.Nrows - imgCoords.Y);
        }

        /// <summary>
        /// Uloží původní nastavení velikosti okna.
        /// </summary>
        private void StoreScale() {
            Matrix mat = ImageWindow.RenderTransform.Value;
            DefaultTransform = new MatrixTransform(mat);
        }        
        #endregion
    }
}
