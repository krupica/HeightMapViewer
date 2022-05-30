using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Heightmap.Classes;

namespace Heightmap.Helpers
{
    public class ViewUtils
    {
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
