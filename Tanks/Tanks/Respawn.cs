using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Tanks
{
    public class Respawn
    {
        Bitmap EnRespBit = new Bitmap(@"Pictures/star.png"); //вражеский респаун
        Bitmap MyRespBit = new Bitmap(@"Pictures/myResp3.png"); //мой респаун

        tankTeam team;
        public int x; //координаты респауна
        public int y;
        public int dx; //размеры респауна
        public int dy;
        public byte Iter; //счетчик для изменения картинки
        byte IterIncr; //счетчик, считающий общее количество проходок счетчика Iter вверх и вниз (для пульсации)
        public bool removed; //нужно ли удалить взрыв
        public Bitmap RBIT; //картинка взрыва
        public Rectangle fromRect; //прямоугольник с нужной картинкой взрыва в спрайте
        public Rectangle toRect; //прямоугольник, в котором нужно нарисовать взрыв на карте

        public Respawn(tankTeam team, int x, int y, int dx, int dy, byte iter, byte typeIndex)
        {
            this.team = team;
            this.x = x;
            this.y = y;
            this.dx = dx;
            this.dy = dy;
            Iter = iter;
            IterIncr = 0;
            removed = false;
            fromRect = new Rectangle(0, 0, 32, 32);
            toRect = new Rectangle(x, y, dx, dy);
            //выбор картинки в зависимости от типа респауна
            if (typeIndex == 1) RBIT = EnRespBit;
            if (typeIndex == 2) RBIT = MyRespBit;
        }

        internal void ChangePicture(List<EnemyTank> enemyList) //изменение картинки респауна вражеских танков
        {
            SelectPicture();
            if (IterIncr == 8 && Iter == 8) //создание танка незадолго до окончания пульсации
            {
                enemyList.Add(new EnemyTank(x, y, dx, dy, moveDirectionEnum.Down, team));
            }
            if (IterIncr == 9) //удаление респауна
            {
                removed = true;
            }
            //пульсация
            if (IterIncr % 2 == 0) Iter--;
            else Iter++;
        }

        internal void ChangePicture(MyTank myTank) //изменение картинки респауна своего танка
        {
            SelectPicture();
            if (IterIncr == 8 && Iter == 8) //появление своего танка незадолго до окончания пульсации
            {
                myTank.immob = false;
                myTank.dir = moveDirectionEnum.Up;
                myTank.changeDir = true;
                myTank.MoveTank();
                myTank.y += 2;
            }
            if (IterIncr == 9) //удаление респауна
            {
                removed = true;
            }
            //пульсация
            if (IterIncr % 2 == 0) Iter--;
            else Iter++;
        }

        private void SelectPicture() //выбор картинки респауна
        {
            switch (Iter)
            {
                case 0:
                    fromRect = new Rectangle(0, 0, 32, 32);
                    IterIncr++;
                    break;
                case 4:
                    fromRect = new Rectangle(32, 0, 32, 32);
                    break;
                case 8:
                    fromRect = new Rectangle(64, 0, 32, 32);
                    break;
                case 12:
                    fromRect = new Rectangle(96, 0, 32, 32);
                    break;
                case 16:
                    fromRect = new Rectangle(128, 0, 32, 32);
                    break;
                case 20:
                    fromRect = new Rectangle(160, 0, 32, 32);
                    break;
                case 24:
                    fromRect = new Rectangle(192, 0, 32, 32);
                    IterIncr++;
                    break;
            }
        }
    }
}
