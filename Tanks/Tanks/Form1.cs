using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices; //для DllImport;

namespace Tanks
{
    public enum moveDirectionEnum //направление танка
    {
        Left,
        Right,
        Up,
        Down
    }
    public enum tankTeam //тип танка
    {
        my, //мой
        enemy_norm, //обычный
        enemy_rap, //rapid
        enemy_arm, //armored
        enemy_rf, //rapid fire
        enemy_imba //king
    }
    public enum checkVarEnum //расположение препятствия
    {
        barNo,
        barLeft,
        barRight,
        barUp,
        barDown
    }
    public partial class Form1 : Form
    {
        #region определение переменных
        Timer renderTimer = new Timer(); //таймер для обновления картинки и сборки мусора
        Timer moveTankTimer = new Timer(); //таймер для передвижения своего танка
        Timer moveEnemyTimer = new Timer(); //таймер для передвижения и стрельбы вражеских танков
        Timer moveBulTimer = new Timer(); //таймер для передвижения пулек
        bool Sound; //звук
        bool[] moveDirection = new bool[] { false, false, false, false }; //состояния движения своего танка
        bool skidding; //скольжение своего танка
        byte skidIter;        
        MyTank myTank; //мой танк
        List<Bullet> bulList = new List<Bullet>(); //список пулек
        List<EnemyTank> enemyList = new List<EnemyTank>(); //список вражеских танков
        List<Bullet> enemyBulList = new List<Bullet>(); //список вражеских пулек
        List<Burst> BurstList = new List<Burst>(); //список взрывов
        List<Respawn> RespawnList = new List<Respawn>(); //список респаунов
        Respawn myResp; //мой респаун
 
        int[,] mapArr = new int[40, 40]; //массив карты
        //int[,] tankCoordArr; //массив координат вражеских танков
        readonly int[,] respawnArr = new int[,] { { 0, 0 }, { 12, 0 }, { 24, 0 }, { 38, 0 } }; //точки респауна вражеских танков
        Bitmap Chicken; //курица
        Bitmap GameOver; //конец игры
        bool GO; //игра окончена
        int crMapNumb; //идентификатор спрайта для режима рисования
        bool drawMode; //режим рисования
        
        //для воспроизведения звуков
        [DllImport("winmm.dll")]
        private static extern uint mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);
                
        //графические компоненты
        public Graphics fieldGraf;
        public BufferedGraphicsContext bufGrafCont;
        public BufferedGraphics bufGraf;
        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            #region присвоение значений переменным
            renderTimer.Tick += new EventHandler(renderTimer_Tick); //таймер для обновления картинки и сборки мусора
            renderTimer.Interval = 10;
            moveTankTimer.Tick += new EventHandler(moveTankTimer_Tick); //таймер для передвижения своего танка
            moveTankTimer.Interval = 10;
            moveEnemyTimer.Tick += new EventHandler(moveEnemyTimer_Tick); //таймер для передвижения и стрельбы вражеских танков
            moveEnemyTimer.Interval = 30;
            moveBulTimer.Tick += new EventHandler(moveBulTimer_Tick); //таймер для передвижения пулек
            moveBulTimer.Interval = 30;
            Sound = true; //включение звука
            skidding = false; //скольжение своего танка
            skidIter = 0;
 
            //считывание массива карты из файла
            Maps.ReadMap(mapArr, Application.StartupPath + @"\Maps\Map2.tmap");

            Chicken = new Bitmap(@"Pictures/chicken.png"); //курица
            GameOver = new Bitmap(@"Pictures/game_over.png"); //конец игры

            //загрузка звуков для обычной игры
            mciSendString("open Sounds//shoot.wav alias soundShoot", null, 0, IntPtr.Zero); //выстрел
            mciSendString("open Sounds//kill.wav alias soundKill", null, 0, IntPtr.Zero); //убийство танка
            mciSendString("open Sounds//wall.wav alias soundWall", null, 0, IntPtr.Zero); //попадание в стену
            mciSendString("open Sounds//back.mid type sequencer alias soundBack", null, 0, IntPtr.Zero); //фон

