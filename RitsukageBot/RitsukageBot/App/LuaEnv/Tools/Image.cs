using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Gif.Components;

namespace Native.Csharp.App.LuaEnv.Tools
{
    class BaseImage
    {
        public Bitmap Source { get; set; }

        public Size Size { get => Source.Size; }

        public int Width { get => Source.Width; }

        public int Height { get => Source.Height; }

        public PixelFormat PixelFormat { get => Source.PixelFormat; }

        public ImageFormat ImageFormat = ImageFormat.Png;

        public string ImageFormatString
        {
            get => ImageExtensions.ImageFormatToString(ImageFormat);
            set => ImageFormat = ImageExtensions.StringToImageFormat(value);
        }

        public string DataUriScheme { get => System.Web.MimeMapping.GetMimeMapping(ImageFormatString); }

        public string ToBase64()
        {
            using MemoryStream ms = new MemoryStream();
            Source.Save(ms, ImageFormat);
            byte[] arr = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(arr, 0, (int)ms.Length);
            ms.Close();
            return Convert.ToBase64String(arr);
        }

        public BaseImage DrawText(int x, int y, string text, string type = "黑体", int size = 9, int r = 0, int g = 0, int b = 0)
        {
            using Graphics pic = GetGraphics();
            using Font font = new Font(type, size);
            Color myColor = Color.FromArgb(r, g, b);
            using SolidBrush myBrush = new SolidBrush(myColor);
            pic.DrawString(text, font, myBrush, new PointF(x, y));
            return this;
        }

        public BaseImage DrawRectangle(int x, int y, int width, int height, int r = 0, int g = 0, int b = 0)
        {
            using Graphics pic = GetGraphics();
            Color myColor = Color.FromArgb(r, g, b);
            using SolidBrush myBrush = new SolidBrush(myColor);
            pic.FillRectangle(myBrush, new Rectangle(x, y, width, height));
            return this;
        }

        public BaseImage DrawEllipse(int x, int y, int width, int height, int r = 0, int g = 0, int b = 0)
        {
            using Graphics pic = GetGraphics();
            Color myColor = Color.FromArgb(r, g, b);
            using Pen myBrush = new Pen(myColor);
            pic.DrawEllipse(myBrush, new Rectangle(x, y, width, height));
            return this;
        }

        public BaseImage DrawImage(string path, int x, int y, int width = 0, int height = 0)
        {
            if (!File.Exists(path))
                return this;
            using Bitmap b = new Bitmap(path);
            using Graphics pic = GetGraphics();
            if (width != 0 && height != 0)
                pic.DrawImage(b, x, y, width, height);
            else if (width == 0 && height == 0)
                pic.DrawImage(b, x, y);
            return this;
        }

        public BaseImage SetImageSize(int width, int height)
        {
            int basex = width < 0 ? -width : 0;
            int basey = height < 0 ? -height : 0;
            Bitmap img = new Bitmap(Math.Abs(width), Math.Abs(height));
            Graphics pic = GetGraphics(img);
            pic.DrawImage(Source, basex, basey, width, height);
            Source = img;
            return this;
        }

