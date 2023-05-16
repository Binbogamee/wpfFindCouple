using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wpfFindCouple
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timer;
        private DateTime startTimer;
        private const int Row_Count = 2;
        Random rnd = new Random();
        private Game game;
        private int selectLevel = 1;
        private List<Image> monsters = new List<Image>();
        private List<int> firstcards = new List<int>(); //список номеров карт для заполнения поля
        private int[,] field; //массив с номерами карт
        private Button[,] buttons; //массив кнопок
        private int couple = 0; //проверка на количество перевернутых карт
        private  (int, int) firstCard;
        private  (int, int) secondCard;
        private int numberMonster = 0;
        private double damage;

        public MainWindow()
        {
            InitializeComponent();

            game = new Game();
            game.DoReset();

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(10000000);

            buOktiLevels.IsEnabled = false;

            buPlaytiMain.Click += (s, e) => tabControl1.SelectedItem = tiLevels;
            buAbouttiMain.Click += (s, e) => tabControl1.SelectedItem = tiAbout;
            buExittilosing.Click += (s, e) => this.Close();
            buExittiMain.Click += (s, e) => this.Close();
            buExittiWin.Click += (s, e) => this.Close();
            buBacktiAbout.Click += (s, e) => tabControl1.SelectedItem = tiMain;
            buBacktiLevels.Click += (s, e) => { tabControl1.SelectedItem = tiMain; buOktiLevels.IsEnabled = false; };
            buSimpletiLevels.Click += (s, e) => { selectLevel = 1; buOktiLevels.IsEnabled = true; };
            buMediumtiLevels.Click += (s, e) => { selectLevel = 2; buOktiLevels.IsEnabled = true; };
            buHardtiLevels.Click += (s, e) => { selectLevel = 3; buOktiLevels.IsEnabled = true; };
            buAuthor.Click += (s, e) => tabControl1.SelectedItem = tiAuthor;
            buBacktiAutor.Click += (s, e) => tabControl1.SelectedItem = tiMain;
            ugCardstiGame.SizeChanged += UgCardstiGame_SizeChanged;

        }

        /// <summary>
        /// изменение размера карт
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UgCardstiGame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            for (int i = 0; i < game.CoupleCount*2; i++)
            {
                var t = ugCardstiGame.Children[i] as Button;
                t.Width = (this.Width - 50) / game.CoupleCount;
            }
        }

        /// <summary>
        /// Окрывает вкладку с игрой, сохраняет выбранный уровень и запускает игру
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buOktiLevels_Click(object sender, RoutedEventArgs e)
        {
            tabControl1.SelectedItem = tiGame;
            game.Level = selectLevel;
            game.StartGame();
            damage = 100 / game.CoupleCount;
            pbHPtiGame.Value = 100;
            lastMonsters.Text = (game.CoupleCount - numberMonster).ToString();

            //создание списка монстров
            for (int i = 0; i < game.MonsterCount; i++)
            {
                Image im = new Image();
                im.Source = new BitmapImage(new Uri("Resources/" + (i+1).ToString() +".png", UriKind.Relative));
                im.Name = "m" + i.ToString();
                im.RenderTransform = new RotateTransform(angle: 0, centerX: this.Width/6, centerY: this.Height/3);
                im.Margin = new Thickness(0, 0, 0, 0);
               
                monsters.Add(im);
                
            }

            //создание поля для карт
            gridGame.Children.Add(monsters[numberMonster]);
            Grid.SetColumn(monsters[numberMonster], 1);
            Grid.SetRow(monsters[numberMonster], 1);

            //создание карт
            createCards();

            this.RegisterName(monsters[numberMonster].Name, monsters[numberMonster]);
        }

        /// <summary>
        /// Обратный таймер
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerTick(object sender, EventArgs e)
        {
            Storyboard animation = Resources["Storyboard"] as Storyboard;
            Storyboard.SetTargetName(animation, "m" + numberMonster.ToString());
            animation.Begin();
            var x = startTimer - DateTime.Now;
            if (x.TotalSeconds < 0)
            {
                timer.Stop();
                x = TimeSpan.FromSeconds(0);
                tabControl1.SelectedItem = tiLosing;
                numberMonster++;
            }

            tbTimetiGame.Text = x.ToString(@"mm\:ss");
        }

        private async void Card_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                await Task.Delay(200);

                //получение позиции карты
                string s = (b.Tag).ToString();
                var coord = (Convert.ToInt32(s[1].ToString()), Convert.ToInt32(s[4].ToString()));
                //добавление картинки
                Image im = new Image();
                im.Source = new BitmapImage(new Uri("Resources/card" + (field[coord.Item1, coord.Item2]).ToString() + ".png", UriKind.Relative));
                b.Background = Brushes.LightSkyBlue;
                b.Content = im;
                b.IsEnabled = false;
                //сохранение координат первой карты
                if (couple == 0)
                {
                    firstCard = coord;
                }
                //сохранение координат второй карты
                if (couple == 1)
                {
                    secondCard = coord;
                    foreach (var item in buttons)
                    {
                        item.IsEnabled = false;
                    }
                }
                couple++;
                if (couple == 2)
                {
                    //если номера в массиве совпадают -> урон, не переворачиваем карты, + в счетчик победы
                    if (field[firstCard.Item1, firstCard.Item2] == field[secondCard.Item1, secondCard.Item2])
                    {
                        pbHPtiGame.Value -= damage;
                        Storyboard animation = Resources["damage"] as Storyboard;
                        Storyboard animationfinish = Resources["finish"] as Storyboard;
                        Storyboard.SetTargetName(animation, "m" + numberMonster.ToString());
                        Storyboard.SetTargetName(animationfinish, "m" + numberMonster.ToString());
                        animation.Begin();
                        await Task.Delay(200);
                        animation.Begin();
                        await Task.Delay(200);
                        animation.Begin();
                        await Task.Delay(200);
                        animationfinish.Begin();

                        game.PartWin++;
                        foreach (var item in buttons)
                        {
                            item.IsEnabled = true;
                        }
                        b.IsEnabled = false;
                        buttons[firstCard.Item1, firstCard.Item2].IsEnabled = false;
                    }
                    // если номера не совпадают -> переворачиваем обратно, убираем картинки
                    else
                    {
                        await Task.Delay(500);
                        foreach (var item in buttons)
                        {
                            item.IsEnabled = true;
                        }

                        Storyboard animation1 = Resources["sbFirstCard"] as Storyboard;
                        Storyboard animation2 = Resources["sbSecondCard"] as Storyboard;
                        Storyboard.SetTargetName(animation1, "card" + firstCard.Item1.ToString() + firstCard.Item2.ToString());
                        Storyboard.SetTargetName(animation2, "card" + secondCard.Item1.ToString() + secondCard.Item2.ToString());

                        animation1.Begin();
                        animation2.Begin();

                        b.Content = null;
                        b.Background = Brushes.LightSeaGreen;
                        buttons[firstCard.Item1, firstCard.Item2].Background = Brushes.LightSeaGreen;
                        buttons[firstCard.Item1, firstCard.Item2].Content = null;
                    }
                    couple = 0;
                    //проверка на завершение игры
                    if (game.PartWin == game.CoupleCount)
                    {
                        newMonster();
                    }
                }
            }
        }

        /// <summary>
        /// переходит к новому монстру и создает новые карты или отображает победу
        /// </summary>
        private void newMonster()
        {
            //очищаем регистр имен от карт
            for (int i = 0; i < Row_Count; i++)
            {
                for (int j = 0; j < game.CoupleCount; j++)
                {
                    UnregisterName("card" + i.ToString() + j.ToString());
                }
            }
            pbHPtiGame.Value = 100;
            numberMonster++;
            game.PartWin = 0;

            // если монстры закончились -> победа
            if (numberMonster == game.MonsterCount)
            {
                tabControl1.SelectedItem = tiWin;
                timer.Stop();
            }
            //иначе удаляем прошлого монстра, добавляем следующего, создаем карты заново
            else
            {
                gridGame.Children.Remove(monsters[numberMonster - 1]);
                this.UnregisterName(monsters[numberMonster-1].Name);
                this.RegisterName(monsters[numberMonster].Name, monsters[numberMonster]);
                gridGame.Children.Add(monsters[numberMonster ]);
                Grid.SetColumn(monsters[numberMonster], 1);
                Grid.SetRow(monsters[numberMonster], 1);

                createCards();
            }
            lastMonsters.Text = (game.CoupleCount - numberMonster).ToString();
        }

        /// <summary>
        /// создает массивы карт и номеров карт, размещает карты на поле
        /// </summary>
        private void createCards()
        {
            //очистка поля и списка с начальными номерами карт
            ugCardstiGame.Children.Clear();
            firstcards.Clear();
            //создание списка с номерами
            for (int i = 0; i < game.CoupleCount; i++)
            {
                firstcards.Add(i);
                firstcards.Add(i);
            }
            //создание массива с номерами
            field = new int[Row_Count, game.CoupleCount];
            buttons = new Button[Row_Count, game.CoupleCount];
            for (int i = 0; i < Row_Count; i++)
            {
                for (int j = 0; j < game.CoupleCount; j++)
                {
                    int t = rnd.Next(firstcards.Count);
                    field[i, j] = firstcards[t];
                    firstcards.Remove(firstcards[t]);
                }
            }

            //добавление карт на карту
            ugCardstiGame.Rows = Row_Count;
            ugCardstiGame.Columns = game.CoupleCount;

            for (int i = 0; i < Row_Count; i++)
            {
                for (int j = 0; j < game.CoupleCount; j++)
                {
                    Button btn = new Button();
                    btn.Style = Resources["button_cards"] as Style;
                    btn.Tag = (i, j);
                    btn.Name = "card" + i.ToString() + j.ToString();
                    btn.Width = (this.Width-50) / game.CoupleCount;
                    this.RegisterName(btn.Name, btn);

                    ugCardstiGame.Children.Add(btn);
                    btn.Click += Card_Click;
                    buttons[i, j] = btn;
                }
            }

            //включение таймера
            startTimer = DateTime.Now.AddSeconds(game.MaxSeconds);
            timer.Start();
        }

        /// <summary>
        /// Открывает вкладку главная, обнуляет игру
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buFinishtiGame_Click(object sender, RoutedEventArgs e)
        {
            buOktiLevels.IsEnabled = false;
            numberMonster++;
            startNewGame();
            game.DoReset();
            tabControl1.SelectedItem = tiMain;
        }

        /// <summary>
        /// Обнуляет игру, открывает вкладку уровни
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buNewGame_Click(object sender, RoutedEventArgs e)
        {
            buOktiLevels.IsEnabled = false;
            tabControl1.SelectedItem = tiLevels;
            startNewGame();
            game.DoReset();
        }

        /// <summary>
        /// Обнуляет и/или создает новую игру
        /// </summary>
        private void startNewGame()
        {
            pbHPtiGame.Value = 100;
            this.UnregisterName(monsters[numberMonster-1].Name);
            gridGame.Children.Remove(monsters[numberMonster-1]);
            numberMonster = 0;
            couple = 0;
            timer.Stop();
            selectLevel = 1;
            monsters.Clear();
            firstcards.Clear();
            //очищаем регистр имен от карт
            for (int i = 0; i < Row_Count; i++)
            {
                for (int j = 0; j < game.CoupleCount; j++)
                {
                    try
                    {
                        UnregisterName("card" + i.ToString() + j.ToString());
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            for (int i = 0; i < Row_Count; i++)
            {
                for (int j = 0; j < game.CoupleCount; j++)
                {
                    field[i, j] = 0;
                }
            }
            for (int i = 0; i < Row_Count; i++)
            {
                for (int j = 0; j < game.CoupleCount; j++)
                {
                    buttons[i, j] = null;
                }
            }
        }

    }
}