            //загрузка звуков для игры 18+
            //mciSendString("open Sounds//drop.wav alias soundShoot", null, 0, IntPtr.Zero); //выстрел
            //mciSendString("open Sounds//splash.wav alias soundKill", null, 0, IntPtr.Zero); //убийство танка
            //mciSendString("open Sounds//walldrop.wav alias soundWall", null, 0, IntPtr.Zero); //попадание в стену
            //mciSendString("open Sounds//lozt.mid type sequencer alias soundBack", null, 0, IntPtr.Zero); //фон

            mciSendString("open Sounds//gameover.wav alias gameOver", null, 0, IntPtr.Zero); //конец игры

            //графические компоненты
            fieldGraf = Battleground.CreateGraphics();
            bufGrafCont = new BufferedGraphicsContext();
            bufGraf = bufGrafCont.Allocate(fieldGraf, new Rectangle(0, 0, Battleground.Width, Battleground.Height));
            #endregion
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            #region начало игры, отрисовка карты
            if (e.KeyCode == Keys.Space)
            {
                GO = false;
                //создание своего танка
                myTank = new MyTank(Battleground.Width * 14 / 40, Battleground.Height - 32 - 2, 32, 32, moveDirectionEnum.Up, tankTeam.my)
                {
                    immob = true //танк неподвижен первые несколько секунд
                };
                myResp = new Respawn(tankTeam.my, myTank.x - 4, myTank.y - 8, 40, 40, 0, 2); //создание респауна своего танка
                label2.Text = myTank.hp.ToString(); //жизни моего танка
                label1.Text = Tank.TankQuantity().ToString(); //количество оставшихся вражеских танков
                //включение таймеров
                renderTimer.Enabled = true;
                moveTankTimer.Enabled = true;
                moveEnemyTimer.Enabled = true;
                moveBulTimer.Enabled = true;
                //включение фоновой музыки
                if (Sound == true) mciSendString("play soundBack from 0", null, 0, IntPtr.Zero);
            }
            #endregion
            
            #region стрельба
            if (e.KeyCode == Keys.Enter)
            {
                if (myTank.shot == true && myTank.immob == false)
                {
                    //расчет начальных координат пульки в зависимости от положения танка
                    int locX = 0, locY = 0;
                    if (myTank.dir == moveDirectionEnum.Left) { locX = myTank.x - 8; locY = myTank.y + myTank.dy / 2 - 4; }
                    if (myTank.dir == moveDirectionEnum.Right) { locX = myTank.x + myTank.dx + 0; locY = myTank.y + myTank.dy / 2 - 4; }
                    if (myTank.dir == moveDirectionEnum.Up) { locX = myTank.x + myTank.dx / 2 - 4; locY = myTank.y - 8; }
                    if (myTank.dir == moveDirectionEnum.Down) { locX = myTank.x + myTank.dx / 2 - 4; locY = myTank.y + myTank.dy + 0; }
                    //создание новой пульки и добавление ее в массив
                    bulList.Add(new Bullet(locX, locY, 8, 8, myTank.dir));
                    //звук выстрела
                    if (Sound == true) mciSendString("play soundShoot from 0", null, 0, IntPtr.Zero);
                    myTank.shot = false; //чтобы мой танк не мог стрелять очередью, при зажатии кнопки
                }
            }
            #endregion

