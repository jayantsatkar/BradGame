using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.DirectX.Direct3D;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Slot_Simulator
{
    class ReelImages
    {
        static Device m_device;
        internal static Dictionary<string, Texture> Textures;
        internal static Dictionary<string, Bitmap> Bitmaps;
        internal static Dictionary<string, Animation> Animations;
        internal static Dictionary<string, int[]> imageDimensions;
        internal static int originalBackgroundHeight = 0;
        static ReelImages()
        {
            Textures = new Dictionary<string, Texture>();
            Bitmaps = new Dictionary<string, Bitmap>();
            Animations = new Dictionary<string, Animation>();
            imageDimensions = new Dictionary<string, int[]>();
        }
        internal static void Initialize(Device _device)
        {
            m_device = _device;
            foreach(string fileName in Bitmaps.Keys)
                Textures[fileName] = new Texture(m_device, Bitmaps[fileName], Usage.None, Pool.Managed);
        }
        internal static void AddImage(FileInfo _fileInfo, double _conversionRatio)
        {
            string fileName = _fileInfo.Name.Substring(0, _fileInfo.Name.Length - _fileInfo.Extension.Length).ToLower();
            if (Bitmaps.ContainsKey(fileName))
            {
                Bitmaps[fileName].Dispose();
                if(m_device != null) Textures[fileName].Dispose();
            }
            Bitmap BitmapToUse = new Bitmap(_fileInfo.FullName);
            if (fileName == "background") originalBackgroundHeight = BitmapToUse.Height;
            Bitmaps[fileName] = getNewBitmap(BitmapToUse, (int)(BitmapToUse.Width * _conversionRatio), (int)(BitmapToUse.Height * _conversionRatio));
            if (fileName != "winframe" && fileName != "winframe2" && fileName != "ss" && fileName != "re") Bitmaps[fileName] = getNewBitmap(BitmapToUse, (int)(BitmapToUse.Width * _conversionRatio), (int)(BitmapToUse.Height * _conversionRatio));
            else if (fileName == "winframe" || fileName == "winframe2" || fileName == "ss" || fileName == "re") Bitmaps[fileName] = getNewBitmap(BitmapToUse, imageDimensions["l4"][0], imageDimensions["l4"][1]);
            imageDimensions.Add(fileName, new int[] { Bitmaps[fileName].Width, Bitmaps[fileName].Height });
            if (m_device != null) Textures[fileName] = new Texture(m_device, Bitmaps[fileName], Usage.None, Pool.Managed);
            string winFolder = string.Format(@"{0}\{1}",_fileInfo.DirectoryName, fileName);
            if (Directory.Exists(winFolder))
                foreach (FileInfo fileInfo in new DirectoryInfo(winFolder).GetFiles())
                    if (fileInfo.Extension == ".png")
                        AddImage(fileInfo, _conversionRatio);
        }
        internal static void AddAnimation(FileInfo _textFileInfo)
        {
            using (StreamReader streamReader = new StreamReader(_textFileInfo.FullName))
            {
                int numberTotalLength = int.Parse(streamReader.ReadLine());
                string upTillUnderScore = string.Format(string.Format("{0}_", _textFileInfo.FullName.Substring(0, _textFileInfo.FullName.Length - 4)));
                string zeroFile = string.Format("{0}{1}.png", upTillUnderScore, (0).ToString().PadLeft(numberTotalLength, '0'));
                int startingIndex = File.Exists(zeroFile) ? 0 : 1;
                string directory = _textFileInfo.DirectoryName;
                List<Texture> textures = new List<Texture>();
                List<Point> offets = new List<Point>();
                while (!streamReader.EndOfStream)
                {
                    string file = string.Format("{0}{1}.png", upTillUnderScore, (startingIndex++).ToString().PadLeft(numberTotalLength, '0'));
                    if (!File.Exists(file))
                        throw new ArgumentException("Cannot find file: ", file);
                    using (Bitmap bitmap = new Bitmap(file))
                    {
                        textures.Add(new Texture(m_device, bitmap, Usage.None, Pool.Managed));
                        bitmap.Dispose();
                        string[] offsetSting = streamReader.ReadLine().Split(new char[] { '\t' });
                        offets.Add(new Point(int.Parse(offsetSting[0]), int.Parse(offsetSting[1])));
                    }
                }
                Animations[_textFileInfo.Name.Substring(0,_textFileInfo.Name.Length - 4).ToLower()] = new Animation(textures, offets);
                streamReader.Close();
            }
        }

        internal static Bitmap getNewBitmap(Image _image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(_image.HorizontalResolution, _image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(_image, destRect, 0, 0, _image.Width, _image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }
    }
}
