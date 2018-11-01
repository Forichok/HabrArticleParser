using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace Article3000
{
    public sealed class Article<TData>
    {
        public HashSet<string> Tags { get; }
        public  TData Data { get; }
        public string Date { get; }
        public string Author { get; }
        public string Title { get; }
        public string Id { get; }
        public string Url => "https://habrahabr.ru/post/"+Id;

        public Article(string id, string title, string author, string date, TData data,HashSet<string>tags)
        {
            Id = id;
            Title = title;
            Tags=tags;
            Author = author;
            Data = data;
            Date = date;
        }

        public static Image ToImage(string text, Font font, Color textColor, Color backColor)
        {
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);
            SizeF textSize = drawing.MeasureString(text, font);
            
            img.Dispose();
            drawing.Dispose();
            
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);

            drawing = Graphics.FromImage(img);
            
            drawing.Clear(backColor);
            
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();
            textBrush.Dispose();
            drawing.Dispose();
            return img;
        }

        public string ToOptimalString(int maxStringLen=100)
        {
            string output = null;
            int strlen=0;
            bool isMaxLenReached = false;
            foreach (var ch in this.ToString())
            {
                strlen++;
                output += ch;
                if (ch == '\n')
                    strlen = 0;
                if (strlen >= maxStringLen)
                    isMaxLenReached = true;
                if (!(isMaxLenReached & (ch == ' ' | ch == '\r'))) continue;
                isMaxLenReached = false;
                output += '\n';
                strlen = 0;
            }
            return output;
        }

        public override string ToString()
        {
            string output =
                $" \n\n{Url}\n\n {Author}     {Date} \n\n      {Title}\n\n {Data} \n\n Tags: {string.Join(" | ", Tags.ToArray())}\n";
            return output;
        }
    }
}