            #region передвижение своего танка
            if (e.KeyCode == Keys.A) //движение своего танка влево
            {
                moveDirection[0] = true; moveDirection[1] = false; moveDirection[2] = false; moveDirection[3] = false;
                if (skidding == true) { skidding = false; skidIter = 0; }; //прекращение проскальзывания на льду, если оно есть
                myTank.dir = moveDirectionEnum.Left;
                if (myTank.changeDir == false) myTank.changeDir = true;
            }
            if (e.KeyCode == Keys.D) //движение своего танка вправо
            {
                moveDirection[0] = false; moveDirection[1] = true; moveDirection[2] = false; moveDirection[3] = false;
                if (skidding == true) { skidding = false; skidIter = 0; }; //прекращение проскальзывания на льду, если оно есть
                myTank.dir = moveDirectionEnum.Right;
                if (myTank.changeDir == false) myTank.changeDir = true;
            }
            if (e.KeyCode == Keys.W) //движение своего танка вверх
            {
                moveDirection[0] = false; moveDirection[1] = false; moveDirection[2] = true; moveDirection[3] = false;
                if (skidding == true) { skidding = false; skidIter = 0; }; //прекращение проскальзывания на льду, если оно есть
                myTank.dir = moveDirectionEnum.Up;
                if (myTank.changeDir == false) myTank.changeDir = true;
            }
            if (e.KeyCode == Keys.S) //движение своего танка вниз
            {
                moveDirection[0] = false; moveDirection[1] = false; moveDirection[2] = false; moveDirection[3] = true;
                if (skidding == true) { skidding = false; skidIter = 0; }; //прекращение проскальзывания на льду, если оно есть
                myTank.dir = moveDirectionEnum.Down;
                if (myTank.changeDir == false) myTank.changeDir = true;
            }
            #endregion

            //включение-отключение звука
            if (e.KeyCode == Keys.M)
            {
                Sound = !Sound;
                if (Sound == true) mciSendString("play soundBack", null, 0, IntPtr.Zero);
                if (Sound == false) mciSendString("pause soundBack", null, 0, IntPtr.Zero);
                moveEnemyTimer.Enabled = false;
            }

