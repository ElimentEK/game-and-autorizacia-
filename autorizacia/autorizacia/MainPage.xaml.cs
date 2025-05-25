using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace autorizacia
{
    public partial class MainPage : ContentPage
    {
  
        private const int TileSize = 20;
        private const int GameSpeed = 150;
        
        private BoxView snakeHead;
        private List<BoxView> snakeBody = new List<BoxView>();
        private BoxView food;
        
        private int score = 0;
        private int gameWidth;
        private int gameHeight;
        
        private enum Direction { Up, Down, Left, Right }
        private Direction currentDirection = Direction.Right;
        private Direction nextDirection = Direction.Right;
        
        private bool isGameRunning = false;
        private Random random = new Random();

        public MainPage()
        {
            InitializeComponent();
            
            // Ждем, когда layout будет полностью загружен
            gameArea.SizeChanged += OnGameAreaSizeChanged;

        }
        private void OnUpClicked(object sender, EventArgs e)
        {
            if (currentDirection != Direction.Down)
                nextDirection = Direction.Up;
        }

        private void OnDownClicked(object sender, EventArgs e)
        {
            if (currentDirection != Direction.Up)
                nextDirection = Direction.Down;
        }

        private void OnLeftClicked(object sender, EventArgs e)
        {
            if (currentDirection != Direction.Right)
                nextDirection = Direction.Left;
        }

        private void OnRightClicked(object sender, EventArgs e)
        {
            if (currentDirection != Direction.Left)
                nextDirection = Direction.Right;
        }
        private void OnGameAreaSizeChanged(object sender, EventArgs e)
        {
            gameArea.SizeChanged -= OnGameAreaSizeChanged;
            
            gameWidth = (int)(gameArea.Width / TileSize);
            gameHeight = (int)(gameArea.Height / TileSize);
            
            StartGame();
        }
        
        private void StartGame()
        {
            // Очищаем игровое поле
            foreach (var bodyPart in snakeBody)
            {
                AbsoluteLayout.SetLayoutBounds(bodyPart, new Rectangle(0, 0, 0, 0));
                gameArea.Children.Remove(bodyPart);
            }
            
            if (snakeHead != null)
            {
                gameArea.Children.Remove(snakeHead);
            }
            
            if (food != null)
            {
                gameArea.Children.Remove(food);
            }
            
            snakeBody.Clear();
            score = 0;
            scoreLabel.Text = $"Score: {score}";
            restartButton.IsVisible = false;
            
            // Создаем голову змеи
            snakeHead = new BoxView
            {
                Color = Color.Green
            };
            AbsoluteLayout.SetLayoutBounds(snakeHead, new Rectangle(5 * TileSize, 5 * TileSize, TileSize, TileSize));
            gameArea.Children.Add(snakeHead);
            
            // Начальное направление
            currentDirection = Direction.Right;
            nextDirection = Direction.Right;
            
            // Создаем еду
            SpawnFood();
            
            isGameRunning = true;
            
            // Запускаем игровой цикл в отдельном потоке
            Thread gameThread = new Thread(GameLoop);
            gameThread.Start();
        }
        
        private void GameLoop()
        {
            while (isGameRunning)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    MoveSnake();
                    CheckCollision();
                });
                
                Thread.Sleep(GameSpeed);
            }
        }

        private void MoveSnake()
        {
            currentDirection = nextDirection;

            // Сохраняем текущую позицию головы
            var headBounds = AbsoluteLayout.GetLayoutBounds(snakeHead);
            double prevX = headBounds.X;
            double prevY = headBounds.Y;

            // Двигаем голову
            switch (currentDirection)
            {
                case Direction.Up:
                    headBounds.Y -= TileSize;
                    break;
                case Direction.Down:
                    headBounds.Y += TileSize;
                    break;
                case Direction.Left:
                    headBounds.X -= TileSize;
                    break;
                case Direction.Right:
                    headBounds.X += TileSize;
                    break;
            }

            // Проверяем границы
            if (headBounds.X < 0) headBounds.X = (gameWidth - 1) * TileSize;
            if (headBounds.X >= gameWidth * TileSize) headBounds.X = 0;
            if (headBounds.Y < 0) headBounds.Y = (gameHeight - 1) * TileSize;
            if (headBounds.Y >= gameHeight * TileSize) headBounds.Y = 0;

            AbsoluteLayout.SetLayoutBounds(snakeHead, headBounds);

            // Двигаем тело
            if (snakeBody.Count > 0)
            {
                // Перемещаем последний сегмент тела на позицию головы
                var lastBodyPart = snakeBody[snakeBody.Count - 1];
                AbsoluteLayout.SetLayoutBounds(lastBodyPart, new Rectangle(prevX, prevY, TileSize, TileSize));

                // Перемещаем последний сегмент в начало списка
                snakeBody.RemoveAt(snakeBody.Count - 1);
                snakeBody.Insert(0, lastBodyPart);
            }
        }

        private void CheckCollision()
        {
            var headBounds = AbsoluteLayout.GetLayoutBounds(snakeHead);

            // Проверяем столкновение с едой
            var foodBounds = AbsoluteLayout.GetLayoutBounds(food);
            if (headBounds.X == foodBounds.X && headBounds.Y == foodBounds.Y)
            {
                EatFood();
                return; // Прерываем проверку, чтобы избежать ложного столкновения
            }

            // Проверяем столкновение с телом (пропускаем первые 2 сегмента,\
            // так как они не могут столкнуться с головой сразу после поворота)
            for (int i = 2; i < snakeBody.Count; i++)
            {
                var bodyPart = snakeBody[i];
                var bodyBounds = AbsoluteLayout.GetLayoutBounds(bodyPart);
                if (headBounds.X == bodyBounds.X && headBounds.Y == bodyBounds.Y)
                {
                    GameOver();
                    return;
                }
            }
        }

        private void EatFood()
        {
            score++;
            scoreLabel.Text = $"Score: {score}";

            // Добавляем новый элемент тела
            BoxView newBodyPart = new BoxView
            {
                Color = Color.GreenYellow
            };

            // Позиция нового элемента тела (вне зоны видимости)
            // Он появится на правильной позиции при следующем движении змейки
            AbsoluteLayout.SetLayoutBounds(newBodyPart, new Rectangle(-TileSize, -TileSize, TileSize, TileSize));
            gameArea.Children.Add(newBodyPart);
            snakeBody.Add(newBodyPart);

            // Создаем новую еду
            SpawnFood();
        }

        private void SpawnFood()
        {
            if (food != null)
            {
                gameArea.Children.Remove(food);
            }
            
            food = new BoxView
            {
                Color = Color.Red
            };
            
            int x = random.Next(0, gameWidth);
            int y = random.Next(0, gameHeight);
            
            // Проверяем, чтобы еда не появилась на змее
            bool onSnake;
            do
            {
                onSnake = false;
                
                var headBounds = AbsoluteLayout.GetLayoutBounds(snakeHead);
                if (x * TileSize == headBounds.X && y * TileSize == headBounds.Y)
                {
                    onSnake = true;
                    x = random.Next(0, gameWidth);
                    y = random.Next(0, gameHeight);
                    continue;
                }
                
                foreach (var bodyPart in snakeBody)
                {
                    var bodyBounds = AbsoluteLayout.GetLayoutBounds(bodyPart);
                    if (x * TileSize == bodyBounds.X && y * TileSize == bodyBounds.Y)
                    {
                        onSnake = true;
                        x = random.Next(0, gameWidth);
                        y = random.Next(0, gameHeight);
                        break;
                    }
                }
            } while (onSnake);
            
            AbsoluteLayout.SetLayoutBounds(food, new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize));
            gameArea.Children.Add(food);
        }
        
        private void GameOver()
        {
            isGameRunning = false;
            Device.BeginInvokeOnMainThread(() =>
            {
                restartButton.IsVisible = true;
            });
        }
        
        private void OnRestartClicked(object sender, EventArgs e)
        {
            StartGame();
        }
        
        // Обработка свайпов для управления
        private double startX, startY;

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Создаем распознаватель жестов для всего ContentPage
            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;

            // Добавляем распознаватель жестов к ContentPage
            if (this.Content != null)
            {
                this.Content.GestureRecognizers.Add(panGesture);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Удаляем обработчик жестов при скрытии страницы
            if (this.Content != null && this.Content.GestureRecognizers.Count > 0)
            {
                this.Content.GestureRecognizers.Clear();
            }
        }

        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    startX = e.TotalX;
                    startY = e.TotalY;
                    break;
                    
                case GestureStatus.Completed:
                    double deltaX = e.TotalX - startX;
                    double deltaY = e.TotalY - startY;
                    
                    if (Math.Abs(deltaX) > Math.Abs(deltaY))
                    {
                        // Горизонтальный свайп
                        if (deltaX > 0 && currentDirection != Direction.Left)
                        {
                            nextDirection = Direction.Right;
                        }
                        else if (deltaX < 0 && currentDirection != Direction.Right)
                        {
                            nextDirection = Direction.Left;
                        }
                    }
                    else
                    {
                        // Вертикальный свайп
                        if (deltaY > 0 && currentDirection != Direction.Up)
                        {
                            nextDirection = Direction.Down;
                        }
                        else if (deltaY < 0 && currentDirection != Direction.Down)
                        {
                            nextDirection = Direction.Up;
                        }
                    }
                    break;
            }
        }
    }
}

