using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ColumnsGame.Enums;

namespace ColumnsGame.Model
{
    /// <summary>
    /// Storage for item colors
    /// </summary>
    public class StorageItems
    {
        private readonly ConcurrentDictionary<int, ItemColor> _cells = new ConcurrentDictionary<int, ItemColor>();
        private const int columnMultiple = 10000;


        /// <summary>
        /// Convert dictionary key to row index
        /// </summary>
        /// <param name="key">Dictionary key</param>
        /// <returns>Row index</returns>
        public static int GetRowFromKey(int key)
        {
            return key % columnMultiple;
        }

        /// <summary>
        /// Convert dictionary key to column index
        /// </summary>
        /// <param name="key">Dictionary key</param>
        /// <returns>Column index</returns>
        public static int GetColumnFromKey(int key)
        {
            return key / columnMultiple;
        }

        /// <summary>
        /// Get reference to items dictionary
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<int, ItemColor> GetCells()
        {
            return _cells;
        }


        /// <summary>
        /// Clear storage
        /// </summary>
        public void Clear()
        {
            _cells.Clear();
        }


        /// <summary>
        /// Determines whether the storage contains item
        /// </summary>
        /// <param name="row">Items row</param>
        /// <param name="column">Item columns</param>
        /// <returns>Return true if the storage contains item, otherwise false</returns>
        public bool Contains(int row, int column)
        {
            return _cells.ContainsKey(column * columnMultiple + row);
        }


        /// <summary>
        /// Add column item to storage
        /// </summary>
        public void Add(ColumnItem item)
        {
            _cells.TryAdd(item.Column * columnMultiple + item.Row, item.BottonColor);
            _cells.TryAdd(item.Column * columnMultiple + item.Row + 1, item.MiddleColor);
            _cells.TryAdd(item.Column * columnMultiple + item.Row + 2, item.TopColor);
        }


        /// <summary>
        ///  Add one item to storage
        /// </summary>
        /// <param name="row">Row index</param>
        /// <param name="column">Column index</param>
        /// <param name="color">Color</param>
        public void AddItem(int row, int column, ItemColor color)
        {
            _cells.TryAdd(column * columnMultiple + row, color);
        }


        /// <summary>
        /// Delete items from storage
        /// </summary>
        /// <param name="items">Dictionary item for delete</param>
        public void Remove(Dictionary<int, ItemColor> items)
        {
            foreach (var item in items)
            {
                _cells.TryRemove(item.Key, out _);
            }
        }


        /// <summary>
        /// Find and get all item for delete
        /// </summary>
        /// <returns>Return dictionary items for delete</returns>
        public Dictionary<int, ItemColor> GetItemsForDelete()
        {
            Dictionary<int, ItemColor> deleteItems = new Dictionary<int, ItemColor>();

            ItemColor color1;
            ItemColor color2;

            // find all item to delete
            foreach (var item in _cells)
            {

                // horizontal
                if (_cells.TryGetValue(item.Key + 1, out color1) && _cells.TryGetValue(item.Key - 1, out color2))
                {
                    if (color1 == color2 && color2 == item.Value)
                    {
                        deleteItems.TryAdd(item.Key, item.Value);
                        deleteItems.TryAdd(item.Key + 1, item.Value);
                        deleteItems.TryAdd(item.Key - 1, item.Value);
                    }
                }

                // vertical
                if (_cells.TryGetValue(item.Key + columnMultiple, out color1) && _cells.TryGetValue(item.Key - columnMultiple, out color2))
                {
                    if (color1 == color2 && color2 == item.Value)
                    {
                        deleteItems.TryAdd(item.Key, item.Value);
                        deleteItems.TryAdd(item.Key + columnMultiple, item.Value);
                        deleteItems.TryAdd(item.Key - columnMultiple, item.Value);
                    }
                }

                // diagonal 1
                if (_cells.TryGetValue(item.Key + columnMultiple + 1, out color1) && _cells.TryGetValue(item.Key - columnMultiple - 1, out color2))
                {
                    if (color1 == color2 && color2 == item.Value)
                    {
                        deleteItems.TryAdd(item.Key, item.Value);
                        deleteItems.TryAdd(item.Key + columnMultiple + 1, item.Value);
                        deleteItems.TryAdd(item.Key - columnMultiple - 1, item.Value);
                    }
                }

                // diagonal 2
                if (_cells.TryGetValue(item.Key + columnMultiple - 1, out color1) && _cells.TryGetValue(item.Key - columnMultiple + 1, out color2))
                {
                    if (color1 == color2 && color2 == item.Value)
                    {
                        deleteItems.TryAdd(item.Key, item.Value);
                        deleteItems.TryAdd(item.Key + columnMultiple - 1, item.Value);
                        deleteItems.TryAdd(item.Key - columnMultiple + 1, item.Value);
                    }
                }
            }

            return deleteItems;
        }


        /// <summary>
        /// Fall down items in storage
        /// </summary>
        public void FallDownItems()
        {
            var change = true;
            var listFallDown = new List<int>();

            while (change)
            {
                change = false;
                listFallDown.Clear();

                foreach (var item in _cells.OrderBy(a => a.Key).ToList())
                {
                    // is not bottom && not exit item row - 1 -> add to list
                    if (item.Key % columnMultiple > 0 && !_cells.ContainsKey(item.Key - 1))
                        listFallDown.Add(item.Key);
                }

                foreach (var item in listFallDown)
                {
                    change = true;
                    if (_cells.TryRemove(item, out ItemColor valColor))
                    {
                        _cells.TryAdd(item - 1, valColor);
                    }
                }
            }
        }


        /// <summary>
        /// Set items transparent color, by input key value
        /// </summary>
        /// <param name="items">Item for change color</param>
        public void SetItemsTransparentColor(Dictionary<int, ItemColor> items)
        {
            foreach (var item in items)
                _cells.TryUpdate(item.Key, ItemColor.Transparent, item.Value);
        }

        /// <summary>
        /// Set items color, by input key value
        /// </summary>
        /// <param name="items">Item for change color</param>
        public void SetItemsColor(Dictionary<int, ItemColor> items)
        {
            foreach (var item in items)
                _cells.TryUpdate(item.Key, item.Value, ItemColor.Transparent);
        }
    }
}
