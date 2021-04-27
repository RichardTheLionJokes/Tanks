using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Tanks
{
    abstract class Tank
    {
        public static int[] TankQuantityArr = new int[] { 10, 5, 5, 5, 1 };
        public static int TankIter = 0;
        public static int FirstTankIter = 0;

        public int x; //координаты танка
        public int y;
        public int dx; //размеры танка
        public int dy;
        public byte Speed; //скорость движения танка
        public byte hp; //количество жизней
        public bool ice; //находится ли танк на льду
        public moveDirectionEnum dir; //направление
        public bool changeDir; //направление изменено
        moveDirectionEnum previousDir; //предыдущее направление
        public tankTeam team; //вид танка
        public bool removed; //нужно ли удалить танк
        internal checkVarEnum checkVar; //расположение препятствия
        public Bitmap TBIT; //картинка танка
        
        public static int TankQuantity() //количество оставшихся вражеских танков
        {
            int result = 0;
            for (int i = 0; i < TankQuantityArr.Length; i++)
            {
                result += TankQuantityArr[i];
            }
            return result;
        }

        public Tank(int x, int y, int dx, int dy, moveDirectionEnum dir, tankTeam team)
        {
            this.x = x;
            this.y = y;
            this.dx = dx;
            this.dy = dy;
            ice = false;
            this.dir = dir;
            changeDir = false;
            previousDir = dir;
            this.team = team;
            removed = false;
            checkVar = checkVarEnum.barNo;
        }

        public void CheckMap(int[,] arr, List<EnemyTank> enemyList, MyTank myTank) //проверка карты на наличие препятствий
        {
            checkVar = checkVarEnum.barNo;
            int iterX = x / 16;
            int iterY = y / 16;
            
            switch (dir)
            {
                #region движение влево
                case moveDirectionEnum.Left:
                    foreach (Tank enemy in enemyList)  //проверка пересечения с вражескими танками
                    {
                        if (enemy.x + dx > x - Speed && enemy.x + dx <= x && enemy.y < y + dy && enemy.y > y - dy)
                        {
                            checkVar = checkVarEnum.barLeft;
                            break;
                        }    
                    }
                    if (checkVar != checkVarEnum.barNo) break;
                    //проверка пересечения с моим танком
                    if (myTank.x + dx >= x - Speed && myTank.x + dx <= x && myTank.y < y + dy && myTank.y > y - dy) 
                    {
                        checkVar = checkVarEnum.barLeft;
                        break;
                    }

                    if (x % 16 == 0) //если танк подъехал к краю клетки
                    {
                        if (x <= 0) { checkVar = checkVarEnum.barLeft; } //если танк подъехал к краю карты
                        else if (y / 16.0 - iterY > iterY + 1 - y / 16.0) //проверка двух нижних клеток
                        {
                            if ((arr[iterY + 1, x / 16 - 1] >= 3) || (arr[iterY + 2, x / 16 - 1] >= 3)) checkVar = checkVarEnum.barLeft;
                            if (arr[iterY + 1, x / 16 - 1] == 1 || arr[iterY + 2, x / 16 - 1] == 1) ice = true; //заезд на лед
                            if (ice == true && arr[iterY + 1, x / 16 - 1] != 1 && arr[iterY + 2, x / 16 - 1] != 1) ice = false; //съезд со льда
                        }
                        else if (y / 16.0 - iterY <= iterY + 1 - y / 16.0) //проверка двух верхних клеток
                        {
                            if ((arr[iterY, x / 16 - 1] >= 3) || (arr[iterY + 1, x / 16 - 1] >= 3)) checkVar = checkVarEnum.barLeft;
                            if (arr[iterY, x / 16 - 1] == 1 || arr[iterY + 1, x / 16 - 1] == 1) ice = true; //заезд на лед
                            if (ice == true && arr[iterY, x / 16 - 1] != 1 && arr[iterY + 1, x / 16 - 1] != 1) ice = false; //съезд со льда
                        }
                    }    
                    break;
                #endregion

                #region движение вправо
                case moveDirectionEnum.Right:
                    foreach (Tank enemy in enemyList)  //проверка пересечения с вражескими танками
                    {
                        if (enemy.x < x + dx + Speed && enemy.x >= x + dx && enemy.y < y + dy && enemy.y > y - dy)
                        {
                            checkVar = checkVarEnum.barRight;
                            break;
                        }
                    }
                    if (checkVar != checkVarEnum.barNo) break;
                    //проверка пересечения с моим танком
                    if (myTank.x <= x + dx + Speed && myTank.x >= x + dx && myTank.y < y + dy && myTank.y > y - dy)
                    {
                        checkVar = checkVarEnum.barRight;
                        break;
                    }

                    if (x % 16 == 0) //если танк подъехал к краю клетки
                    {
                        if (x >= 640 - dx) { checkVar = checkVarEnum.barRight; } //если танк подъехал к краю карты
                        else if (y / 16.0 - iterY > iterY + 1 - y / 16.0) //проверка двух нижних клеток
                        {
                            if ((arr[iterY + 1, x / 16 + 2] >= 3) || (arr[iterY + 2, x / 16 + 2] >= 3)) checkVar = checkVarEnum.barRight;
                            if (arr[iterY + 1, x / 16 + 2] == 1 || arr[iterY + 2, x / 16 + 2] == 1) ice = true; //заезд на лед
                            if (ice == true && arr[iterY + 1, x / 16 + 2] != 1 && arr[iterY + 2, x / 16 + 2] != 1) ice = false; //съезд со льда
                        }
                        else if (y / 16.0 - iterY <= iterY + 1 - y / 16.0) //проверка двух верхних клеток
                        {
                            if ((arr[iterY, x / 16 + 2] >= 3) || (arr[iterY + 1, x / 16 + 2] >= 3)) checkVar = checkVarEnum.barRight;
                            if (arr[iterY, x / 16 + 2] == 1 || arr[iterY + 1, x / 16 + 2] == 1) ice = true; //заезд на лед
                            if (ice == true && arr[iterY, x / 16 + 2] != 1 && arr[iterY + 1, x / 16 + 2] != 1) ice = false; //съезд со льда
                        }
                    }
                    break;
                #endregion

                #region движение вверх
                case moveDirectionEnum.Up:
                    foreach (Tank enemy in enemyList)  //проверка пересечения с вражескими танками
                    {
                        if ((enemy.y + dy > y - Speed && enemy.y + dy <= y && enemy.x < x + dx && enemy.x > x - dx)
                            || (enemy.x + 32 == x && enemy.y - 5 <= y && enemy.y + 5 >= y))
                        {
                            //x = (x / 16 + 1) * 16;
                            checkVar = checkVarEnum.barUp;
                            break;
                        }
                    }
                    if (checkVar != checkVarEnum.barNo) break;
                    //проверка пересечения с моим танком
                    if (myTank.y + dy >= y - Speed && myTank.y + dy <= y && myTank.x < x + dx && myTank.x > x - dx)
                    {
                        checkVar = checkVarEnum.barUp;
                        break;
                    }

                    if (y % 16 == 0) //если танк подъехал к краю клетки
                    {
                        if (y <= 0) { checkVar = checkVarEnum.barUp; } //если танк подъехал к краю карты
                        else if (x / 16.0 - iterX > iterX + 1 - x / 16.0) //проверка двух правых клеток
                        {
                            if ((arr[y / 16 - 1, iterX + 1] >= 3) || (arr[y / 16 - 1, iterX + 2] >= 3)) checkVar = checkVarEnum.barUp;
                            if (arr[y / 16 - 1, iterX + 1] == 1 || arr[y / 16 - 1, iterX + 2] == 1) ice = true; //заезд на лед
                            if (ice == true && arr[y / 16 - 1, iterX + 1] != 1 && arr[y / 16 - 1, iterX + 2] != 1) ice = false; //съезд со льда
                        }
                        else if (x / 16.0 - iterX <= iterX + 1 - x / 16.0) //проверка двух левых клеток
                        {
                            if ((arr[y / 16 - 1, iterX] >= 3) || (arr[y / 16 - 1, iterX + 1] >= 3)) checkVar = checkVarEnum.barUp;
                            if (arr[y / 16 - 1, iterX] == 1 || arr[y / 16 - 1, iterX + 1] == 1) ice = true; //заезд на лед
                            if (ice == true && arr[y / 16 - 1, iterX] != 1 && arr[y / 16 - 1, iterX + 1] != 1) ice = false; //съезд со льда
                        }
                    }
                    break;
                #endregion

                #region движение вниз
                case moveDirectionEnum.Down:
                    foreach (Tank enemy in enemyList)  //проверка пересечения с вражескими танками
                    {
                        if (enemy.y < y + dy + Speed && enemy.y >= y + dy && enemy.x < x + dx && enemy.x > x - dx)
                        {
                            checkVar = checkVarEnum.barDown;
                            break;
                        }
                    }
                    if (checkVar != checkVarEnum.barNo) break;
                    //проверка пересечения с моим танком
                    if (myTank.y <= y + dy + Speed && myTank.y >= y + dy && myTank.x < x + dx && myTank.x > x - dx)
                    {
                        checkVar = checkVarEnum.barDown;
                        break;
                    }

                    if (y % 16 == 0) //если танк подъехал к краю клетки
                    {
                        if (y >= 640 - dy) { checkVar = checkVarEnum.barDown; } //если танк подъехал к краю карты
                        else if (x / 16.0 - iterX > iterX + 1 - x / 16.0) //проверка двух правых клеток
                        {
                            if ((arr[y / 16 + 2, iterX + 1] >= 3) || (arr[y / 16 + 2, iterX + 2] >= 3)) checkVar = checkVarEnum.barDown;
                            if (arr[y / 16 + 2, iterX + 1] == 1 || arr[y / 16 + 2, iterX + 2] == 1) ice = true; //заезд на лед
                            if (ice == true && arr[y / 16 + 2, iterX + 1] != 1 && arr[y / 16 + 2, iterX + 2] != 1) ice = false; //съезд со льда
                        }
                        else if (x / 16.0 - iterX <= iterX + 1 - x / 16.0) //проверка двух левых клеток
                        {
                            if ((arr[y / 16 + 2, iterX] >= 3) || (arr[y / 16 + 2, iterX + 1] >= 3)) checkVar = checkVarEnum.barDown;
                            if (arr[y / 16 + 2, iterX] == 1 || arr[y / 16 + 2, iterX + 1] == 1) ice = true; //заезд на лед
                            if (ice == true && arr[y / 16 + 2, iterX] != 1 && arr[y / 16 + 2, iterX + 1] != 1) ice = false; //съезд со льда
                        }
                    }
                    break;
                #endregion
            }
        }

        public virtual void MoveTank() //движение танка
        {
            int iterX = x / 16;
            int iterY = y / 16;

            #region выравнивание по сетке (нужно, чтобы вписываться в повороты) и расчет новых координат
            if (dir == moveDirectionEnum.Left)
            {
                if (y % 16 != 0)
                {
                    if (y / 16.0 - iterY > iterY + 1 - y / 16.0) { y = (iterY + 1) * 16; }
                    else y = iterY * 16;
                }
                x -= Speed;
            }
            if (dir == moveDirectionEnum.Right)
            {
                if (y % 16 != 0)
                {
                    if (y / 16.0 - iterY > iterY + 1 - y / 16.0) { y = (iterY + 1) * 16; }
                    else y = iterY * 16;
                }
                x += Speed;
            }
            if (dir == moveDirectionEnum.Up)
            {
                if (x % 16 != 0)
                {
                    if (x / 16.0 - iterX > iterX + 1 - x / 16.0) { x = (iterX + 1) * 16; }
                    else x = iterX * 16;
                }
                y -= Speed;
            }
            if (dir == moveDirectionEnum.Down)
            {
                if (x % 16 != 0)
                {
                    if (x / 16.0 - iterX > iterX + 1 - x / 16.0) { x = (iterX + 1) * 16; }
                    else x = iterX * 16;
                }
                y += Speed;
            }
            #endregion

            //проверка на наличие препятствий
            if (checkVar == checkVarEnum.barLeft) x += Speed;
            if (checkVar == checkVarEnum.barRight) x -= Speed;
            if (checkVar == checkVarEnum.barUp) y += Speed;
            if (checkVar == checkVarEnum.barDown) y -= Speed;
            
            #region выбор картинки
            if (changeDir == true)
            {
                switch (dir)
                {
                    //картинка поворачиватся с учетом предыдущего положения
                    case moveDirectionEnum.Left:
                        switch (previousDir)
                        {
                            case moveDirectionEnum.Right:
                                TBIT.RotateFlip(RotateFlipType.RotateNoneFlipX);
                                break;
                            case moveDirectionEnum.Up:
                                TBIT.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                break;
                            case moveDirectionEnum.Down:
                                TBIT.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                break;
                        }
                        break;
                    case moveDirectionEnum.Right:
                        switch (previousDir)
                        {
                            case moveDirectionEnum.Left:
                                TBIT.RotateFlip(RotateFlipType.RotateNoneFlipX);
                                break;
                            case moveDirectionEnum.Up:
                                TBIT.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                break;
                            case moveDirectionEnum.Down:
                                TBIT.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                break;
                        }
                        break;
                    case moveDirectionEnum.Up:
                        switch (previousDir)
                        {
                            case moveDirectionEnum.Left:
                                TBIT.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                break;
                            case moveDirectionEnum.Right:
                                TBIT.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                break;
                            case moveDirectionEnum.Down:
                                TBIT.RotateFlip(RotateFlipType.RotateNoneFlipY);
                                break;
                        }
                        break;
                    case moveDirectionEnum.Down:
                        switch (previousDir)
                        {
                            case moveDirectionEnum.Left:
                                TBIT.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                break;
                            case moveDirectionEnum.Right:
                                TBIT.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                break;
                            case moveDirectionEnum.Up:
                                TBIT.RotateFlip(RotateFlipType.RotateNoneFlipY);
                                break;
                        }
                        break;
                }
                changeDir = false;
                previousDir = dir;
            }
            #endregion
        }                
    }

    class MyTank : Tank
    {
        Bitmap MyTankBit = new Bitmap(@"Pictures/myTank.png"); //мой танк
        //Bitmap MyTankBit = new Bitmap(@"Pictures/Dick.png"); //мой танк 18+
        public bool shot; //может ли танк стрелять
        public bool immob; //может ли свой танк двигаться

        public MyTank(int x, int y, int dx, int dy, moveDirectionEnum dir, tankTeam team)
            : base(x, y, dx, dy, dir, team)
        {
            Speed = 2;
            hp = 3;
            shot = true;
            TBIT = MyTankBit;
        }
    }

    class EnemyTank : Tank
    {
        Bitmap enemyNormBit = new Bitmap(@"Pictures/enemy_norm.png"); //обычный вражеский танк
        Bitmap enemyRapBit = new Bitmap(@"Pictures/enemy_rap.png"); //быстрый вражеский танк
        Bitmap enemyArmBit = new Bitmap(@"Pictures/enemy_arm.png"); //вражеский танк с 3-мя жизнями
        Bitmap enemyRFBit = new Bitmap(@"Pictures/enemy_rf.png"); //быстро стреляющий вражеский танк
        Bitmap enemyImbBit = new Bitmap(@"Pictures/enemy_imba.png"); //быстрый вражеский танк
        byte iterChDir; //счетчик смены направления для вражеских танков
        int shootIter; //счетчик для стрельбы вражеских танков
        int shootIterIncr; //рандомное увеличение счетчика для стрельбы вражеских танков
        int shootInterval; //интервал между выстрелами вражеских танков

        public EnemyTank(int x, int y, int dx, int dy, moveDirectionEnum dir, tankTeam team)
            : base(x, y, dx, dy, dir, team)
        {
            if (team == tankTeam.enemy_rap || team == tankTeam.enemy_imba) Speed = 4; else Speed = 2;
            if (team == tankTeam.enemy_arm || team == tankTeam.enemy_imba) hp = 3; else hp = 1;
            if (team == tankTeam.enemy_rf || team == tankTeam.enemy_imba) shootInterval = 15; else shootInterval = 40;
            shootIter = 0;
            iterChDir = 0;
            #region выбор картинки
            switch (team)
            {
                case tankTeam.enemy_norm:
                    TBIT = enemyNormBit;
                    TBIT.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    break;
                case tankTeam.enemy_rap:
                    TBIT = enemyRapBit;
                    TBIT.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    break;
                case tankTeam.enemy_arm:
                    TBIT = enemyArmBit;
                    TBIT.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    break;
                case tankTeam.enemy_rf:
                    TBIT = enemyRFBit;
                    TBIT.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    break;
                case tankTeam.enemy_imba:
                    TBIT = enemyImbBit;
                    TBIT.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    break;
            }
            #endregion
        }

        public override void MoveTank() //движение танка
        {
            base.MoveTank();

            #region рандомное движение вражеских танков (полностью переделать)
            //сделано как попало, чтобы хоть как-то ездили
            //генерация чисел в рандомайзере тоже как попало, числа взяты первые, которые пришли в голову
            if (team != tankTeam.my)
            {
                //int ChickenX = 320; //координаты центра курицы
                //int ChickenY = 640 - 48;
                if (iterChDir == 50)
                {
                    //Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                    //int dirTemp = rand.Next(26);
                    ////танк с большей вероятностью повернет в сторону курицы
                    //if (Y < ChickenY + 16) //здесь 16 - разница по высоте между танком и курицей
                    //{
                    //    if (X + 16 < ChickenX)
                    //    {
                    //        if (dirTemp < 5) { DIR = moveDirectionEnum.Left; }
                    //        if (dirTemp >= 5 && dirTemp < 13) { DIR = moveDirectionEnum.Right; }
                    //        if (dirTemp >= 13 && dirTemp < 18) { DIR = moveDirectionEnum.Up; }
                    //        if (dirTemp >= 18 && dirTemp < 26) { DIR = moveDirectionEnum.Down; }
                    //    }
                    //    if (X + 16 > ChickenX)
                    //    {
                    //        if (dirTemp < 8) { DIR = moveDirectionEnum.Left; }
                    //        if (dirTemp >= 8 && dirTemp < 13) { DIR = moveDirectionEnum.Right; }
                    //        if (dirTemp >= 13 && dirTemp < 18) { DIR = moveDirectionEnum.Up; }
                    //        if (dirTemp >= 18 && dirTemp < 26) { DIR = moveDirectionEnum.Down; }
                    //    }
                    //}
                    //else
                    //{
                    //    if (X + 16 < ChickenX)
                    //    {
                    //        if (dirTemp < 3) { DIR = moveDirectionEnum.Left; }
                    //        if (dirTemp >= 3 && dirTemp < 13) { DIR = moveDirectionEnum.Right; }
                    //        if (dirTemp >= 13 && dirTemp < 16) { DIR = moveDirectionEnum.Up; }
                    //    }
                    //    if (X + 16 > ChickenX)
                    //    {
                    //        if (dirTemp < 8) { DIR = moveDirectionEnum.Left; }
                    //        if (dirTemp >= 8 && dirTemp < 13) { DIR = moveDirectionEnum.Right; }
                    //        if (dirTemp >= 13 && dirTemp < 18) { DIR = moveDirectionEnum.Up; }
                    //    }
                    //}
                    ChangeDir();
                    //IterChDir = 0;
                    //changeDir = true;
                }
                //смена направления если впереди препятствие
                if (checkVar == checkVarEnum.barLeft || checkVar == checkVarEnum.barRight || checkVar == checkVarEnum.barUp || checkVar == checkVarEnum.barDown)
                {
                    //Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF); int dirTemp = Convert.ToInt32(Math.Floor((double)rand.Next(4000) / 1000));
                    //Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF); int dirTemp = rand.Next(4);
                    //if (dirTemp == 0) { DIR = moveDirectionEnum.Left; }
                    //if (dirTemp == 1) { DIR = moveDirectionEnum.Right; }
                    //if (dirTemp == 2) { DIR = moveDirectionEnum.Up; }
                    //if (dirTemp == 3) { DIR = moveDirectionEnum.Down; }
                    ChangeDir();
                    //IterChDir = 0;
                    //changeDir = true;
                }
                iterChDir++;
            }
            #endregion
        }

        private void ChangeDir() //рандомная смена направления вражеским танком
        {
            int ChickenX = 320; //координаты центра курицы
            int ChickenY = 640 - 48;
            double maxdist = Math.Sqrt(Math.Pow(608, 2) + Math.Pow(304, 2)); //максимальное расстояние между танком и курицей
            double distance = Math.Sqrt(Math.Pow(320 - (x + 16), 2) + Math.Pow(640 - (y + 32), 2));
            //int largerChance = (int)Math.Sqrt(maxdist / distance);
            int largerChance = (int)(maxdist / distance);
            Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            int dirTemp = rand.Next(2 * largerChance + 2);
            //танк с большей вероятностью повернет в сторону курицы
            if (y < ChickenY + 16) //здесь 16 - разница по высоте между танком и курицей
            {
                if (x + 16 < ChickenX)
                {
                    if (dirTemp == 0) { dir = moveDirectionEnum.Left; }
                    if (dirTemp > 0 && dirTemp <= largerChance) { dir = moveDirectionEnum.Right; }
                    if (dirTemp == largerChance + 1) { dir = moveDirectionEnum.Up; }
                    if (dirTemp > largerChance + 1) { dir = moveDirectionEnum.Down; }
                }
                else if (x + 16 > ChickenX)
                {
                    if (dirTemp < largerChance) { dir = moveDirectionEnum.Left; }
                    if (dirTemp == largerChance) { dir = moveDirectionEnum.Right; }
                    if (dirTemp == largerChance + 1) { dir = moveDirectionEnum.Up; }
                    if (dirTemp > largerChance + 1) { dir = moveDirectionEnum.Down; }
                }
                else
                {
                    if (dirTemp < largerChance) { dir = moveDirectionEnum.Left; }
                    if (dirTemp >= largerChance && dirTemp <= largerChance + 1) { dir = moveDirectionEnum.Right; }
                    if (dirTemp > largerChance + 1) { dir = moveDirectionEnum.Up; }
                }
            }
            else
            {
                if (x + 16 < ChickenX)
                {
                    if (dirTemp == 0) { dir = moveDirectionEnum.Left; }
                    if (dirTemp > 0 && dirTemp <= 2 * largerChance) { dir = moveDirectionEnum.Right; }
                    if (dirTemp > 2 * largerChance) { dir = moveDirectionEnum.Up; }
                }
                else if (x + 16 > ChickenX)
                {
                    if (dirTemp < 2 * largerChance) { dir = moveDirectionEnum.Left; }
                    if (dirTemp == 2 * largerChance) { dir = moveDirectionEnum.Right; }
                    if (dirTemp > 2 * largerChance) { dir = moveDirectionEnum.Up; }
                }
            }
            iterChDir = 0;
            changeDir = true;
        }

        public bool Shooting(List<Bullet> BulList) //стрельба вражеских танков
        {
            if (shootIter == shootInterval)
            {
                //увеличение интервала стрельбы на случайную величину
                //чтобы у танка каждый раз был разный интервал между выстрелами
                Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                shootIterIncr = rand.Next(1, shootInterval + 1);
                shootIter++;
                return false;
            }
            else if (shootIter > shootInterval)
            {
                if (shootIter == (shootInterval + shootIterIncr)) //выстрел
                {
                    int locX = 0, locY = 0;
                    if (dir == moveDirectionEnum.Left) { locX = x - 8; locY = y + dy / 2 - 4; }
                    if (dir == moveDirectionEnum.Right) { locX = x + dx + 0; locY = y + dy / 2 - 4; }
                    if (dir == moveDirectionEnum.Up) { locX = x + dx / 2 - 4; locY = y - 8; }
                    if (dir == moveDirectionEnum.Down) { locX = x + dx / 2 - 4; locY = y + dy + 0; }
                    BulList.Add(new Bullet(locX, locY, 8, 8, dir)); //создание пульки и добавление ее в массив
                    shootIter = 0;
                    return true; //выстрел произведен
                }
                else shootIter++;
                return false;
            }
            else shootIter++;
            return false;
        }
    }
}