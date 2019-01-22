using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace HelloApp { 
    //Листочек в ширину и в высоту имеет по 20 клеток
    public partial class Form1 : Form {
        Helper help; //Помощник
        Plane plane; //Плоскость
        Parabola parabola1, parabola2; //Параболы 1 и 2
        Rect rectangle1, rectangle2; //Прямоугольник 1 и 2
        Ellipse ellipse1; //Эллипс

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            this.Text = "Нарисовать";
            button1.Text = "Рисовалка";
            groupBox1.Text = "Линия";

            help = new Helper(500, 500, 1f, 25, 10);
            pictureBox1.Width = help.W;
            pictureBox1.Height = help.H;

            plane = new Plane(help);
            parabola1 = new Parabola(-2, -3, 1f, +1, help);
            parabola2 = new Parabola(-2, +5, 2f, -1, help);

            rectangle1 = new Rect(-5, +5, 8, 14, help);
            rectangle2 = new Rect(-4, +3, 10, 6, help);

            ellipse1 = new Ellipse(-3, -5, 6, 4, help);

            pictureBox1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e) {
            //Обновляем GUI
            label1.Refresh();
            label2.Refresh();
            pictureBox1.Refresh();
        }

        private void Label1_Paint(object sender, PaintEventArgs e) {
            label1.Text = Convert.ToString(help.W);
        }

        private void Label2_Paint(object sender, PaintEventArgs e) {
            label2.Text = Convert.ToString(help.H);
        }

        /* ЗАКРАШИВАНИЕ ПОЛЯ
         * Порядок следования блоков кода важен. Сначала идёт закрашивание фигур,
         * а уже потом рисование координатной сетки.
         * При рисовании фигур сначала закрашивается вся область, 
         * следующим шагом делается контур.
         */
        private void pictureBox1_Paint(object sender, PaintEventArgs e) {
            //Задаю параметры для букв
            Font n = new Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            //Рисую букву "X" на абсциссе
            e.Graphics.DrawString("Y", n, Brushes.Black, help.W / 2, 0);
            //Рисую букву "Y" на ориднате
            e.Graphics.DrawString("X", n, Brushes.Black, help.W - help.toDiem(1), help.H / 2);
            
            //Сначала делается пересечение. Потом рисуются границы фигур.
            //Заполнение фигур цветом происходит именно во время пересечения.
            intersectAllShapes(ref e);

            //Рисую эллипс (-3; -5)
            ellipse1.drawBorder(ref e);

            //Рисую рамку для прямоугольников 1 (-5; +5) и 2 (-4; -4)
            rectangle1.drawBorder(ref e);
            rectangle2.drawBorder(ref e);

            //Рисуем границы параболы 1 (-2, -3)
            parabola1.drawBorder(ref e);

            //Рисуем границы параболы 2 (-2, +5)
            parabola2.drawBorder(ref e);

            //Запихаем все полученные линии в массив. В таком случае оси X = 0 и Y = 0  лежит в индекса a[n / 2]
            plane.drawCoordinateGrid(ref e);
        }

        /* Пересечение всех фигур и заливка пересеченных областей.
         * Движемся из 2 четверти (-W/2; H/2) до (W/2;-H/2).
         */
        void intersectAllShapes(ref PaintEventArgs e) {
            for (double x = -help.W / 2; x <= help.W / 2; x += 0.2) {
                for (double y = help.H / 2; y >= -help.H / 2; y -= 0.2) {
                    //Принадлежит параболе 1 и прямоугольнику 1
                    if (parabola1.belong(x, y) && rectangle1.belong(x, y)) {
                        help.drawPoint(ref e, x, y, Brushes.White);
                    }

                    //Закрашиваю середину 2 прямоугольника
                    if (rectangle2.belong(x, y) && !parabola1.belong(x, y) && !parabola2.belong(x, y)) {
                        help.drawPoint(ref e, x, y, Brushes.LightGray);
                    }

                    if (parabola2.belong(x, y) && !rectangle2.belong(x, y)) {
                        help.drawPoint(ref e, x, y, Brushes.LightGray);
                    }

                    if (ellipse1.belong(x, y)) {
                        help.drawPoint(ref e, x, y, Brushes.White);
                    }

                    if (!ellipse1.belong(x, y)) {
                        if (ellipse1.belongQuarter1(x, y)) {
                            help.drawPoint(ref e, x, y, Brushes.LightGray);
                        }

                        if (ellipse1.belongQuarter2(x, y)) {
                            help.drawPoint(ref e, x, y, Brushes.LightGray);
                        }

                        if (ellipse1.belongQuarter3(x, y)) {
                            help.drawPoint(ref e, x, y, Brushes.LightGray);
                        }
                    }

                    if (ellipse1.belongQuarter4(x, y)) {
                        help.drawPoint(ref e, x, y, Brushes.White);
                    }

                    if (!ellipse1.belongRect(x, y) && rectangle1.belong(x, y) && !parabola1.belong(x, y) && y <= help.toDiem(-3)) {
                        help.drawPoint(ref e, x, y, Brushes.LightGray);
                    }
                }
            }
        }

        /* TODO
         * Возможно бага при масштабировании, что если менять размер, то не будет закрашиваться последней линией прямоугольник.Поправить.
         * Сделать нормальную разметку.
         * Добавить пересечение.
         * 
         */
    }

    public class Helper {
        public int W = 510, H = 510; //Ширина и высота ветки
        public float scale = 1f; //Масштаб картинки
        public int diem = 25; //Переменная, соответствующая длина стороны квадрата клетки [diem] = 25px
        public int eps = 10; //Маленькая величина для сравнения величин [eps] = px
        public SolidBrush pointBlack = new SolidBrush(Color.Black); //Цвет белой клетки
        public SolidBrush pointWhite = new SolidBrush(Color.White); //Цвет черной клетки
        public Pen line, line0; //line - обычная линия координатной сетки. line0 - линия X = 0 и Y = 0.
        public Pen border; //Цвет краёв фигур

        public Helper(int W, int H, float scale, int diem, int eps) {
            this.W = W;
            this.H = H;
            this.scale = 1f;
            diem = Convert.ToInt32(25 / scale);
            this.eps = eps;
            line = new Pen(Brushes.Black, 0.1f);
            line0 = new Pen(Brushes.Red, 2f);
            border = new Pen(Brushes.Black, 2f);
        }

        //В случае изменения масштаба картинки пересчитать diem
        void recalcDiem() {
            diem = Convert.ToInt32(25 / scale);
        }

        //Преобразование "нормальной" величины в "компьютерную"
        public int toDiem(int val) {
            return val * diem;
        }

        //Перевести "нормальную" величину в "компьютерную"
        public int toDiem(float val) {
            return Convert.ToInt32(val * diem);
        }

        //хз
        public int getX(int x) {
            return toDiem(x + 10);
        }

        //TODO: разобраться с этим
        public int getY(int y) {
            if (y >= 0) return toDiem(10 - y);
            else return toDiem(Math.Abs(y) + 10);
        }

        //Преобразовать ширину из "нормальной" в "компьютерную"
        public int getWidth(int w) {
            return toDiem(w);
        }

        //Преобразовать высоту из "нормальной" в "компьютерную".
        public int getHeight(int h) {
            return toDiem(h);
        }

        /* Преобразовать "нормальную" координату, но в пикселях
         * в "компьютерную" в пикселях.
         * TODO: я сам не особо понимаю для чего это.
         */
        public double getXpx(double x) {
            return x + W / 2;
        }

        /* Преобразовать "нормальную" координату, но в пикселях
         * в "компьютерную" в пикселях. 
         * TODO: Я сам не особо понимаю для чего это.
         */
        public double getYpx(double y) {
            if (y >= 0) return (H / 2 - y);
            else return (Math.Abs(y) + H / 2);
        }

        //Нарисовать точку цвета brush
        public void drawPoint(ref PaintEventArgs e, double x, double y, Brush brush) {
            e.Graphics.FillEllipse(brush, new RectangleF((float)getXpx(x), (float)getYpx(y), 1, 1));
        }
    }

    class Plane {
        //Вспомогательный класс для распечатывания координатной сетки
        private class Line {
            public PointF A, B; //Две точки, через которые проводится прямая

            public Line(PointF A, PointF B) {
                this.A = A;
                this.B = B;
            }
        }

        Helper h;
        private List<Line> linesX = new List<Line>();
        private List<Line> linesY = new List<Line>();

        public Plane(Helper h) {
            this.h = h;
        }

        //Предпросчитать горизонтальные линии
        private void calcXAxes() {
            //Предподсчитаем горизонтальные
            for (int i = 0; i <= h.H; i += h.diem)
            {
                PointF A = new PointF(0, i);
                PointF B = new PointF(h.H, i);
                linesX.Add(new Line(A, B));
            }
        }

        //Предпросчитать вертикальные линии
        private void calcYAxes() {
            //Предподсчитаем вертикальные
            for (int i = 0; i <= h.W; i += h.diem)
            { //need scale
                PointF A = new PointF(i, 0);
                PointF B = new PointF(i, h.H);
                linesY.Add(new Line(A, B));
            }
        }

        //Нарисовать горизонтальные оси
        public void drawXAxes(ref PaintEventArgs e) {
            calcXAxes();
            //Распечатаем
            for (int i = 0; i < linesX.Count; ++i) {
                PointF pt1 = linesX[i].A;
                PointF pt2 = linesX[i].B;
                //В случае, если i==n/2, то ось становится абсциссой.
                e.Graphics.DrawLine(i == linesX.Count / 2 ? h.line0 : h.line, pt1, pt2);
            }
        }

        //Нарисовать вертикальные оси
        public void drawYAxes(ref PaintEventArgs e) {
            calcYAxes();
            //Распечатаем
            for (int i = 0; i < linesY.Count; ++i) {
                PointF pt1 = linesY[i].A;
                PointF pt2 = linesY[i].B;
                //В случае, если i==n/2, то ось становится ординатой
                e.Graphics.DrawLine(i == linesY.Count / 2 ? h.line0 : h.line, pt1, pt2);
            }
        }

        //Нарисовать координатную ось
        public void drawCoordinateGrid(ref PaintEventArgs e) {
            drawXAxes(ref e);
            drawYAxes(ref e);
        }
    }

    //Парабола описывается уравнение Y^2 = sign*kx
    //sign = +1 - парабола влево, sign = -1 - парабола вправо
    //Моя парабола повернута на 90deg, поэтому используется такое уравнение.
    //Для разных работ разный класс.
    //Prob TODO: Сделать общий класс для всех парабол.
    class Parabola {
        Helper h; //Помощник
        int X, Y; //Координаты вершины параболы
        float k; //Коэффицент параболы
        int sign; //+1 - парабола 1, -1 - парабола 2.

        public Parabola(int X, int Y, float k, int sign, Helper h) {
            this.X = X;
            this.Y = Y;
            this.k = k;
            this.sign = sign;
            this.h = h;
        }
        
        //Точка лежит внутри параболы
        public bool belong(double x, double y) {
            if (Math.Pow(y - h.toDiem(Y), 2) + sign * h.toDiem(k) * (x - h.toDiem(X)) < h.eps)
                return true;
            else 
                return false;
        }

        //Точка лежит на контуре параболы
        public bool belongBorder(double x, double y) {
            if ( Math.Abs(Math.Pow(y - h.toDiem(Y), 2) + sign * h.toDiem(k) * (x - h.toDiem(X))) < h.eps )
                return true;
            else
                return false;
        }

        //Нарисовать контур
        public void drawBorder(ref PaintEventArgs e) {
            for (double x = -h.W / 2; x <= h.W / 2; x += 0.25) {
                for (double y = -h.H / 2; y <= h.H / 2; y += 0.25) {
                    //Парабола описывается функцией y^2 = sign*x
                    if ( belongBorder(x, y) ) {
                        e.Graphics.FillEllipse(h.pointBlack, (float)h.getXpx(x), (float)h.getYpx(y), 2, 2);
                    }
                }
            }
        }

        //Сделать заливку цвета brush
        public void fill(ref PaintEventArgs e, Brush brush) {
            for (double x = -h.W / 2; x <= h.W / 2; x += 0.25) {
                for (double y = -h.H / 2; y <= h.H / 2; y += 0.25) {
                    //Парабола описывается функцией y^2 = sign*x
                    if (belong(x, y)) {
                        h.drawPoint(ref e, x, y, brush);
                    }
                }
            }
        }
    }

    class Rect {
        Helper h; //Помощник
        int X1, Y1; //Кордината верхнего левого угла
        int X2, Y2; //Координата правого нижнего угла
        int W, H; //Ширина и высота

        public Rect(int X, int Y, int W, int H, Helper h) {
            this.X1 = X;
            this.Y1 = Y;
            this.W = W;
            this.H = H;
            this.X2 = this.X1 + this.W;
            this.Y2 = this.Y1 - this.H;
            this.h = h;
        }

        //Точка лежит внутри
        public bool belong(double x, double y) {
            if (x >= h.toDiem(X1) && x <= h.toDiem(X2))
                if (y <= h.toDiem(Y1) && y >= h.toDiem(Y2))
                    return true;
            return false;
        }

        //Нарисовать контур
        public void drawBorder(ref PaintEventArgs e) {
            Rectangle rect1 = new Rectangle(h.getX(X1), h.getY(Y1), h.getWidth(W), h.getHeight(H));
            e.Graphics.DrawRectangle(h.border, rect1);
        }

        //Сделать заливку цвета brush
        public void fill(ref PaintEventArgs e, Brush brush) {
            Rectangle rect1 = new Rectangle(h.getX(X1), h.getY(Y1), h.getWidth(W), h.getHeight(H));
            e.Graphics.FillRectangle(brush, rect1);
        }
    }

    class Ellipse {
        Helper h; //Помощник
        public int X1, Y1; //Координата левой верхней точки
        public int X2, Y2; //Координата правой нижней точки
        int W, H; //Ширина и высота прямоугольника, по которому строится эллипс
        Rect rect; //Прямоугольник
        float A, B; // Для уравнения X^2/A + Y^2/B = 1
        public Point O; //Центр эллипса

        public Ellipse(int X1, int Y1, int W, int H, Helper h) {
            this.h = h;
            this.W = W;
            this.H = H;
            this.X1 = X1;
            this.Y1 = Y1;
            this.X2 = X1 + W;
            this.Y2 = Y1 - H;
            //Строим прямоугольник
            rect = new Rect(X1, Y1, W, H, h);
            //Вычисляем A и B
            this.A = Math.Abs(X2 - X1) / 2;
            this.B = Math.Abs(Y2 - Y1) / 2;
            //Вычилсяем центр эллипса
            O.X = X1 + (X2 - X1) / 2;
            O.Y = Y1 + (Y2 - Y1) / 2;
        }

        //Сделать заливку цвета brush
        public void draw(ref PaintEventArgs e, Brush brush) {
            RectangleF rectEllipse = new RectangleF(h.getX(X1), h.getY(Y1), h.getWidth(W), h.getHeight(H));
            e.Graphics.FillEllipse(brush, rectEllipse);
        }

        //Нарисовать контур 
        public void drawBorder(ref PaintEventArgs e) {
            RectangleF rectEllipse = new RectangleF(h.getX(X1), h.getY(Y1), h.getWidth(W), h.getHeight(H));
            e.Graphics.DrawEllipse(h.border, rectEllipse);
        }

        //Точка лежит внутри эллипса. Переменные x и y до 10^9, т.к. в ином случае
        //происходит переполнение
        public bool belong(double x, double y) {
            if ( Math.Pow(x - h.toDiem(O.X), 2) / Math.Pow(h.toDiem(A), 2) + Math.Pow(y - h.toDiem(O.Y), 2) / Math.Pow(h.toDiem(B), 2) < 1 )
                return true;
            else
                return false;
        }

        //Принадлежит контуру эллипса
        public bool belongBorder(double x, double y) {
            if (Math.Pow(x - h.toDiem(O.X), 2) / Math.Pow(h.toDiem(A), 2) + Math.Pow(y - h.toDiem(O.Y), 2) / Math.Pow(h.toDiem(B), 2) == 1)
                return true;
            else
                return false;
        }

        //Принадлежит прямоугольнику, по которому был построен эллипс
        public bool belongRect(double x, double y) {
            if (rect.belong(x, y))
                return true;
            else
                return false;
        }

        //Эллипс задаётся прямоугольником.
        //Прямоугольник бьётся на 4 четверти как в геометрии

        //Точка принадлежит 1 четверти эллипса
        public bool belongQuarter1(double x, double y) {
            if (x >= h.toDiem(O.X) && x <= h.toDiem(X2) && y >= h.toDiem(O.Y) && y <= h.toDiem(Y1)) 
                return true;
            else
                return false;
        }

        //Точка принадлежит 2 четверти эллписа
        public bool belongQuarter2(double x, double y) {
            if (x >= h.toDiem(X1) && x <= h.toDiem(O.X) && y >= h.toDiem(O.Y) && y <= h.toDiem(Y1))
                return true;
            else
                return false;
        }

        //Точка принадлежит 3 четверти эллипса
        public bool belongQuarter3(double x, double y) {
            if (x >= h.toDiem(X1) && x <= h.toDiem(O.X) && y >= h.toDiem(Y2) && y <= h.toDiem(O.Y))
                return true;
            else
                return false;
        }

        //Точка принадлежит 4 четверти эллписа
        public bool belongQuarter4(double x, double y) {
            if (x >= h.toDiem(O.X) && x <= h.toDiem(X2) && y >= h.toDiem(Y2) && y <= h.toDiem(O.Y))
                return true;
            else
                return false;
        }
    }
}



