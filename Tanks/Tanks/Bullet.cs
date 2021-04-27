using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Tanks
{
    public class Bullet
    {
        Bitmap bullBit = new Bitmap(@"Pictures/bullet.png"); //пулька
        //Bitmap bullBit = new Bitmap(@"Pictures/drop.png"); //пулька 18+

        public int x; //координаты пульки
        public int y;
        public int dx; //размеры пульки
        public int dy;
        public moveDirectionEnum dir; //направление
        public bool removed; //нужно ли удалить пульку
        public checkVarEnum checkVar; //расположение препятствия
        public Bitmap BBIT; //картинка пульки
        public int burstX, burstY, burstDX, burstDY; //координаты и размеры взрыва

        public Bullet(int x, int y, int dx, int dy, moveDirectionEnum dir)
        {
            this.x = x;
            this.y = y;
            this.dx = dx;
            this.dy = dy;
            this.dir = dir;
            BBIT = bullBit;
            #region выбор картинки
            switch (dir)
            {
                case moveDirectionEnum.Left:
                    BBIT.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
                case moveDirectionEnum.Right:
                    BBIT.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case moveDirectionEnum.Down:
                    BBIT.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    break;
            }
            #endregion
            removed = false;
            checkVar = checkVarEnum.barNo;
            burstX = 0; burstY = 0; burstDX = 16; burstDY = 16;
        }

        public void CheckMap(int[,] arr) //проверка карты на наличие препятствий
        {
            checkVar = checkVarEnum.barNo;
            int iterX = x / 16;
            int iterY = y / 16;

            #region движение влево
            if (dir == moveDirectionEnum.Left)
            {
                if (iterX >= 0 && (arr[iterY, iterX] >= 4 || arr[iterY + 1, iterX] >= 4)) //если пулька сразу находится внутри препятствия
                {
                    checkVar = checkVarEnum.barLeft; burstX = iterX * 16; burstY = y - (burstDY - dy) / 2;
                    //разрушение кирпича
                    if (arr[iterY, iterX] == 4) { arr[iterY, iterX] = 0; }
                    if (arr[iterY + 1, iterX] == 4) { arr[iterY + 1, iterX] = 0; }
                }
                else if (x % 16 <= 10 && iterX > 0 && (arr[iterY, iterX - 1] >= 4 || arr[iterY + 1, iterX - 1] >= 4)) //если пулька приближается к препятствию (10 - шаг пульки)
                {
                    checkVar = checkVarEnum.barLeft; burstX = iterX * 16; burstY = y - (burstDY - dy) / 2;
                    //разрушение кирпича
                    if (arr[iterY, iterX - 1] == 4) { arr[iterY, iterX - 1] = 0; burstX = (iterX - 1) * 16; burstY = y - (burstDY - dy) / 2; }
                    if (arr[iterY + 1, iterX - 1] == 4) { arr[iterY + 1, iterX - 1] = 0; burstX = (iterX - 1) * 16; burstY = y - (burstDY - dy) / 2; }
                }
            }
            #endregion

            #region движение вправо
            if (dir == moveDirectionEnum.Right)
            {
                iterX = Convert.ToInt32(Math.Floor((double)(x + 8) / 16));
                if (iterX <= 39 && (arr[iterY, iterX] >= 4 || arr[iterY + 1, iterX] >= 4)) //если пулька сразу находится внутри препятствия
                {
                    checkVar = checkVarEnum.barRight; burstX = (iterX + 1) * 16 - burstDX; burstY = y - (burstDY - dy) / 2;
                    //разрушение кирпича
                    if (arr[iterY, iterX] == 4) arr[iterY, iterX] = 0;
                    if (arr[iterY + 1, iterX] == 4) arr[iterY + 1, iterX] = 0;
                }
                else if ((x + 8) % 16 >= 6 && iterX < 39 && (arr[iterY, iterX + 1] >= 4 || arr[iterY + 1, iterX + 1] >= 4)) //если пулька приближается к препятствию (6 - длина клетки минус шаг пульки)
                {
                    checkVar = checkVarEnum.barRight; burstX = (iterX + 1) * 16 - burstDX; burstY = y - (burstDY - dy) / 2;
                    //разрушение кирпича
                    if (arr[iterY, iterX + 1] == 4) { arr[iterY, iterX + 1] = 0; burstX = (iterX + 2) * 16 - burstDX; burstY = y - (burstDY - dy) / 2; }
                    if (arr[iterY + 1, iterX + 1] == 4) { arr[iterY + 1, iterX + 1] = 0; burstX = (iterX + 2) * 16 - burstDX; burstY = y - (burstDY - dy) / 2; }
                }
            }
            #endregion

            #region движение вверх

            if (dir == moveDirectionEnum.Up)
            {
                if (iterY >= 0 && (arr[iterY, iterX] >= 4 || arr[iterY, iterX + 1] >= 4)) //если пулька сразу находится внутри препятствия
                {
                    checkVar = checkVarEnum.barUp; burstX = x - (burstDX - dx) / 2; burstY = iterY * 16;
                    //разрушение кирпича
                    if (arr[iterY, iterX] == 4) arr[iterY, iterX] = 0;
                    if (arr[iterY, iterX + 1] == 4) arr[iterY, iterX + 1] = 0;
                }
                else if (y % 16 <= 10 && iterY > 0 && (arr[iterY - 1, iterX] >= 4 || arr[iterY - 1, iterX + 1] >= 4)) //если пулька приближается к препятствию (10 - шаг пульки)
                {
                    checkVar = checkVarEnum.barUp; burstX = x - (burstDX - dx) / 2; burstY = iterY * 16;
                    //разрушение кирпича
                    if (arr[iterY - 1, iterX] == 4) { arr[iterY - 1, iterX] = 0; burstX = x - (burstDX - dx) / 2; burstY = (iterY - 1) * 16; }
                    if (arr[iterY - 1, iterX + 1] == 4) { arr[iterY - 1, iterX + 1] = 0; burstX = x - (burstDX - dx) / 2; burstY = (iterY - 1) * 16; }
                }
            }
            #endregion

            #region движение вниз
            if (dir == moveDirectionEnum.Down)
            {
                iterY = Convert.ToInt32(Math.Floor((double)(y + 8) / 16));
                if (iterY <= 39 && (arr[iterY, iterX] >= 4 || arr[iterY, iterX + 1] >= 4)) //если пулька сразу находится внутри препятствия
                {
                    checkVar = checkVarEnum.barUp; burstX = x - (burstDX - dx) / 2; burstY = (iterY + 1) * 16 - burstDY;
                    //разрушение кирпича
                    if (arr[iterY, iterX] == 4) arr[iterY, iterX] = 0;
                    if (arr[iterY, iterX + 1] == 4) arr[iterY, iterX + 1] = 0;
                }
                else if ((y + 8) % 16 >= 6 && iterY < 39 && (arr[iterY + 1, iterX] >= 4 || arr[iterY + 1, iterX + 1] >= 4)) //если пулька приближается к препятствию (6 - длина клетки минус шаг пульки)
                {
                    checkVar = checkVarEnum.barDown; burstX = x - (burstDX - dx) / 2; burstY = (iterY + 1) * 16 - burstDY;
                    //разрушение кирпича
                    if (arr[iterY + 1, iterX] == 4) { arr[iterY + 1, iterX] = 0; burstX = x - (burstDX - dx) / 2; burstY = (iterY + 2) * 16 - burstDY; }
                    if (arr[iterY + 1, iterX + 1] == 4) { arr[iterY + 1, iterX + 1] = 0; burstX = x - (burstDX - dx) / 2; burstY = (iterY + 2) * 16 - burstDY; }
                }
            }
            #endregion
        }

        public void MoveBullet() //движение пульки
        {
            //новые координаты, если нет препятствия
            if (checkVar == checkVarEnum.barNo)
            {
                if (dir == moveDirectionEnum.Left) x -= 10;
                if (dir == moveDirectionEnum.Right) x += 10;
                if (dir == moveDirectionEnum.Up) y -= 10;
                if (dir == moveDirectionEnum.Down) y += 10;
            }

            //проверка на наличие препятствий
            if (checkVar != checkVarEnum.barNo) { removed = true; }

            if ((x <= -8) || (x >= 640) || (y <= -8) || (y >= 640)) removed = true; //удаление при вылете за границы карты
        }
    }
}