using ColumnsGame.Model;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ColumnsGame.ViewModel
{
    public partial class GameWindowViewModel: INotifyPropertyChanged
    {

        /// <summary>
        ///  The event is invoked when it is necessary to refresh the UI
        /// </summary>
        public event EventHandler NeedRefreshVisualControl;


        #region Properties

        private int _rowsCount;
        private int _columnsCount;
        private int _colorsCount;
        private bool _showAnimation;
        private int _speedGame;
        private bool _gameRunnig;
        private bool _gameOver;
        private GameBoard _board;


        /// <summary>
        /// Get or Set GameBoard
        /// </summary>
        public GameBoard Board
        {
            get => _board;
            private set
            {
                _board = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Get or Set rows count board
        /// </summary>
        public int RowsCount
        {
            get => _rowsCount;
            set
            {
                _rowsCount = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Get or Set columns count board
        /// </summary>
        public int ColumnsCount
        {
            get => _columnsCount;
            set
            {
                _columnsCount = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Get or Set Colors count board
        /// </summary>
        public int ColorsCount
        {
            get => _colorsCount;
            set
            {
                _colorsCount = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Get or Set whether an animation is showed
        /// </summary>
        public bool ShowAnimation
        { 
            get => _showAnimation;
            set 
            {
                _showAnimation = value;

                if (Board != null)
                    Board.ShowAnimation = value;

                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Get or Set speed game, recommend 0(slow) to 10(fast)
        /// </summary>
        public int SpeedGame
        {
            get => _speedGame;
            set
            {
                _speedGame = value;
                RaisePropertyChanged();
            }
        }


        /// <summary>
        /// Get or Set whether the game is running (Run/Pause)
        /// </summary>
        public bool GameRunnig
        {
            get => _gameRunnig;
            set
            {
                _gameRunnig = value;
                RaisePropertyChanged();
                updateButtonCanExecute();
            }
        }


        /// <summary>
        /// Get or Set whether the game over
        /// </summary>
        public bool GameOver
        {
            get => _gameOver;
            set
            {
                _gameOver = value;

                if (_gameOver)
                    GameRunnig = false;

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(GameRunnig));
                updateButtonCanExecute();
            }
        }


        /// <summary>
        /// Get or Set start speed game
        /// </summary>
        public int StartSpeed { get; set; }


        #endregion


        public GameWindowViewModel()
        {
            RowsCount = GameBoard.DefaultRowsCount;
            ColumnsCount = GameBoard.DefaultColumnsCount;
            ColorsCount = GameBoard.DefaultColorsCount;
            ShowAnimation = true;
            
            createCommands();
            createNewGameBoard();
        }


        private void createNewGameBoard()
        {
            Board = new GameBoard(RowsCount, ColumnsCount, ColorsCount, ShowAnimation);

            Board.NeedRefreshVisualControl += (s, e) =>
            {
                NeedRefreshVisualControl?.Invoke(s,e);
            };
        }


        #region Board move

        public void MoveLeft()
        {
            Board.MoveLeft();
        }

        public void MoveRight()
        {
            Board.MoveRight();
        }

        public void Rotate()
        {
            Board.RotateItem();
        }

        public async void MoveDown()
        {
            if (!await Board.MoveDown())
                GameOver = true; 

            // speed-up game
            SpeedGame = StartSpeed + (Board.Score / 10);

        }

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