        public BaseImage CutImage(int x, int y, int width, int height)
        {
            int basex = width < 0 ? x - width : x;
            int basey = height < 0 ? y - height : y;
            int dx = width < 0 ? -1 : 1;
            int dy = height < 0 ? -1 : 1;
            Bitmap img = new Bitmap(Math.Abs(width), Math.Abs(height));
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    if (basex + dx * i >= 0 && basex + dx * i < Source.Width && basey + dy * j > 0 && basey + dy * j < Source.Height)
                        img.SetPixel(i, j, Source.GetPixel(basex + dx * i, basey + dy * j));
                }
            }
            Source = img;
            return this;
        }

        public BaseImage LeftRotateImage()
        {
            Bitmap img = new Bitmap(Height, Width);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    img.SetPixel(y, Width - x - 1, Source.GetPixel(x, y));
                }
            }
            Source = img;
            return this;
        }

        public BaseImage RightRotateImage()
        {
            Bitmap img = new Bitmap(Height, Width);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    img.SetPixel(Height - y - 1, x, Source.GetPixel(x, y));
                }
            }
            Source = img;
            return this;
        }

        public BaseImage TranslatePixel(int dx, int dy)
        {
            Bitmap img = new Bitmap(Width, Height);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    img.SetPixel(x, y, Source.GetPixel((Width + x - dx) % Width, (Height + y - dy) % Height));
                }
            }
            Source = img;
            return this;
        }

        public BaseImage TranslateHSV(float h, float s, float v)
        {
            ImageExtensions.HSVColor hsv;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    hsv = ImageExtensions.HSVColor.FromRGB(Source.GetPixel(x, y));
                    hsv.hue += h;
                    hsv.hue %= 360;
                    hsv.hue = hsv.hue < 0 ? 360 + hsv.hue : hsv.hue;
                    hsv.saturation += s;
                    hsv.saturation = Math.Min(Math.Max(hsv.saturation, 0), 100);
                    hsv.value += v;
                    hsv.value = Math.Min(Math.Max(hsv.value, 0), 100);
                    Source.SetPixel(x, y, hsv.ToRGB(Source.GetPixel(x, y).A));
                }
            }
            return this;
        }

        public BaseImage GetGrayImage()
        {
            int rgb;
            Color c;
            BaseImage img = Clone();
            Bitmap bmp = img.Source;
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    c = bmp.GetPixel(x, y);
                    rgb = (int)Math.Round(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);
                    bmp.SetPixel(x, y, Color.FromArgb(c.A, rgb, rgb, rgb));
                }
            }
            return img;
        }

        private Graphics GetGraphics(Bitmap img = null)
        {
            img ??= Source;
            Graphics pic = Graphics.FromImage(img);
            pic.SmoothingMode = SmoothingMode.HighQuality;
            pic.CompositingQuality = CompositingQuality.HighQuality;
            pic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            pic.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            return pic;
        }

        public string Save(string name)
        {
            Source.Save(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "data/image/" + name + ".luatemp", ImageFormat);
            return name + ".luatemp";
        }

        public string SaveAndDispose(string name)
        {
            string result = Save(name);
            Source.Dispose();
            return result;
        }

        public BaseImage Clone()
        {
            BaseImage img = new BaseImage
            {
                Source = (Bitmap)Source.Clone(),
                ImageFormat = ImageFormat
            };
            return img;
        }
    }

    class EmptyImage : BaseImage
    {
        public EmptyImage(int Width, int Height)
        {
            Source = new Bitmap(Width, Height);
        }
    }

    class MemoryImage : BaseImage
    {
        public MemoryImage(Image image)
        {
            Source = (Bitmap)image;
            ImageFormat = Source.RawFormat;
        }
    }

    class FileImage : BaseImage
    {
        public FileImage(string path)
        {
            Source = new Bitmap(path);
            ImageFormat = Source.RawFormat;
        }
    }

    class NetworkImage : BaseImage
    {
        public NetworkImage(string url)
        {
            Source = (Bitmap)url.GetImageFromNet().Clone();
            ImageFormat = Source.RawFormat;
        }
    }

    class GifManager
    {
        public ArrayList Frames = new ArrayList();

        public int FrameCount { get => Frames.Count; }

        public GifManager() {}

        public GifManager(string path)
        {
            GifDecoder gifDecoder = new GifDecoder();
            gifDecoder.Read(path);
            for (int i = 0, count = gifDecoder.GetFrameCount(); i < count; i++)
            {
                Frames.Add(new GifFrame(new MemoryImage((Bitmap)gifDecoder.GetFrame(i)), gifDecoder.GetDelay(i)));
            }
        }

        public string Save(string name)
        {
            AnimatedGifEncoder gifEncoder = new AnimatedGifEncoder();
            using FileStream fs = new FileStream(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "data/image/" + name + ".luatemp", FileMode.CreateNew, FileAccess.ReadWrite);
            gifEncoder.Start(fs);
            gifEncoder.SetRepeat(0);
            gifEncoder.SetQuality(256);
            gifEncoder.SetTransparent(Color.Black);
            for (int i = 0; i < FrameCount; i++)
            {
                GifFrame frame = (GifFrame)Frames[i];
                gifEncoder.SetDelay(frame.Delay);
                gifEncoder.AddFrame(frame.Image.Source);
            }
            gifEncoder.Finish();
            fs.Close();
            fs.Dispose();
            return name + ".luatemp";
        }
    }

    class GifFrame
    {
        public BaseImage Image;
        public int Delay = 1;

        public GifFrame() { }

        public GifFrame(BaseImage img, int delay = 1)
        {
            Image = img;
            Delay = delay;
        }
    }

    public static class ImageExtensions
    {
        public static Image GetImageFromNet(this string url, Action<WebRequest> requestAction = null, Func<WebResponse, Image> responseFunc = null)
        {
            return new Uri(url).GetImageFromNet(requestAction, responseFunc);
        }

        public static Image GetImageFromNet(this Uri url, Action<WebRequest> requestAction = null, Func<WebResponse, Image> responseFunc = null)
        {
            Image img;
            try
            {
                WebRequest request = WebRequest.Create(url);
                requestAction?.Invoke(request);
                using WebResponse response = request.GetResponse();
                if (responseFunc != null)
                {
                    img = responseFunc(response);
                }
                else
                {
                    img = Image.FromStream(response.GetResponseStream());
                }
            }
            catch
            {
                img = null;
            }
            return img;
        }

        public static string ImageFormatToString(ImageFormat format)
        {
            if (format.Equals(ImageFormat.Jpeg))
                return ".jpg";
            else if (format.Equals(ImageFormat.Png))
                return ".png";
            else if (format.Equals(ImageFormat.Gif))
                return ".gif";
            else if (format.Equals(ImageFormat.Bmp))
                return ".bmp";
            else if (format.Equals(ImageFormat.Icon))
                return ".ico";
            else
                return string.Empty;
        }

        public static ImageFormat StringToImageFormat(string format)
        {
            return format switch
            {
                ".jpg" => ImageFormat.Jpeg,
                ".jpeg" => ImageFormat.Jpeg,
                ".png" => ImageFormat.Png,
                ".gif" => ImageFormat.Gif,
                ".bmp" => ImageFormat.Bmp,
                ".ico" => ImageFormat.Icon,
                _ => null,
            };
        }

        public struct HSVColor
        {
            public float hue;
            public float saturation;
            public float value;

            public HSVColor(float h, float s, float v)
            {
                hue = h;
                saturation = s;
                value = v;
            }

            public Color ToRGB(int alpha = 255)
            {
                hue -= Convert.ToSingle(Math.Floor(hue / 360) * 360);
                saturation /= 100;
                value /= 100;
                byte v = Convert.ToByte(value * 255);
                if (saturation == 0)
                {
                    return Color.FromArgb(255, v, v, v);
                }
                int h = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
                float f = hue / 60 - h;
                byte a = Convert.ToByte(v * (1 - saturation));
                byte b = Convert.ToByte(v * (1 - saturation * f));
                byte c = Convert.ToByte(v * (1 - saturation * (1 - f)));
                switch (h)
                {
                    case 0:
                        return Color.FromArgb(alpha, v, c, a);
                    case 1:
                        return Color.FromArgb(alpha, b, v, a);
                    case 2:
                        return Color.FromArgb(alpha, a, v, c);
                    case 3:
                        return Color.FromArgb(alpha, a, b, v);
                    case 4:
                        return Color.FromArgb(alpha, c, a, v);
                    case 5:
                        return Color.FromArgb(alpha, v, a, b);
                    default:
                        throw new NotImplementedException();
                }
            }

            public static HSVColor FromRGB(Color RGB)
            {
                HSVColor hsv = new HSVColor();
                byte max = Math.Max(RGB.R, RGB.G);
                max = Math.Max(max, RGB.B);
                byte min = Math.Min(RGB.R, RGB.G);
                min = Math.Min(min, RGB.B);
                hsv.value = ((float)max) / 255;
                int mm = max - min;
                if (max == 0)
                {
                    hsv.saturation = 0;
                }
                else
                {
                    hsv.saturation = ((float)mm) / max;
                }
                if (mm == 0)
                {
                    hsv.hue = 0;
                }
                else if (RGB.R == max)
                {
                    hsv.hue = ((float)(RGB.G - RGB.B)) / mm * 60;
                }
                else if (RGB.G == max)
                {
                    hsv.hue = 120 + ((float)(RGB.B - RGB.R)) / mm * 60;
                }
                else if (RGB.B == max)
                {
                    hsv.hue = 240 + ((float)(RGB.R - RGB.G)) / mm * 60;
                }
                if (hsv.hue < 0) hsv.hue += 360;
                hsv.saturation *= 100;
                hsv.value *= 100;
                return hsv;
            }
        }
    }
}
