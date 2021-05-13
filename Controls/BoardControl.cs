using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ColumnsGame.Enums;
using ColumnsGame.Helpers;
using ColumnsGame.Model;

namespace ColumnsGame.Controls
{
    /// <summary>
    /// Visual component for rendering game board
    /// </summary>
    internal class BoardControl : FrameworkElement
    {

        #region Property

        public static DependencyProperty BoardProperty = DependencyProperty.Register(nameof(Board), 
            typeof(GameBoard), 
            typeof(BoardControl), 
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(BoardPropertyChanged)));

        /// <summary>
        /// Get or Set Game board
        /// </summary>
        public GameBoard Board
        {
            get => (GameBoard)GetValue(BoardProperty);
            set => SetValue(BoardProperty, value);        
        }

        private static void BoardPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is BoardControl c)
            {
                c.InvalidateVisual();
            }
        }


        public static DependencyProperty SpeedProperty = DependencyProperty.Register(nameof(Speed), 
            typeof(int), 
            typeof(BoardControl));


        /// <summary>
        /// Get or Set speed game
        /// </summary>
        public int Speed
        {
            get => (int)GetValue(SpeedProperty);
            set => SetValue(SpeedProperty, value);
        }

        #endregion


        public BoardControl()
        {
            RenderOptions.SetEdgeMode(this, (EdgeMode.Aliased));
            Loaded += (sender, args) => InvalidateVisual();
        }


        private int _sizeItem;
        private int _shiftX;
        private DrawingContext _context;


        protected override void OnRender(DrawingContext context)
        {
            _context = context;
            base.OnRender(context);

            //background
            _context.DrawRectangle(ColorHelper.BackgroundBoard, null, new Rect(0, 0, ActualWidth, ActualHeight));

            if (Board == null)
                return;

            // compute itemSize by control size 
            var height = (int)ActualHeight / Board.RowsCount;
            var width = (int)ActualWidth / (Board.ColumnsCount + 5); // 5 --> count rectangle for next item 
            _sizeItem = height < width ? height : width;
            _shiftX = ((int) ActualWidth - (_sizeItem * (Board.ColumnsCount + 5))) / 2;

            drawBoardBackground();
            drawCells();
            drawColumnItem();
            drawNextColumnItem();
            drawInfo();

        }


        /// <summary>
        /// Draw background board
        /// </summary>
        private void drawBoardBackground()
        {

            //background desk
            _context.DrawRectangle(ColorHelper.BackgroundDesk, null,
                new Rect(_shiftX, 0, Board.ColumnsCount * _sizeItem, Board.RowsCount * _sizeItem));

            //background next item (size 3x5)
            _context.DrawRectangle(ColorHelper.BackgroundDesk, null,
                new Rect(Board.ColumnsCount * _sizeItem + _sizeItem + _shiftX, 0, 3 * _sizeItem, 5 * _sizeItem));

            // horizontal line
            for (var rowIndex = 1; rowIndex < Board.RowsCount; rowIndex++)
            {
                _context.DrawLine(ColorHelper.Line, new Point(_shiftX, rowIndex * _sizeItem),
                    new Point(_sizeItem * (Board.ColumnsCount + 5) + _shiftX, rowIndex * _sizeItem));
            }

            // vertical line
            for (var coIndex = 1; coIndex < Board.ColumnsCount + 5; coIndex++)
            {
                _context.DrawLine(ColorHelper.Line, new Point(coIndex * _sizeItem + _shiftX, 0),
                    new Point(coIndex * _sizeItem + _shiftX, _sizeItem * Board.RowsCount));
            }
        }


        /// <summary>
        /// Draw all item in storage
        /// </summary>
        private void drawCells()
        {
            if (Board.Cells == null)
                return;

            foreach (var item in Board.Cells)
            {
                var colIndex = StorageItems.GetColumnFromKey(item.Key);
                var rowIndex = StorageItems.GetRowFromKey(item.Key);
                drawRectangle(rowIndex, colIndex, item.Value);
            }
        }


        /// <summary>
        /// Draw column item rectangles
        /// </summary>
        private void drawColumnItem()
        {
            if (Board.ColumnItem == null)
                return;

            drawRectangle(Board.ColumnItem.Row, Board.ColumnItem.Column, Board.ColumnItem.BottonColor);
            drawRectangle(Board.ColumnItem.Row + 1, Board.ColumnItem.Column, Board.ColumnItem.MiddleColor);
            drawRectangle(Board.ColumnItem.Row + 2, Board.ColumnItem.Column, Board.ColumnItem.TopColor);
        }


        /// <summary>
        /// Draw next column item rectangles
        /// </summary>
        private void drawNextColumnItem()
        {
            if (Board.NextColumnItem == null) 
                return;

            drawRectangle(Board.RowsCount - 2, Board.ColumnsCount + 2, Board.NextColumnItem.BottonColor);
            drawRectangle(Board.RowsCount - 3, Board.ColumnsCount + 2, Board.NextColumnItem.MiddleColor);
            drawRectangle(Board.RowsCount - 4, Board.ColumnsCount + 2, Board.NextColumnItem.TopColor);
        }

      
        /// <summary>
        /// Draw game info (score, speed)
        /// </summary>
        private void drawInfo()
        {
            var ft = GetFormattedText("Bodů: " + Board.Score);
            _context.DrawText(ft, new Point((Board.ColumnsCount + 1) * _sizeItem + _shiftX, _sizeItem * 6));

            var ft2 = GetFormattedText("Obtížnost: " + Speed);
            _context.DrawText(ft2, new Point((Board.ColumnsCount + 1) * _sizeItem + _shiftX, _sizeItem * 6 + 40));
        }


        /// <summary>
        /// Draw one item rectangle 
        /// </summary>
        /// <param name="rowIndex">Row index</param>
        /// <param name="columnIndex">Column index</param>
        /// <param name="color">Color</param>
        private void drawRectangle(int rowIndex, int columnIndex, ItemColor color)
        {
            if (rowIndex > (Board.RowsCount - 1))
                return;

            _context.DrawRectangle( ColorHelper.GetColor((int)color), 
                null,
                new Rect(columnIndex * _sizeItem + 1 + _shiftX, ((Board.RowsCount - 1) * _sizeItem) - ((rowIndex) * _sizeItem) + 1, _sizeItem - 2, _sizeItem - 2));
        }


        /// <summary>
        /// Get formatted text for string
        /// </summary>
        /// <param name="text">text string</param>
        /// <returns>Formatted text</returns>
        public FormattedText GetFormattedText(string text)
        {
            return new FormattedText(text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal,
                    new FontStretch()),  _sizeItem > 20 ? 20 : _sizeItem, new SolidColorBrush(Colors.Black), VisualTreeHelper.GetDpi(this).PixelsPerDip);
        }
    }
}
