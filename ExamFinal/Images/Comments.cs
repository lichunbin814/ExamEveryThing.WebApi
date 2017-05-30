using ImageSharp;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Linq;

namespace ExamFinal.Images
{
    public class Comments
    {
        private static  FontCollection collection = new FontCollection();

        /*
         * 取得字型 - Google黑體
         */
        private Font getChFontOfGoogle()
        {
            string fontName = "cwTeXHei";
            var fontFamily = collection.Families.FirstOrDefault(_fontFamily => _fontFamily.Name == fontName);
            var isInstalled = fontFamily != null;
            if (isInstalled)
            {
                int emptyFontSize = 0;
                return new Font(fontFamily, emptyFontSize);
            }


            Stream fileStream = getFileStream("fonts", $"{fontName}-zhonly.ttf");
            var font =  collection.Install(fileStream);

            disposeForStream(fileStream);

            return font;
        }

        private string getAssertFilePath(string directoryName  , string fileName)
        {
            string splitSymbol = ".";
            string[] namespacePaths = new[] { "ExamFinal", "assert", directoryName };
            string fullPath = string.Join(splitSymbol, namespacePaths) + splitSymbol + fileName;

            return fullPath;
        }

        private Stream getFileStream(string directoryName, string fileName)
        {
            var assembly = typeof(Comments).GetTypeInfo().Assembly;
            string fileFullPath = getAssertFilePath(directoryName, fileName);
            System.IO.Stream stream = assembly.GetManifestResourceStream(fileFullPath);
            return stream;
        }

        private Stream getImageStream()
        {
            return getFileStream("images", "paper_toy.jpg");
        }

        public string GetBase64(float score)
        {
            Stream imgStream = getImageStream();
            Image<Rgba32> img = Image.Load(imgStream);

            SetTitle(img, "你好 恭喜你完成了測驗");
            SetScore(img, score);

            disposeForStream(imgStream);

            return img.ToBase64String();
        }

        private void disposeForStream(Stream stream)
        {
            stream.Flush();
            stream.Dispose();
        }

        private Image<Rgba32> SetScore(Image<Rgba32> img, float score)
        {
            int fontSizeOfScore = 100;
            Font scoreFont = new Font(FontCollection.SystemFonts.Find("Forte"), fontSizeOfScore);
            int scoreX = 870;
            int scoreY = 700;
            var fontColor = new Rgba32(244, 177, 131, 255);

            // 分數(數字）
            img.DrawText(
                score.ToString(),
                scoreFont,
                fontColor,
                new Vector2(scoreX, scoreY));

            int fontSizeOfScoreUnit = fontSizeOfScore / 2;
            Font fontOfScoreUnit = new Font(getChFontOfGoogle(), fontSizeOfScoreUnit);
            // 分數（單位）
            int scoreUnitX = scoreX;
            int scoreUnitY = scoreY + 50;
            int interceptOfscoreUnitX = 40;
            int digit = (Convert.ToInt32(score) / 10) + 1;
            if (digit <= 3)
            {
                int padding = 10;
                scoreUnitX = scoreUnitX + (digit * interceptOfscoreUnitX) + padding;
            }
            img.DrawText(
                "分",
                fontOfScoreUnit,
                fontColor,
                new Vector2(scoreUnitX, scoreUnitY));

            // 回傳
            return img;
        }



        private Image<Rgba32> SetTitle(Image<Rgba32> img, string text)
        {
            float padding = 20;

            Font font = getChFontOfGoogle();
            float widthOfTitle = img.Width - (padding * 2);
            float heightOfTitle = 120;
            float fontSizeOfTitle = 20;
            var fontOfTitle = GetSizeOfMeasureText(text, widthOfTitle, heightOfTitle, heightOfTitle, font, fontSizeOfTitle);

            Rgba32 fill = new Rgba32(64, 64, 64, 1f);

            img.DrawText(text, fontOfTitle
                , fill, new Vector2(padding), new ImageSharp.Drawing.TextGraphicsOptions
                {
                    TextAlignment = TextAlignment.Center,
                    WrapTextWidth = widthOfTitle
                });

            return img;
        }

        private Font GetSizeOfMeasureText(string text, float targetWidth, float targetHeight, float targetMinHeight, Font font, float fontSize)
        {
            var cloneFont = new Font(font, fontSize);
            TextMeasurer measurer = new TextMeasurer();

            targetMinHeight = targetHeight - fontSize;

            SixLabors.Fonts.Size size = new SixLabors.Fonts.Size(fontSize, fontSize);

            float scaleFactor = (cloneFont.Size / 2);// everytime we change direction we half this size
            int trapCount = (int)cloneFont.Size * 2;
            if (trapCount < 10)
            {
                trapCount = 10;
            }

            bool isTooSmall = false;
            while ((size.Height > targetHeight || size.Height < targetMinHeight) && trapCount > 0)
            {
                if (size.Height > targetHeight)
                {
                    if (isTooSmall)
                    {
                        scaleFactor = scaleFactor / 2;
                    }

                    cloneFont = new Font(cloneFont, cloneFont.Size - scaleFactor);
                    isTooSmall = false;
                }

                if (size.Height < targetMinHeight)
                {
                    if (!isTooSmall)
                    {
                        scaleFactor = scaleFactor / 2;
                    }
                    cloneFont = new Font(cloneFont, cloneFont.Size + scaleFactor);
                    isTooSmall = true;
                }
                trapCount--;

                var style = new FontSpan(cloneFont, 72)
                {
                    WrappingWidth = targetWidth
                };

                size = measurer.MeasureText(text, style);
            }

            return cloneFont;
        }
    }
}
