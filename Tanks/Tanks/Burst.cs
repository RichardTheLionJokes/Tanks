using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Tanks
{
    public class Burst //класс взрыв
    {
        Bitmap BurstPict = new Bitmap(@"Pictures/burst.png"); //взрыв
        //Bitmap BurstPict = new Bitmap(@"Pictures/splash.png"); //вcплеск
        Bitmap BloodPict = new Bitmap(@"Pictures/blood.png"); //кровь

        public byte Iter; //счетчик для изменения картинки
        public bool removed; //нужно ли удалить взрыв
        public Bitmap BBIT; //картинка взрыва
        public Rectangle fromRect; //прямоугольник с нужной картинкой взрыва в спрайте
        public Rectangle toRect; //прямоугольник, в котором нужно нарисовать взрыв на карте

        public Burst(Rectangle toRect, byte iter, byte typeIndex)
        {
            Iter = iter;
            removed = false;
            fromRect = new Rectangle(0, 0, 32, 32);
            this.toRect = toRect;
            //выбор картинки в зависимости от типа взрыва
            if (typeIndex == 1) BBIT = BurstPict;
            if (typeIndex == 2) BBIT = BloodPict;
        }

        public void ChangePicture() //изменение картинки взрыва
        {
            if (Iter == 27) removed = true;
            switch (Iter)
            {
                case 1:
                    fromRect = new Rectangle(0, 0, 32, 32);
                    break;
                case 7:
                    fromRect = new Rectangle(32, 0, 32, 32);
                    break;
                case 11:
                    fromRect = new Rectangle(64, 0, 32, 32);
                    break;
                case 15:
                    fromRect = new Rectangle(96, 0, 32, 32);
                    break;
                case 19:
                    fromRect = new Rectangle(128, 0, 32, 32);
                    break;
                case 23:
                    fromRect = new Rectangle(160, 0, 32, 32);
                    break;
                case 27:
                    fromRect = new Rectangle(192, 0, 32, 32);
                    break;
            }
            Iter++;
        }
    }
}