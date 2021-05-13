using ColumnsGame.Helpers;
using ColumnsGame.Enums;

namespace ColumnsGame.Model
{
    /// <summary>
    /// Represents a falling Itemcolumns 
    /// </summary>
    public class ColumnItem
    {
        public ColumnItem(int startRow, int startColumn, int colorCount)
        {
            Row = startRow;
            Column = startColumn;
            TopColor = ColorHelper.GetRandomColor(colorCount);
            MiddleColor = ColorHelper.GetRandomColor(colorCount);
            BottonColor = ColorHelper.GetRandomColor(colorCount);
        }

        /// <summary>
        /// Row index
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// Column index
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Top rectangle color
        /// </summary>
        public ItemColor TopColor { get; set; }

        /// <summary>
        /// MIddle rectangle color
        /// </summary>
        public ItemColor MiddleColor { get; set; }


        /// <summary>
        /// Bottom rectangle color
        /// </summary>
        public ItemColor BottonColor { get; set; }

        /// <summary>
        /// Rotate column items
        /// </summary>
        public void Rotate()
        {
            var pom = TopColor;
            TopColor = BottonColor;
            BottonColor = MiddleColor;
            MiddleColor = pom;
        }

    }
}
