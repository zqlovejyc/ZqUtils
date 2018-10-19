#region License
/***
 * Copyright © 2018, 张强 (943620963@qq.com).
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * without warranties or conditions of any kind, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using System;
using System.Collections;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
/****************************
* [Author] 张强
* [Date] 2016-09-27
* [Describe] 图片工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// 图片帮助类
    /// </summary>
    public class ImageHelper
    {
        #region 缩略图
        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originalImagePath">源图路径（物理路径）</param>
        /// <param name="thumbnailPath">缩略图路径（物理路径）</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="mode">生成缩略图的方式</param>    
        /// <returns>bool</returns>
        public static bool Thumbnail(string originalImagePath, string thumbnailPath, int width, int height, string mode)
        {
            var res = false;
            try
            {
                using (var originalImage = Image.FromFile(originalImagePath))
                {
                    var towidth = width;
                    var toheight = height;
                    var x = 0;
                    var y = 0;
                    var ow = originalImage.Width;
                    var oh = originalImage.Height;
                    switch (mode)
                    {
                        case "HW":  //指定高宽缩放（可能变形）                
                            break;
                        case "W":   //指定宽，高按比例                    
                            toheight = originalImage.Height * width / originalImage.Width;
                            break;
                        case "H":   //指定高，宽按比例
                            towidth = originalImage.Width * height / originalImage.Height;
                            break;
                        case "Cut": //指定高宽裁减（不变形）                
                            if ((double)originalImage.Width / originalImage.Height > towidth / (double)toheight)
                            {
                                oh = originalImage.Height;
                                ow = originalImage.Height * towidth / toheight;
                                y = 0;
                                x = (originalImage.Width - ow) / 2;
                            }
                            else
                            {
                                ow = originalImage.Width;
                                oh = originalImage.Width * height / towidth;
                                x = 0;
                                y = (originalImage.Height - oh) / 2;
                            }
                            break;
                        default:
                            break;
                    }
                    //新建一个bmp图片
                    using (var bitmap = new Bitmap(towidth, toheight))
                    {
                        //新建一个画板
                        using (var g = Graphics.FromImage(bitmap))
                        {
                            //设置高质量插值法
                            g.InterpolationMode = InterpolationMode.High;
                            //设置高质量,低速度呈现平滑程度
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            //清空画布并以透明背景色填充
                            g.Clear(Color.Transparent);
                            //在指定位置并且按指定大小绘制原图片的指定部分
                            g.DrawImage(originalImage, new Rectangle(0, 0, towidth, toheight), new Rectangle(x, y, ow, oh), GraphicsUnit.Pixel);
                            //以jpg格式保存缩略图
                            bitmap.Save(thumbnailPath, ImageFormat.Jpeg);
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "生成缩略图");
                res = false;
            }
            return res;
        }
        #endregion

        #region 图片水印
        /// <summary>
        /// 图片水印处理方法
        /// </summary>
        /// <param name="path">需要加载水印的图片路径（绝对路径）</param>
        /// <param name="waterpath">水印图片（绝对路径）</param>
        /// <param name="location">水印位置（传送正确的代码）</param>
        /// <returns>返回图片水印处理后的文件路径</returns>
        public static string Watermark(string path, string waterpath, string location)
        {
            var kz_name = Path.GetExtension(path);
            if (kz_name == ".jpg" || kz_name == ".bmp" || kz_name == ".jpeg" || kz_name == ".png")
            {
                var time = DateTime.Now;
                var filename = "" + time.Year.ToString() + time.Month.ToString() + time.Day.ToString() + time.Hour.ToString() + time.Minute.ToString() + time.Second.ToString() + time.Millisecond.ToString();
                using (var img = Image.FromFile(path))
                {
                    using (var waterimg = Image.FromFile(waterpath))
                    {
                        using (var g = Graphics.FromImage(img))
                        {
                            var loca = GetLocation(location, img, waterimg);
                            g.DrawImage(waterimg, new Rectangle(int.Parse(loca[0].ToString()), int.Parse(loca[1].ToString()), waterimg.Width, waterimg.Height));
                        }
                    }
                    var newpath = Path.GetDirectoryName(path) + filename + kz_name;
                    img.Save(newpath);
                    File.Copy(newpath, path, true);
                    if (File.Exists(newpath)) File.Delete(newpath);
                }
            }
            return path;
        }

        /// <summary>
        /// 图片水印位置处理方法
        /// </summary>
        /// <param name="location">水印位置</param>
        /// <param name="img">需要添加水印的图片</param>
        /// <param name="waterimg">水印图片</param>
        /// <returns>返回图片水印位置</returns>
        private static ArrayList GetLocation(string location, Image img, Image waterimg)
        {
            var loca = new ArrayList();
            var x = 0;
            var y = 0;
            if (location == "LT")
            {
                x = 10;
                y = 10;
            }
            else if (location == "T")
            {
                x = img.Width / 2 - waterimg.Width / 2;
                y = img.Height - waterimg.Height;
            }
            else if (location == "RT")
            {
                x = img.Width - waterimg.Width;
                y = 10;
            }
            else if (location == "LC")
            {
                x = 10;
                y = img.Height / 2 - waterimg.Height / 2;
            }
            else if (location == "C")
            {
                x = img.Width / 2 - waterimg.Width / 2;
                y = img.Height / 2 - waterimg.Height / 2;
            }
            else if (location == "RC")
            {
                x = img.Width - waterimg.Width;
                y = img.Height / 2 - waterimg.Height / 2;
            }
            else if (location == "LB")
            {
                x = 10;
                y = img.Height - waterimg.Height;
            }
            else if (location == "B")
            {
                x = img.Width / 2 - waterimg.Width / 2;
                y = img.Height - waterimg.Height;
            }
            else
            {
                x = img.Width - waterimg.Width;
                y = img.Height - waterimg.Height;
            }
            loca.Add(x);
            loca.Add(y);
            return loca;
        }
        #endregion

        #region 文字水印
        /// <summary>
        /// 文字水印处理方法
        /// </summary>
        /// <param name="path">图片路径（绝对路径）</param>
        /// <param name="size">字体大小</param>
        /// <param name="letter">水印文字</param>
        /// <param name="color">颜色</param>
        /// <param name="location">水印位置</param>
        /// <returns>返回文字水印处理后的文件路径</returns>
        public static string LetterWatermark(string path, int size, string letter, Color color, string location)
        {
            var kz_name = Path.GetExtension(path);
            if (kz_name == ".jpg" || kz_name == ".bmp" || kz_name == ".jpeg" || kz_name == ".png")
            {
                var time = DateTime.Now;
                var filename = "" + time.Year.ToString() + time.Month.ToString() + time.Day.ToString() + time.Hour.ToString() + time.Minute.ToString() + time.Second.ToString() + time.Millisecond.ToString();
                using (var img = Image.FromFile(path))
                {
                    using (var gs = Graphics.FromImage(img))
                    {
                        var loca = GetLocation(location, img, size, letter.Length);
                        using (var font = new Font("宋体", size))
                        {
                            var br = new SolidBrush(color);
                            gs.DrawString(letter, font, br, float.Parse(loca[0].ToString()), float.Parse(loca[1].ToString()));
                        }
                    }
                    var newpath = Path.GetDirectoryName(path) + filename + kz_name;
                    img.Save(newpath);
                    File.Copy(newpath, path, true);
                    if (File.Exists(newpath)) File.Delete(newpath);
                }
            }
            return path;
        }

        /// <summary>
        /// 文字水印位置的方法
        /// </summary>
        /// <param name="location">位置代码</param>
        /// <param name="img">图片对象</param>
        /// <param name="width">宽(当水印类型为文字时,传过来的就是字体的大小)</param>
        /// <param name="height">高(当水印类型为文字时,传过来的就是字符的长度)</param>
        private static ArrayList GetLocation(string location, Image img, int width, int height)
        {
            var loca = new ArrayList();  //定义数组存储位置
            var x = 10;
            var y = 10;
            if (location == "LT")
            {
                loca.Add(x);
                loca.Add(y);
            }
            else if (location == "T")
            {
                x = img.Width / 2 - (width * height) / 2;
                loca.Add(x);
                loca.Add(y);
            }
            else if (location == "RT")
            {
                x = img.Width - width * height;
            }
            else if (location == "LC")
            {
                y = img.Height / 2;
            }
            else if (location == "C")
            {
                x = img.Width / 2 - (width * height) / 2;
                y = img.Height / 2;
            }
            else if (location == "RC")
            {
                x = img.Width - height;
                y = img.Height / 2;
            }
            else if (location == "LB")
            {
                y = img.Height - width - 5;
            }
            else if (location == "B")
            {
                x = img.Width / 2 - (width * height) / 2;
                y = img.Height - width - 5;
            }
            else
            {
                x = img.Width - width * height;
                y = img.Height - width - 5;
            }
            loca.Add(x);
            loca.Add(y);
            return loca;
        }
        #endregion

        #region 调整光暗
        /// <summary>
        /// 调整光暗
        /// </summary>
        /// <param name="mybm">原始图片</param>
        /// <param name="width">原始图片的长度</param>
        /// <param name="height">原始图片的高度</param>
        /// <param name="val">增加或减少的光暗值</param>
        /// <returns>返回调整光暗后的图片</returns>
        public static Bitmap LDPic(Bitmap mybm, int width, int height, int val)
        {
            var bm = new Bitmap(width, height);//初始化一个记录经过处理后的图片对象
            int x, y, resultR, resultG, resultB;//x、y是循环次数，后面三个是记录红绿蓝三个值的
            Color pixel;
            for (x = 0; x < width; x++)
            {
                for (y = 0; y < height; y++)
                {
                    pixel = mybm.GetPixel(x, y);//获取当前像素的值
                    resultR = pixel.R + val;//检查红色值会不会超出[0, 255]
                    resultG = pixel.G + val;//检查绿色值会不会超出[0, 255]
                    resultB = pixel.B + val;//检查蓝色值会不会超出[0, 255]
                    bm.SetPixel(x, y, Color.FromArgb(resultR, resultG, resultB));//绘图
                }
            }
            return bm;
        }
        #endregion

        #region 反色处理
        /// <summary>
        /// 反色处理
        /// </summary>
        /// <param name="mybm">原始图片</param>
        /// <param name="width">原始图片的长度</param>
        /// <param name="height">原始图片的高度</param>
        /// <returns>返回反色处理后的图片</returns>
        public static Bitmap RePic(Bitmap mybm, int width, int height)
        {
            var bm = new Bitmap(width, height);//初始化一个记录处理后的图片的对象
            int x, y, resultR, resultG, resultB;
            Color pixel;
            for (x = 0; x < width; x++)
            {
                for (y = 0; y < height; y++)
                {
                    pixel = mybm.GetPixel(x, y);//获取当前坐标的像素值
                    resultR = 255 - pixel.R;//反红
                    resultG = 255 - pixel.G;//反绿
                    resultB = 255 - pixel.B;//反蓝
                    bm.SetPixel(x, y, Color.FromArgb(resultR, resultG, resultB));//绘图
                }
            }
            return bm;
        }
        #endregion

        #region 浮雕处理
        /// <summary>
        /// 浮雕处理
        /// </summary>
        /// <param name="oldBitmap">原始图片</param>
        /// <param name="Width">原始图片的长度</param>
        /// <param name="Height">原始图片的高度</param>
        /// <returns>返回浮雕处理后的图片</returns>
        public static Bitmap FD(Bitmap oldBitmap, int Width, int Height)
        {
            var newBitmap = new Bitmap(Width, Height);
            Color color1, color2;
            for (int x = 0; x < Width - 1; x++)
            {
                for (int y = 0; y < Height - 1; y++)
                {
                    int r = 0, g = 0, b = 0;
                    color1 = oldBitmap.GetPixel(x, y);
                    color2 = oldBitmap.GetPixel(x + 1, y + 1);
                    r = Math.Abs(color1.R - color2.R + 128);
                    g = Math.Abs(color1.G - color2.G + 128);
                    b = Math.Abs(color1.B - color2.B + 128);
                    if (r > 255) r = 255;
                    if (r < 0) r = 0;
                    if (g > 255) g = 255;
                    if (g < 0) g = 0;
                    if (b > 255) b = 255;
                    if (b < 0) b = 0;
                    newBitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            return newBitmap;
        }
        #endregion

        #region 拉伸图片
        /// <summary>
        /// 拉伸图片
        /// </summary>
        /// <param name="bmp">原始图片</param>
        /// <param name="newW">新的宽度</param>
        /// <param name="newH">新的高度</param>
        /// <returns>返回拉伸图片后的图片</returns>
        public static Bitmap Resize(Bitmap bmp, int newW, int newH)
        {
            try
            {
                var bap = new Bitmap(newW, newH);
                using (var g = Graphics.FromImage(bap))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(bap, new Rectangle(0, 0, newW, newH), new Rectangle(0, 0, bap.Width, bap.Height), GraphicsUnit.Pixel);
                }
                return bap;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "拉伸图片");
                return null;
            }
        }
        #endregion

        #region 滤色处理
        /// <summary>
        /// 滤色处理
        /// </summary>
        /// <param name="mybm">原始图片</param>
        /// <param name="width">原始图片的长度</param>
        /// <param name="height">原始图片的高度</param>
        /// <returns>返回滤色处理后的图片</returns>
        public static Bitmap FilPic(Bitmap mybm, int width, int height)
        {
            var bm = new Bitmap(width, height);//初始化一个记录滤色效果的图片对象
            int x, y;
            Color pixel;
            for (x = 0; x < width; x++)
            {
                for (y = 0; y < height; y++)
                {
                    pixel = mybm.GetPixel(x, y);//获取当前坐标的像素值
                    bm.SetPixel(x, y, Color.FromArgb(0, pixel.G, pixel.B));//绘图
                }
            }
            return bm;
        }
        #endregion

        #region 左右翻转
        /// <summary>
        /// 左右翻转
        /// </summary>
        /// <param name="mybm">原始图片</param>
        /// <param name="width">原始图片的长度</param>
        /// <param name="height">原始图片的高度</param>
        /// <returns>返回左右翻转后的图片</returns>
        public static Bitmap RevPicLR(Bitmap mybm, int width, int height)
        {
            var bm = new Bitmap(width, height);
            int x, y, z; //x,y是循环次数,z是用来记录像素点的x坐标的变化的
            Color pixel;
            for (y = height - 1; y >= 0; y--)
            {
                for (x = width - 1, z = 0; x >= 0; x--)
                {
                    pixel = mybm.GetPixel(x, y);//获取当前像素的值
                    bm.SetPixel(z++, y, Color.FromArgb(pixel.R, pixel.G, pixel.B));//绘图
                }
            }
            return bm;
        }
        #endregion

        #region 上下翻转
        /// <summary>
        /// 上下翻转
        /// </summary>
        /// <param name="mybm">原始图片</param>
        /// <param name="width">原始图片的长度</param>
        /// <param name="height">原始图片的高度</param>
        /// <returns>返回上下翻转后的图片</returns>
        public static Bitmap RevPicUD(Bitmap mybm, int width, int height)
        {
            var bm = new Bitmap(width, height);
            int x, y, z;
            Color pixel;
            for (x = 0; x < width; x++)
            {
                for (y = height - 1, z = 0; y >= 0; y--)
                {
                    pixel = mybm.GetPixel(x, y);//获取当前像素的值
                    bm.SetPixel(x, z++, Color.FromArgb(pixel.R, pixel.G, pixel.B));//绘图
                }
            }
            return bm;
        }
        #endregion

        #region 缩放图片
        /// <summary>
        /// 等比例缩放图片到指定尺寸
        /// </summary>
        /// <param name="oldFile">原文件</param>
        /// <param name="newFile">新文件</param>
        /// <param name="destHeight">目标高度</param>
        /// <param name="destWidth">目标宽度</param>
        /// <param name="q">压缩比例1-100，默认100</param>
        /// <returns>是否缩放成功</returns>
        public static bool Zoom(string oldFile, string newFile, int destHeight, int destWidth, int q = 100)
        {
            var res = false;
            try
            {
                using (var sourImage = Image.FromFile(oldFile))
                {
                    int width = 0,
                        height = 0,
                        sourWidth = sourImage.Width,
                        sourHeight = sourImage.Height;
                    if (sourHeight > destHeight || sourWidth > destWidth)
                    {
                        if ((sourWidth * destHeight) > (sourHeight * destWidth))
                        {
                            width = destWidth;
                            height = (destWidth * sourHeight) / sourWidth;
                        }
                        else
                        {
                            height = destHeight;
                            width = (sourWidth * destHeight) / sourHeight;
                        }
                    }
                    else
                    {
                        width = sourWidth;
                        height = sourHeight;
                    }
                    using (var destBitmap = new Bitmap(destWidth, destHeight))
                    {
                        using (var g = Graphics.FromImage(destBitmap))
                        {
                            g.Clear(Color.Transparent);
                            //设置画布的描绘质量           
                            g.CompositingQuality = CompositingQuality.HighQuality;
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.DrawImage(sourImage, new Rectangle((destWidth - width) / 2, (destHeight - height) / 2, width, height), 0, 0, sourImage.Width, sourImage.Height, GraphicsUnit.Pixel);
                        }
                        //设置压缩质量       
                        var encoderParams = new EncoderParameters();
                        var quality = new long[1];
                        quality[0] = q;
                        var encoderParam = new EncoderParameter(Encoder.Quality, quality);
                        encoderParams.Param[0] = encoderParam;
                        ImageCodecInfo ici = null;
                        var arrayICI = ImageCodecInfo.GetImageEncoders();
                        var fd = Path.GetExtension(oldFile).ToLower().Contains("png") ? "PNG" : "JPEG";
                        for (int i = 0; i < arrayICI.Length; i++)
                        {
                            if (arrayICI[i].FormatDescription.Equals(fd))
                            {
                                ici = arrayICI[i];
                                break;
                            }
                        }
                        //保存缩放后的图片
                        if (ici != null)
                        {
                            destBitmap.Save(newFile, ici, encoderParams);
                        }
                        else
                        {
                            destBitmap.Save(newFile);
                        }
                        res = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "等比例缩放图片到指定尺寸");
                res = false;
            }
            return res;
        }
        #endregion

        #region 图片灰度化
        /// <summary>
        /// 图片灰度化
        /// </summary>
        /// <param name="c">颜色</param>
        /// <returns>处理后的颜色</returns>
        public static Color Gray(Color c)
        {
            var rgb = Convert.ToInt32((double)(((0.3 * c.R) + (0.59 * c.G)) + (0.11 * c.B)));
            return Color.FromArgb(rgb, rgb, rgb);
        }
        #endregion

        #region 转换为黑白图片
        /// <summary>
        /// 转换为黑白图片
        /// </summary>
        /// <param name="mybm">要进行处理的图片</param>
        /// <param name="width">图片的长度</param>
        /// <param name="height">图片的高度</param>
        /// <returns>返回黑白图片</returns>
        public static Bitmap BWPic(Bitmap mybm, int width, int height)
        {
            var bm = new Bitmap(width, height);
            int x, y, result; //x,y是循环次数，result是记录处理后的像素值
            Color pixel;
            for (x = 0; x < width; x++)
            {
                for (y = 0; y < height; y++)
                {
                    pixel = mybm.GetPixel(x, y);//获取当前坐标的像素值
                    result = (pixel.R + pixel.G + pixel.B) / 3;//取红绿蓝三色的平均值
                    bm.SetPixel(x, y, Color.FromArgb(result, result, result));
                }
            }
            return bm;
        }
        #endregion

        #region 获取图片中的各帧
        /// <summary>
        /// 获取图片中的各帧
        /// </summary>
        /// <param name="pPath">图片路径</param>
        /// <param name="pSavePath">保存路径</param>
        public static void GetFrames(string pPath, string pSavePath)
        {
            using (var gif = Image.FromFile(pPath))
            {
                var fd = new FrameDimension(gif.FrameDimensionsList[0]);
                int count = gif.GetFrameCount(fd); //获取帧数(gif图片可能包含多帧，其它格式图片一般仅一帧)
                for (int i = 0; i < count; i++)    //以Jpeg格式保存各帧
                {
                    gif.SelectActiveFrame(fd, i);
                    gif.Save($"{pSavePath}\\frame_{i}.jpg", ImageFormat.Jpeg);
                }
            }
        }
        #endregion

        #region 图片生成字符画
        /// <summary>
        /// 根据图片创建字符画
        /// </summary>
        /// <param name="bitmap">图片</param>
        /// <param name="rowSize">行分隔粒度，默认：1</param>
        /// <param name="colSize">列分隔粒度，默认：1</param>
        /// <param name="type">填充类型：1-数字；2-复杂字繁体字；3-自定义格式，配合chars使用；0-默认</param>
        /// <param name="chars">当type为3的时候才有用，用于填充</param>
        /// <returns></returns>
        public static string BuildCharacter(Bitmap bitmap, int rowSize = 1, int colSize = 1, int type = 0, char[] chars = null)
        {
            var result = new System.Text.StringBuilder();
            char[] charset = null;
            switch (type)
            {
                case 1:
                    charset = new[] { ' ', '.', '1', '2', '0', '7', '5', '3', '4', '6', '9', '8' };
                    break;
                case 2:
                    charset = new[] { '丶', '卜', '乙', '日', '瓦', '車', '馬', '龠', '齱', '龖' };
                    break;
                case 3:
                    charset = chars;
                    break;
                default:
                    charset = new[] { ' ', '.', ',', ':', ';', 'i', '1', 'r', 's', '5', '3', 'A', 'H', '9', '8', '&', '@', '#' };
                    break;
            }
            var bitmapH = bitmap.Height;
            var bitmapW = bitmap.Width;
            for (var h = 0; h < bitmapH / rowSize; h++)
            {
                var offsetY = h * rowSize;
                for (int w = 0; w < bitmapW / colSize; w++)
                {
                    var offsetX = w * colSize;
                    float averBright = 0;
                    for (int j = 0; j < rowSize; j++)
                    {
                        for (int i = 0; i < colSize; i++)
                        {
                            try
                            {
                                var color = bitmap.GetPixel(offsetX + i, offsetY + j);
                                averBright += color.GetBrightness();
                            }
                            catch
                            {
                                averBright += 0;
                            }
                        }
                    }
                    averBright /= (rowSize * colSize);
                    int index = (int)(averBright * charset.Length);
                    if (index == charset.Length)
                    {
                        index--;
                    }
                    result.Append(charset[charset.Length - 1 - index]);
                }
                result.Append("\r\n");
            }
            return result.ToString();
        }
        #endregion
    }
}
