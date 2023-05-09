using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Xps.Packaging;

namespace Snake
{
    public enum GridValue
    {
        Empty,
        Snake,
        Food,
        Outside
    }

    public class GameState
    {
        public int Score { get; private set; }
        public bool GameOver { get; private set; }

        public int Rows { get; }
        public int Columns { get; }
        public GridValue[,] Grid { get; } //Row, Column
        public Direction Dir { get; private set; }

        private readonly LinkedList<Direction> directionBuffer = new LinkedList<Direction>(); 
        private readonly LinkedList<Position> snakePositions = new LinkedList<Position>(); //Linked list so we can edit from the front and end of the list. For our purpose the start of the list is the head of the snake. the end is the tail.
        private readonly Random random = new Random(); //For spawing food

        public GameState(int rows, int cols)
        {
            Rows = rows;
            Columns = cols;

            Grid = new GridValue[rows, cols]; //Initialize grid. Everything starts as empty
            Dir = Direction.right;

            AddSnake();
            AddFood();

        }

        //Add the snake to the grid
        private void AddSnake()
        {
            var startRow = Rows / 2;
            var startColumn = Columns / 5;

            //If start Column is smaller than 3 than start on 3
            if (startColumn < 3) { startColumn = 3; }

            for (int c = startColumn - 2; c <= startColumn; c++)
            {
                Grid[startRow, c] = GridValue.Snake;
                snakePositions.AddFirst(new Position(startRow, c));
            }
        }

        private IEnumerable<Position> EmptyPosistions()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    if (Grid[row, col] == GridValue.Empty)
                    {
                        yield return new Position(row, col);
                    }
                }
            }
        }

        private void AddFood()
        {
            List<Position> empty = new List<Position>(EmptyPosistions());

            if (empty.Count == 0) { return; } //if this list is empty the player won

            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Column] = GridValue.Food;

        }

        public Position HeadPosition()
        {
            if (snakePositions.First is not null)
            {
                return snakePositions.First.Value;
            }

            return null;
        }

        public Position TailPosition()
        {
            if (snakePositions.Last is not null)
            {
                return snakePositions.Last.Value;
            }

            return null;
        }

        public IEnumerable<Position> SnakePositions()
        {
            return snakePositions;
        }

        private void AddHead(Position pos)
        {
            snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Column] = GridValue.Snake;
        }

        private void RemoveTail()
        {
            if (snakePositions.Count == 0) { return; }

            Position tail = snakePositions.Last!.Value;
            Grid[tail.Row, tail.Column] = GridValue.Empty;
            snakePositions.RemoveLast();
        }

        private bool CanChangeDirection(Direction newDir)
        {
            //if the buffer already has 5 than its just spaming buttons.
            if (directionBuffer.Count == 5) { return false; }

            Direction lastDir = directionBuffer.Last is not null ? directionBuffer.Last.Value : Dir;

            return newDir != lastDir && newDir != lastDir.Opposite();
        }

        public void ChangeDirection(Direction direction)
        {
            if (CanChangeDirection(direction))
            {
                directionBuffer.AddLast(direction);
            }

        }


        private bool OutsideGrid(Position pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Column < 0 || pos.Column >= Columns;
        }

        private GridValue WillHit(Position newHeadPosistion)
        {
            if (OutsideGrid(newHeadPosistion)) { return GridValue.Outside; } //Outside the grid

            return newHeadPosistion == TailPosition() ? GridValue.Empty : Grid[newHeadPosistion.Row, newHeadPosistion.Column];
        }

        public void Move()
        {
            //Loop through the buffer if the last direction change is not valid than go to the next
            while (directionBuffer.Count > 0 && directionBuffer.Last is not null)
            {
                Direction lastDir = directionBuffer.Last.Value;

                if (lastDir.Opposite() != Dir)
                {
                    Dir = lastDir;
                    break;
                }

                directionBuffer.RemoveLast();
            }

            directionBuffer.Clear(); //Clear movement buffer

            Position newHeadPos = HeadPosition().Translate(Dir);

            GridValue hit = WillHit(newHeadPos);

            //Game over
            if (hit == GridValue.Outside || hit == GridValue.Snake) { GameOver = true; }

            //Move
            else if (hit == GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPos);
            }

            //Grow
            else if (hit == GridValue.Food)
            {
                AddHead(newHeadPos);
                Score++;
                AddFood();
            }
        }
    }
}
