using ColumnsGame.Enums;
using ColumnsGame.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ColumnsGame.Model
{
    /// <summary>
    /// Represents a game board with size, items, falling column, next column
    /// </summary>
    public class GameBoard : INotifyPropertyChanged
    {

        public static readonly int MaxRowsCount = 100;
        public static readonly int MinRowsCount = 7;
        public static readonly int DefaultRowsCount = 13;

        public static readonly int MaxColumnsCount = 100;
        public static readonly int MinColumnsCount = 3;
        public static readonly int DefaultColumnsCount = 6;

        public static readonly int MaxColorsCount = 10;
        public static readonly int MinColorsCount = 2;
        public static readonly int DefaultColorsCount = 5;


        /// <summary>
        ///  The event is invoked when it is necessary to refresh the UI
        /// </summary>
        public event EventHandler NeedRefreshVisualControl;


        #region Properties

        private StorageItems _storageItems;
        private int _rowsCount;
        private int _columnsCount;
        private int _colorsCount;

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
        /// Get or Set colors count board
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

        public ConcurrentDictionary<int, ItemColor> Cells => _storageItems.GetCells();

        /// <summary>
        /// Get or Set whether an animation is showed
        /// </summary>
        public bool ShowAnimation { get; set; }

        /// <summary>
        /// Get or Set game score 
        /// </summary>
        public int Score { get; set; }


        /// <summary>
        /// Get or Set falling column item
        /// </summary>
        public ColumnItem ColumnItem { get; set; }

        /// <summary>
        /// Get or Set
        /// </summary>
        public ColumnItem NextColumnItem { get; set; }

        #endregion


        /// <summary>
        /// Create a game board with parameter 
        /// </summary>
        /// <param name="rowsCount">Rows count board</param>
        /// <param name="columnsCount">Columns count board</param>
        /// <param name="colorCount">Color count board</param>
        /// <param name="animation">Indicates whether an animation is showed</param>
        public GameBoard(int rowsCount, int columnsCount, int colorCount, bool animation)
        {
            if (rowsCount < MinRowsCount || rowsCount > MaxRowsCount)
                throw new ArgumentOutOfRangeException(nameof(rowsCount));

            if (columnsCount < MinColumnsCount || columnsCount > MaxColumnsCount)
                throw new ArgumentOutOfRangeException(nameof(rowsCount));

            if (colorCount < MinColorsCount || colorCount > MaxColorsCount)
                throw new ArgumentOutOfRangeException(nameof(rowsCount));


            RowsCount = rowsCount;
            ColumnsCount = columnsCount;
            ColorsCount = colorCount;
            ShowAnimation = animation;

            _storageItems = new StorageItems();

    }

        #region Public methods

        /// <summary>
        /// Initializing and start game
        /// </summary>
        public void StartGame()
        {
            Score = 0;
            _storageItems.Clear();
            createNewColumnItems();
        }

        /// <summary>
        /// Stop game
        /// </summary>
        public void StopGame()
        {
            ColumnItem = null;
            NextColumnItem = null;
            refreshUI();
        }


        /// <summary>
        ///  Move item left
        /// </summary>
        public void MoveLeft()
        {
            if (ColumnItem == null)
                return;

            ColumnItem.Column--;

            if (!validateMove())
            {
                ColumnItem.Column++;
            }
        }

        /// <summary>
        /// Move item right
        /// </summary>
        public void MoveRight()
        {
            if (ColumnItem == null)
                return;
            ColumnItem.Column++;

            if (!validateMove())
            {
                ColumnItem.Column--;
            }
        }

        /// <summary>
        /// Rotate item
        /// </summary>
        public void RotateItem()
        {
            ColumnItem?.Rotate();
        }


        /// <summary>
        /// Move down falling column item and check valid item
        /// </summary>
        /// <returns>Return false when the game ends, otherwise return true</returns>
        public async Task<bool> MoveDown()
        {
            if (ColumnItem == null)
                return true;

            ColumnItem.Row--;
            if (!validateMove())
            {
                ColumnItem.Row++;

                // check game over
                if (ColumnItem.Row >= RowsCount - 2)
                {
                    return false;
                }

                _storageItems.Add(ColumnItem);
                ColumnItem = null;
                refreshUI();

                // check valid items, delete, and fall down
                await Task.Run(() =>
                {
                    while (checkItemsForRemove())
                    {
                        refreshUI();
                        _storageItems.FallDownItems();
                    }
                });

                switchAndCreateColumnItems();
                refreshUI();
            }

            return true;
        }

        #endregion region


        #region Private method


        /// <summary>
        /// Switch next column item to falling item and create new next item
        /// </summary>
        public void switchAndCreateColumnItems()
        {
            ColumnItem = NextColumnItem;
            NextColumnItem = new ColumnItem(RowsCount - 1, ColumnsCount / 2, ColorsCount);
        }


        /// <summary>
        /// Create next column item and falling column item
        /// </summary>
        private void createNewColumnItems()
        {
            ColumnItem = new ColumnItem(RowsCount - 1, ColumnsCount / 2, ColorsCount);
            NextColumnItem = new ColumnItem(RowsCount, ColumnsCount / 2, ColorsCount);
        }


        /// <summary>
        /// Validate the column item move 
        /// </summary>
        /// <returns>Return False for invalid move, True for valid move</returns>
        private bool validateMove()
        {
            if (ColumnItem.Row < 0 || ColumnItem.Column > (ColumnsCount - 1) || ColumnItem.Column < 0)
                return false;

            return !_storageItems.Contains(ColumnItem.Row, ColumnItem.Column);
        }


        /// <summary>
        /// check items for remove from board
        /// </summary>
        /// <returns>Return True when some item has been removed, otherwise Return False  </returns>
        private bool checkItemsForRemove()
        {
            var dicDelete = _storageItems.GetItemsForDelete();

            Score += dicDelete.Count;

            var ret = dicDelete.Count > 0;

            if (ret && ShowAnimation)
            {
                animateDeleteItems(dicDelete);
            }

            _storageItems.Remove(dicDelete);

            return ret;
        }


        /// <summary>
        /// Animate deleted item (change color to transparent and back )
        /// </summary>
        /// <param name="dicDelete"></param>
        private void animateDeleteItems(Dictionary<int, ItemColor> dicDelete)
        {

            for (int i = 0; i < 4; i++)
            {
                _storageItems.SetItemsTransparentColor(dicDelete);
                refreshUI();
                Thread.Sleep(100);

                _storageItems.SetItemsColor(dicDelete);
                refreshUI();
                Thread.Sleep(100);
            }
        }



        /// <summary>
        /// test method for random fill board, not use :-)
        /// </summary>
        public void FillBoard()
        {
            for (int row = 0; row < RowsCount - 4; row++)
            {
                for (int col = 0; col < ColumnsCount; col ++)
                {
                    _storageItems.AddItem(row, col, ColorHelper.GetRandomColor(ColorsCount));
                }
            }
        }
        

        /// <summary>
        /// UI refresh request 
        /// </summary>
        private void refreshUI()
        {
            NeedRefreshVisualControl?.Invoke(this, EventArgs.Empty);
        }

        #endregion


        #region 

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
