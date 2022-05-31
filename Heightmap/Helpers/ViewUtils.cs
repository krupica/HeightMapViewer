using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Heightmap.Classes;

namespace Heightmap.Helpers
{
    /// <summary>
    /// Třída obsahuje pomocné funkce pro zobrazení.
    /// </summary>
    public class ViewUtils
    {
        /// <summary>
        /// Upraví matici velikosti obrazu.
        /// </summary>
        /// <param name="zoomIn"></param>
        /// <param name="center"></param>
        /// <param name="imgWindow"></param>
        /// <param name="scaleSpeed"></param>
        public static void ZoomImage(bool zoomIn, Point center, Image imgWindow, float scaleSpeed)
        {            
            Matrix mat = imgWindow.RenderTransform.Value; 

            // přibližování
            if (zoomIn)
            {
                mat.ScaleAtPrepend(scaleSpeed, scaleSpeed, center.X, center.Y);
            }                
            else
            {
                mat.ScaleAtPrepend(1 / scaleSpeed, 1 / scaleSpeed, center.X, center.Y);
            }

            // aplikování transformace
            var mtf = new MatrixTransform(mat);
            imgWindow.RenderTransform = mtf;
        }

        /// <summary>
        /// Z vstupních dat vytvoří Bitmapu.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static BitmapSource CreateBitmap(HeightMapData data) 
        {
            PixelFormat pf = PixelFormats.Gray32Float;
            var width = data.Ncols;
            var height = data.Nrows;
            var rawStride = (width * pf.BitsPerPixel + 7) / 8;
            var dpi = 96;            

            // vytvoření bitmapy
            BitmapSource bitmap = BitmapSource.Create(width, height,
                dpi, dpi, pf, null,data.GetNormalizedArray(), rawStride);

            return bitmap;
        }              
    }
}
