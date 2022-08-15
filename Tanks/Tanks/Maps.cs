using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Tanks
{
    static class Maps
    {
        static Bitmap map = new Bitmap(@"Pictures/map.png"); //спрайты карты

        public static void ReadMap(int[,] mapArr, string Path) //считывание карты из файла в массив
        {
            string sLine;
            char[] charArray = new char[] { ' ' };
            StreamReader objReader = new StreamReader(Path, Encoding.Default);
            for (int i = 0; i < 40; i++)
            {
                sLine = objReader.ReadLine();
                string[] strArray = sLine.Split(charArray);
                for (int j = 0; j < 40; j++)
                {
                    mapArr[i, j] = Int32.Parse(strArray[j]);
                }
            }
            objReader.Close();
        }

        public static void EmptyMap(int[,] mapArr) //считывание пустой карты (черный фон) в массив
        {
            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 40; j++)
                {
                    mapArr[i, j] = 0;
                }
            }
        }

        public static Bitmap MakeMap(int[,] mapArr) //создание карты из массива
        {
            Bitmap resultMap = new Bitmap(40 * 16, 40 * 16); //создание пустого битмапа
            Graphics mapGr = Graphics.FromImage(resultMap);
            GraphicsUnit units = GraphicsUnit.Pixel;
            for (int i = 0; i < 40; i++) //рисование текстур
            {
                for (int j = 0; j < 40; j++)
                {
                    if (mapArr[i, j] == 0) mapGr.FillRectangle(new SolidBrush(Color.Black), j * 16, i * 16, 16, 16); //черный фон
                    else
                    {
                        if (mapArr[i, j] == 2) //если лес, то сначала рисуем черный фон
                        {
                            mapGr.DrawImage(map, new Rectangle(j * 16, i * 16, 16, 16), new Rectangle(0, 0, 16, 16), units);
                        }
                        mapGr.DrawImage(map, new Rectangle(j * 16, i * 16, 16, 16), new Rectangle(mapArr[i, j] * 16, 0, 16, 16), units); //рисуем спрайт
                    }
                }
            }
            return resultMap;
        }

        public static void MakeMap(int[,] mapArr, int number, Graphics gr) //отрисовка конкретного спрайта из массива на карте
        {
            GraphicsUnit units = GraphicsUnit.Pixel;
            for (int i = 0; i < 40; i++) //рисование текстур
            {
                for (int j = 0; j < 40; j++)
                {
                    if (mapArr[i, j] == number)
                    {
                        gr.DrawImage(map, new Rectangle(j * 16, i * 16, 16, 16), new Rectangle(mapArr[i, j] * 16, 0, 16, 16), units);
                    }
                }
            }
        }

        public static Bitmap MakeSpritePanel() //создание панели спрайтов
        {
            Bitmap resultPanel = new Bitmap(3 * 32, 2 * 32); //создание пустого битмапа
            Graphics gr = Graphics.FromImage(resultPanel);
            GraphicsUnit units = GraphicsUnit.Pixel;
            //отрисовка панели 2x3
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    gr.DrawImage(map, new Rectangle(j * 32, i * 32, 32, 32), new Rectangle((i * 3 + j) * 16, 0, 16, 16), units);
                }
            }
            return resultPanel;
        }

        public static void drawMap(int[,] mapArr, int x, int y, int number, Graphics gr) //отрисовка выбранного спрайта в режиме конструктора карт
        {
            GraphicsUnit units = GraphicsUnit.Pixel;
            if (x >= 0 && x <=640 && y > 0 && y < 640)
            {
                gr.DrawImage(map, new Rectangle((x / 16) * 16, (y / 16) * 16, 16, 16), new Rectangle(number * 16, 0, 16, 16), units); //рисуем спрайт
                mapArr[y / 16, x / 16] = number; //записываем идентификатор спрайта в массив
            }
        }
    }
}