            //if (e.KeyCode == Keys.Q)
            //{
            //    bufGraf.Graphics.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, Battleground.Width, Battleground.Height));
            //    bufGraf.Graphics.DrawString("Press Space", new Font("Arial", 32), new SolidBrush(Color.Black), new PointF(0, 0));
            //}
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                myTank.shot = true; //мой танк снова может стрелять
            }
            if (e.KeyCode == Keys.A)
            {
                moveDirection[0] = false;
                if (moveDirection[1] == false && moveDirection[2] == false && moveDirection[3] == false && myTank.ice == true) skidding = true; //проскальзывание, если танк на льду
            }
            if (e.KeyCode == Keys.D)
            {
                moveDirection[1] = false;
                if (moveDirection[0] == false && moveDirection[2] == false && moveDirection[3] == false && myTank.ice == true) skidding = true; //проскальзывание, если танк на льду
            }
            if (e.KeyCode == Keys.W)
            {
                moveDirection[2] = false;
                if (moveDirection[0] == false && moveDirection[1] == false && moveDirection[3] == false && myTank.ice == true) skidding = true; //проскальзывание, если танк на льду
            }
            if (e.KeyCode == Keys.S)
            {
                moveDirection[3] = false;
                if (moveDirection[0] == false && moveDirection[1] == false && moveDirection[2] == false && myTank.ice == true) skidding = true; //проскальзывание, если танк на льду
            }
        }

        private void renderTimer_Tick(object sender, EventArgs e) //таймер для обновления картинки и сборки мусора
        {
            //повтор фоновой музыки после окончания. плохой способ, нужно сделать по-другому
            //StringBuilder statusStr = new StringBuilder();
            //mciSendString("status soundBack mode", statusStr, 100, IntPtr.Zero);
            //string xx = statusStr.ToString();
            //if (statusStr.ToString() == "stopped")
            //{
            //    mciSendString("seek soundBack to start", null, 0, IntPtr.Zero);
            //    mciSendString("play soundBack", null, 0, IntPtr.Zero);
            //}

            bufGraf.Graphics.DrawImage(Maps.MakeMap(mapArr), 0, 0); //рисование карты

            foreach (Tank enemy in enemyList) //рисование вражеских танков
            {
                bufGraf.Graphics.DrawImage(enemy.TBIT, enemy.x, enemy.y, enemy.dx, enemy.dy);
            }
            if (myTank.immob == false) //если свой танк может двигаться
            {
                bufGraf.Graphics.DrawImage(myTank.TBIT, myTank.x, myTank.y, myTank.dx, myTank.dy); //рисование своего танка
            }

            //создание первых 4-х вражеских танков с небольшим промежутком по времени
            //промежуток нужен для рандомной стрельбы
            if (Tank.FirstTankIter <= 60)
            {
                CreateFirstTanks();
                Tank.TankIter = 0;
                label1.Text = Tank.TankQuantity().ToString();  //количество оставшихся вражеских танков
            }

            //создание вражеских танков
            Tank.TankIter++;
            if (Tank.TankIter == 300 && Tank.TankQuantity() > 0)
            {
                CreateTanks();
                Tank.TankIter = 0;
                label1.Text = Tank.TankQuantity().ToString();  //количество оставшихся вражеских танков
            }

            //рисование курицы
            Rectangle fromRect;
            Rectangle toRect;
            //если игра не окончена, рисуем живую курицу
            if (GO == false)
            {
                fromRect = new Rectangle(0, 0, Chicken.Width / 2, Chicken.Height);
                toRect = new Rectangle(Battleground.Width * 18 / 40, Battleground.Height - Chicken.Height, Chicken.Width / 2, Chicken.Height);
                bufGraf.Graphics.DrawImage(Chicken, toRect, fromRect, GraphicsUnit.Pixel);
            }
            //если игра не окончена, рисуем жареную курицу
            else
            {
                fromRect = new Rectangle(Chicken.Width / 2, 0, Chicken.Width / 2, Chicken.Height);
                toRect = new Rectangle(Battleground.Width * 18 / 40, Battleground.Height - Chicken.Height, Chicken.Width / 2, Chicken.Height);
                bufGraf.Graphics.DrawImage(Chicken, toRect, fromRect, GraphicsUnit.Pixel);
            }
            
            foreach (Bullet bul in bulList) //рисование своих пулек
            {
                bufGraf.Graphics.DrawImage(bul.BBIT, bul.x, bul.y, bul.dx, bul.dy);
            }
            foreach (Bullet bul in enemyBulList) //рисование вражеских пулек
            {
                bufGraf.Graphics.DrawImage(bul.BBIT, bul.x, bul.y, bul.dx, bul.dy);
            }

            Maps.MakeMap(mapArr, 2, bufGraf.Graphics); //рисование леса поверх танков и пулек

            foreach (Respawn rsp in RespawnList) //рисование респаунов
            {
                rsp.ChangePicture(enemyList);
                GraphicsUnit units = GraphicsUnit.Pixel;
                bufGraf.Graphics.DrawImage(rsp.RBIT, rsp.toRect, rsp.fromRect, units);
            }

            if (myResp.removed == false) //рисование моего респауна
            {
                myResp.ChangePicture(myTank);
                GraphicsUnit units = GraphicsUnit.Pixel;
                bufGraf.Graphics.DrawImage(myResp.RBIT, myResp.toRect, myResp.fromRect, units);
            }

            foreach (Burst bst in BurstList) //рисование взрывов танков
            {
                bst.ChangePicture();
                GraphicsUnit units = GraphicsUnit.Pixel;
                bufGraf.Graphics.DrawImage(bst.BBIT, bst.toRect, bst.fromRect, units);
            }

            //удаление из массивов пулек, вылетевших за границы карты или взорвавшихся
            bulList.RemoveAll(bul => bul.removed == true); //моих
            enemyBulList.RemoveAll(bul => bul.removed == true); //вражеских
            //удаление взрывов
            BurstList.RemoveAll(bst => bst.removed == true);
            //удаление респаунов
            RespawnList.RemoveAll(rsp => rsp.removed == true);

            if (GO == true) //если игра окончена
            {
                bufGraf.Graphics.DrawImage(GameOver, Battleground.Width / 2 - 200,
                    Battleground.Height / 2 - 100, 400, 200); //рисование слов "Game Over"
            }

            bufGraf.Render(); //обновление картинки
            GC.Collect(); //сборка мусора
        }

        private void moveTankTimer_Tick(object sender, EventArgs e) //таймер для передвижения своего танка
        {
            if (myTank.immob == false) //если танк может двигаться
            {
                if ((moveDirection[0] == true) || (moveDirection[1] == true) || (moveDirection[2] == true) || (moveDirection[3] == true))
                {
                    myTank.CheckMap(mapArr, enemyList, myTank);
                    myTank.MoveTank();
                }
                if (skidding == true) //проскальзывание по льду
                {
                    myTank.CheckMap(mapArr, enemyList, myTank);
                    myTank.MoveTank();
                    skidIter += 1;
                    if (skidIter == 30)
                    {
                        skidIter = 0;
                        skidding = false;
                    }
                }
            }
        }

        private void moveBulTimer_Tick(object sender, EventArgs e) //таймер для передвижения пулек
        {
            //координаты курицы
            int ChickenX0 = Battleground.Width * 18 / 40;
            int ChickenY0 = Battleground.Height - Chicken.Height;

            //передвижение всех пулек из массивов

            #region свои пульки
            foreach (Bullet bul in bulList)
            {
                bul.CheckMap(mapArr);
                bul.MoveBullet();
                //при попадании пульки в препятствие
                if (bul.checkVar != checkVarEnum.barNo)
                {
                    if (Sound == true) mciSendString("play soundWall from 0", null, 0, IntPtr.Zero); //звук попадания в стену
                    BurstList.Add(new Burst(new Rectangle(bul.burstX, bul.burstY, bul.burstDX, bul.burstDY), 1, 1)); //создание нового взрыва и добавление его в массив
                }
                foreach (Tank enemy in enemyList)
                {
                    //при попадании пульки во вражеский танк
                    if (((bul.x + bul.dx) > enemy.x) && (bul.x < (enemy.x + enemy.dx)) && ((bul.y + bul.dy) > enemy.y) && (bul.y < (enemy.y + enemy.dy)))
                    {
                        bul.removed = true; //удаление пульки
                        enemy.hp--; //уменьшение количества жизней
                        if (enemy.hp == 0) //если танк убит
                        {
                            enemy.removed = true; //удаление вражеского танка
                            BurstList.Add(new Burst(new Rectangle(enemy.x, enemy.y, enemy.dx, enemy.dy), 1, 1)); //создание нового взрыва и добавление его в массив
                            if (Sound == true) mciSendString("play soundKill from 0", null, 0, IntPtr.Zero); //звук убийства танка
                        }
                        else //если танк ранен
                        {
                            BurstList.Add(new Burst(new Rectangle(enemy.x, enemy.y, enemy.dx, enemy.dy), 1, 2)); //создание нового кровавого взрыва и добавление его в массив
                            if (Sound == true) mciSendString("play soundWall from 0", null, 0, IntPtr.Zero); //звук ранения танка
                        }
                    }
                }
                
                //при попадании пульки в курицу
                if (((bul.x + bul.dx) > ChickenX0) && (bul.x < (ChickenX0 + Chicken.Width / 2)) && ((bul.y + bul.dy) > ChickenY0) && (bul.y < (ChickenY0 + Chicken.Height)))
                {
                    if (Sound == true) mciSendString("play soundKill from 0", null, 0, IntPtr.Zero); //звук убийства танка
                    bul.removed = true; //удаление пульки
                    BurstList.Add(new Burst(new Rectangle(ChickenX0, ChickenY0, Chicken.Width / 2, Chicken.Height), 1, 1)); //создание нового взрыва и добавление его в массив
                    EndGame(); //конец игры
                }
            }
            #endregion

            #region вражеские пульки
            foreach (Bullet bul in enemyBulList)
            {
                bul.CheckMap(mapArr);
                bul.MoveBullet();
                //при попадании пульки в препятствие
                if (bul.checkVar != checkVarEnum.barNo)
                {
                    if (Sound == true) mciSendString("play soundWall from 0", null, 0, IntPtr.Zero); //звук попадания в стену
                    BurstList.Add(new Burst(new Rectangle(bul.burstX, bul.burstY, bul.burstDX, bul.burstDY), 1, 1)); //создание нового взрыва и добавление его в массив
                }
                //при попадании пульки в свой танк
                if (((bul.x + bul.dx) > myTank.x) && (bul.x < (myTank.x + myTank.dx)) && ((bul.y + bul.dy) > myTank.y) && (bul.y < (myTank.y + myTank.dy)))
                {
                    if (Sound == true) mciSendString("play soundKill from 0", null, 0, IntPtr.Zero); //звук убийства танка
                    bul.removed = true; //удаление пульки
                    BurstList.Add(new Burst(new Rectangle(myTank.x, myTank.y, myTank.dx, myTank.dy), 1, 1)); //создание нового взрыва и добавление его в массив
                    myTank.hp--; //уменьшение количества жизней
                    label2.Text = myTank.hp.ToString();
                    if (myTank.hp == 0) //если мой танк убит
                    {
                        EndGame(); //конец игры
                    }
                    else //если мой танк еще жив
                    {
                        //перемещение в точку респауна
                        myTank.x = Battleground.Width * 14 / 40;
                        myTank.y = Battleground.Height - 32 - 2;
                        myTank.immob = true; //танк неподвижен первые несколько секунд
                        myResp = new Respawn(tankTeam.my, myTank.x - 4, myTank.y - 8, 40, 40, 0, 2); //создание респауна своего танка
                    }
                }

                //при попадании пульки в курицу
                if (((bul.x + bul.dx) > ChickenX0) && (bul.x < (ChickenX0 + Chicken.Width / 2)) && ((bul.y + bul.dy) > ChickenY0) && (bul.y < (ChickenY0 + Chicken.Height)))
                {
                    if (Sound == true) mciSendString("play soundKill from 0", null, 0, IntPtr.Zero); //звук убийства танка
                    bul.removed = true; //удаление пульки
                    BurstList.Add(new Burst(new Rectangle(ChickenX0, ChickenY0, Chicken.Width / 2, Chicken.Height), 1, 1)); //создание нового взрыва и добавление его в массив
                    EndGame(); //конец игры
                }
            }
            #endregion
        }

        private void moveEnemyTimer_Tick(object sender, EventArgs e) //таймер для передвижения и стрельбы вражеских танков
        {
            //int iter = 0; //счетчик для заполнения масссива координат вражеских танков
            //tankCoordArr = new int[enemyList.Count, 2];
            foreach (EnemyTank enemy in enemyList) //передвижение всех вражеских танков из массива
            {
                //tankCoordArr[iter, 0] = enemy.X;
                //tankCoordArr[iter, 1] = enemy.Y;
                //iter++;
                enemy.CheckMap(mapArr, enemyList, myTank);
                enemy.MoveTank();
                if (enemy.Shooting(enemyBulList) == true) //если танк выстрелил
                {
                    if (Sound == true) mciSendString("play soundShoot from 0", null, 0, IntPtr.Zero);
                }
            }

            //удаление из массивов убитых танков
            enemyList.RemoveAll(en => en.removed == true);
        }

        private void CreateTanks() //создание вражеских танков. возможно, стоит перенести в Tank.cs
        {
            Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            int respPoint = rand.Next(4); //рандомная точка респауна
            int randTeam; //рандомный тип танка
            do
            {
                randTeam = rand.Next(5);
            }
            while (Tank.TankQuantityArr[randTeam] == 0); //если танков это типа не осталось, повторяем заново
            Tank.TankQuantityArr[randTeam]--; //уменьшение количества танков выбранного типа
            tankTeam team = (tankTeam)(randTeam + 1); //получение константы из перечисления по значению
            RespawnList.Add(new Respawn(team, respawnArr[respPoint, 0] * 16, respawnArr[respPoint, 1] * 16, 32, 32, 0, 1)); //создание нового респауна и добавление его в массив
        }

        private void CreateFirstTanks() //создание первых 4-х вражеских танков. возможно, стоит перенести в Tank.cs
        {
            switch (Tank.FirstTankIter)
            {
                case 0:
                    RespawnList.Add(new Respawn(tankTeam.enemy_norm, respawnArr[1, 0] * 16, respawnArr[1, 1] * 16, 32, 32, 0, 1));
                    Tank.TankQuantityArr[0]--;
                    break;
                case 20:
                    RespawnList.Add(new Respawn(tankTeam.enemy_norm, respawnArr[2, 0] * 16, respawnArr[2, 1] * 16, 32, 32, 0, 1));
                    Tank.TankQuantityArr[0]--;
                    break;
                case 40:
                    RespawnList.Add(new Respawn(tankTeam.enemy_rap, 0, 0, 32, 32, 0, 1));
                    Tank.TankQuantityArr[1]--;
                    break;
                case 60:
                    RespawnList.Add(new Respawn(tankTeam.enemy_arm, 608, 0, 32, 32, 0, 1));
                    Tank.TankQuantityArr[2]--;
                    break;
            }
            Tank.FirstTankIter++;
        }

        private void EndGame() //конец игры (кончились жизни или убита курица)
        {
            //выключение таймеров
            //renderTimer.Enabled = false;
            moveTankTimer.Enabled = false;
            moveEnemyTimer.Enabled = false;
            moveBulTimer.Enabled = false;
            mciSendString("stop soundBack", null, 0, IntPtr.Zero); //выключение фоновой музыки
            if (Sound == true) mciSendString("play gameOver from 0", null, 0, IntPtr.Zero); //музыка game over
            GO = true;
        }

        private void mapsConstructorToolStripMenuItem_Click(object sender, EventArgs e) //включение режима конструктора
        {
            //заполнение панели спрайтов
            Graphics gr = CreateMap.CreateGraphics();
            gr.DrawImage(Maps.MakeSpritePanel(), 0, 0);

            //вывод пустой карты
            Maps.EmptyMap(mapArr); //загрузить в массив пустую карту
            Graphics gr2 = Battleground.CreateGraphics();
            gr2.DrawImage(Maps.MakeMap(mapArr), 0, 0); //рисуем пустую карту
        }

        private void CreateMap_MouseDown(object sender, MouseEventArgs e) //выбор спрайта на панели конструктора
        {
            byte x = (byte)(e.X / 32);
            byte y = (byte)(e.Y / 32);

            //заполнение массива с идентификаторами спрайтов
            //осторожно, возможно индусский код! но это не точно
            byte[,] spriteArr = new byte[2, 3];
            for (byte i = 0; i < 2; i++)
            {
                for (byte j = 0; j < 3; j++)
                {
                    spriteArr[i, j] = (byte)(i * 3 + j);
                }
            }

            crMapNumb = spriteArr[y, x]; //запомнить выбранный спрайт            
        }

        private void Battleground_MouseDown(object sender, MouseEventArgs e) //включение режима рисования в конструкторе
        {
            drawMode = true;
            //рисуем один спрайт
            Graphics gr = Battleground.CreateGraphics();
            Maps.drawMap(mapArr, e.X, e.Y, crMapNumb, gr);
        }

        private void Battleground_MouseMove(object sender, MouseEventArgs e) //рисование карты в конструкторе
        {
            if (drawMode) //если включен режим рисования
            {
                Graphics gr = Battleground.CreateGraphics();
                Maps.drawMap(mapArr, e.X, e.Y, crMapNumb, gr);
            }
        }

        private void Battleground_MouseUp(object sender, MouseEventArgs e) //отключение режима рисования в конструкторе
        {
            drawMode = false;
        }

        private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e) //сохранение карты в файл
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = Application.StartupPath + @"\Maps\";
            sfd.Filter = "Tank maps files(*.tmap)|*.tmap";
            if (sfd.ShowDialog() == DialogResult.Cancel) { return; }
            StreamWriter objWriter = new StreamWriter(sfd.FileName, false, Encoding.Default);
            string mapLine = "";
            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 40; j++)
                {
                    mapLine += mapArr[i, j] + " ";
                }
                objWriter.WriteLine(mapLine.Trim());
                mapLine = "";
            }
            objWriter.Close();
        }

        private void loadFromFileToolStripMenuItem_Click(object sender, EventArgs e) //загрузка карты из файла
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Application.StartupPath + @"\Maps\";
            ofd.Filter = "Tank maps files(*.tmap)|*.tmap";
            if (ofd.ShowDialog() == DialogResult.Cancel) { return; }
            Maps.ReadMap(mapArr, ofd.FileName); //считывание файла в массив
            Graphics gr = Battleground.CreateGraphics();
            gr.DrawImage(Maps.MakeMap(mapArr), 0, 0); //отрисовка карты из массива 
        }
    }
}