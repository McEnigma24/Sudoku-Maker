using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Sudoku_Maker
{
    public partial class Start : Form
    {
        /*[DllImport(@"test_exe.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Make_Sudoku();*/

        /*        [DllImport(@"test_exe.dll", CallingConvention = CallingConvention.Cdecl)]
                public static extern void Save_Matrix_to_File();*/

        public System.Drawing.Image[] numbers;
        public System.Drawing.Image imageBackground;
        public System.Drawing.Image blank;
        public System.Drawing.Image background_pdf;
        public bool[,] blank_matrix;

        public System.Drawing.Image[] solutions_tab;
        public System.Drawing.Image[] sudokus_tab;

        public int[,] sudoku_matrix;
        public int[,] pattern_for_sudoku_matrix;
        public int[,,] coordinate_matrix;
        public List<int>[] available_numbers_row;
        public List<int>[] available_numbers_column;
        public List<int>[] available_numbers_group;

        public Random r;

        public int[,] color_pattern_matrix;
        public Color[] color_tab;
        public List<Color> color_list;
        public int current_chosen_paint;

        public Button[,] buttons_matrix;
        public Button[] paint_buttons_tab;
        public int[] paint_buttons_limits_tab;
        public List<int> black_list_for_color_limits;

        CancellationTokenSource cts = new CancellationTokenSource();
        bool sudoku_process_running = false;

        // przerobić żeby nie się nie podawało procent tylko ilość pozostawionych liczb
        public Start()
        {
            InitializeComponent();

            textBox_file_path.Text = @"C:\Users\Admin\Desktop";

            // Test list
            {
                /*
                Coordinet obj1 = new Coordinet(1, 1);
                Coordinet obj2 = new Coordinet(1, 1);
                Coordinet obj3 = new Coordinet(1, 0);

                List<Coordinet> lista = new List<Coordinet>();
                lista.Add(obj1);

                if(lista.Any(obj => obj.First() == 1 && obj.Second() == 1)) check.Text = "jest\n";

                if (obj1 == obj2) check.Text = "same\n";
                if (obj1 == obj3) check.Text = "same\n";
                */
            }


            // Jeszcze tworzenie tej customowego patternu

            // Załadowanie potrzebnych obrazków
            {
                imageBackground = System.Drawing.Image.FromFile("Assets for Sudoku/sudoku template.jpg");
                blank = System.Drawing.Image.FromFile("Assets for Sudoku/blank.jpg");
                background_pdf = System.Drawing.Image.FromFile("Assets for Sudoku/background_for_pdf.jpg");

                string file_path = "Assets for Sudoku/N_for_Sudoku/number_1.jpg";
                numbers = new System.Drawing.Image[9];

                for (int i = 0; i < 9; i++)
                {
                    //check.Text += ("" + file_path + "\n");
                    numbers[i] = System.Drawing.Image.FromFile(file_path);
                    file_path = file_path.Replace((char)(i + 49), (char)(i + 49 + 1));
                }
            }
            // Show -  sudoku_matrix
            {
                /*
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        check.Text += ("" + sudoku_matrix[i, j] + " ");
                    }
                    check.Text += "\n";
                }
                */
            }

            // Potrzene zmienne
            coordinate_matrix = new int[9, 9, 2];
            sudoku_matrix = new int[10, 10];
            pattern_for_sudoku_matrix = new int[10, 10];
            blank_matrix = new bool[9, 9];
            r = new Random();
            label_exeptions.Text = "";

            // ta na prawdę 6, a nie 7 albo coś nie działało przy usuwaniu
            solutions_tab = new System.Drawing.Image[7];
            sudokus_tab = new System.Drawing.Image[7];

            // alokacja list do tworzenia sudoku
            available_numbers_row = new List<int>[9]; for (int i = 0; i < 9; i++) available_numbers_row[i] = new List<int>();
            available_numbers_column = new List<int>[9]; for (int i = 0; i < 9; i++) available_numbers_column[i] = new List<int>();
            available_numbers_group = new List<int>[9]; for (int i = 0; i < 9; i++) available_numbers_group[i] = new List<int>();

            // na bierząco będzie zaznaczał jakie jest ustawienie patternu
            color_pattern_matrix = new int[10, 10]; for (int i = 0; i < 9; i++) for (int j = 0; j < 9; j++) color_pattern_matrix[i, j] = -1;

            // zapisuje kolory do tablicy i listy                <--- tylko tutaj jak chcesz zmienić kolory
            {
                color_tab = new Color[9];
                color_tab[0] = Color.Black;
                color_tab[1] = Color.Blue;
                color_tab[2] = Color.Red;
                color_tab[3] = Color.Green;
                color_tab[4] = Color.DarkViolet;
                color_tab[5] = Color.Yellow;
                color_tab[6] = Color.Brown;
                color_tab[7] = Color.SteelBlue;
                color_tab[8] = Color.White;

                color_list = new List<Color>();
                for (int i = 0; i < 9; i++) color_list.Add(color_tab[i]);
            }

            // ustawia Paints żeby pokazywały kolory
            {
                paints_1.BackColor = color_tab[0];
                paints_2.BackColor = color_tab[1];
                paints_3.BackColor = color_tab[2];
                paints_4.BackColor = color_tab[3];
                paints_5.BackColor = color_tab[4];
                paints_6.BackColor = color_tab[5];
                paints_7.BackColor = color_tab[6];
                paints_8.BackColor = color_tab[7];
                paints_9.BackColor = color_tab[8];
            }

            current_chosen_paint = -1;

            // button tab
            {
                buttons_matrix = new Button[10, 10];
                Button btn;
                string button_name = "";
                int x = 1;

                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        button_name = "b" + x.ToString();
                        btn = (Button)this.Controls[button_name];
                        buttons_matrix[i, j] = btn;
                        x++;
                    }
                }
            }

            // paint buttons
            {
                Button btn;
                paint_buttons_tab = new Button[9];
                string button_name = "";

                for (int x = 0; x < 9; x++)
                {
                    button_name = "paints_" + (x + 1).ToString();
                    btn = (Button)this.Controls[button_name];
                    paint_buttons_tab[x] = btn;
                    x++;
                }
                paint_buttons_limits_tab = new int[9]; for (int i = 0; i < 9; i++) paint_buttons_limits_tab[i] = 0;
            }

            // black_list
            black_list_for_color_limits = new List<int>();

            //Generate_Sudoku();
            // wskazówki co do obsługi
            {
                // jak chcesz zmienić rozstawienie tych sudoku na stronie
                // należy zmodyfikować:
                // 1. rozmiar dokumentu pdf
                // 2. odstępy pomiędzy obrazami w pliku jpg
                // 3. rozmiar backgroundu na który naklejane są zdjęcia
                // (bardzo mało prawdopodobe, bo jest bardzo duży, a i tak zawsze przycianany do rozmiarów wyznaczanych podczas tworzenia combined.jpg)
            }
        }

        // ##############################
        //
        // zrobić tą modyfikację z x ( tym co jest ta sama liczba na środku ) <---> pokolorować tylko ten template na który naklejam, albo lepiej zrobić zbiór pokolorowanych liczb to i tak przysłaniają całą przestrzeń
        // w sumie to wtedy tylko dodatkowe dwie listy, jedna na liczby możliwe dla jednej przekątnej, druga dla drugiej i naura, na podstawie współrzędnych sprawdza na którą listę ma patrzeć



        private void Another_Iteration(object sender, ProgressClass e)
        {
            progressBar_finish_sudoku.Value = e.Precentage;
        }
        private async void begin_Click(object sender, EventArgs e)
        {            
            if (sudoku_process_running == false)
            {
                begin.Text = "Generate Sudoku";               
                sudoku_process_running = true;
                if ((checkBox_custom_pattern.Checked == true) && (Is_Pattern_Chosen_Correctly() == false))
                {
                    sudoku_process_running = false;
                    return;
                }
                label_exeptions.Text = "";

                Progress<ProgressClass> progress = new Progress<ProgressClass>();
                progress.ProgressChanged += Another_Iteration;

                try
                {                    
                    await Task.Run(() => Main_Thing(progress, cts.Token));
                    sudoku_process_running = false;
                }
                catch (OperationCanceledException)
                {
                    begin.Text = "cancelled";

                    cts.Dispose();
                    cts = new CancellationTokenSource();                    
                }                
            }
            else
            {
                sudoku_process_running = false;
                cts.Cancel();
                begin.Text = "Generate Sudoku";
            }

            // Thread.Sleep(200);
        }
        public async Task Main_Thing(IProgress<ProgressClass> progress, CancellationToken cancellation_token)
        {
            ProgressClass progress_tracker = new ProgressClass();

            // używane zmienne
            string sudoku = "";
            string solution = "";
            int how_many_pages = 1;
            int how_many_percent = 50;
            double tmp = 0;
            int how_many_blanks = 0;

            // Ile kartek
            {
                if (textBox_liczba_stron.Text != "" && int.TryParse(textBox_liczba_stron.Text, out int c))
                {
                    how_many_pages = int.Parse(textBox_liczba_stron.Text);
                }
            }
            // Poziom trudności
            {
                if (hardness_level.Text != "" && int.TryParse(hardness_level.Text, out int b))
                {
                    how_many_percent = int.Parse(hardness_level.Text);
                }
            }
            // Ile blanków ma wstawić
            {
                tmp = (double)how_many_percent / 100;
                how_many_blanks = (int)(((double)81) * tmp);
            }


            // Gdzieś można jeszcze wstawić numerację stron, albo na samym końcu

            for (int big_iterator = 0; big_iterator < how_many_pages; big_iterator++)
            {
                // tworzy jpg - 6 sudoku i 6 solution                
                {
                    // tworzy solutions                     
                    for (int x = 0; x < 6; x++)
                    {
                        // if (cancellation_token.IsCancellationRequested) cancellation_token.ThrowIfCancellationRequested();

                        // wywołania c++
                        {
                            //Make_Sudoku();
                            //Save_Matrix_to_File();

                            // wywołanie c++ .exe - strasznie wolno ---> nawet Parallel procesy nie są tu przydatne
                            /*{
                                ProcessStartInfo startInfo = new ProcessStartInfo();
                                startInfo.FileName = @"Assets for Sudoku\Exe_Files\test_exe.exe";

                                startInfo.CreateNoWindow = true;
                                startInfo.UseShellExecute = true;
                                //startInfo.UseShellExecute = false;
                                startInfo.WindowStyle = ProcessWindowStyle.Hidden;                            

                                //startInfo.Arguments = "";

                                using (Process create_matrix_exe = Process.Start(startInfo))
                                {
                                    create_matrix_exe.WaitForExit();
                                }

                                //Process p = new Process();
                                //p.StartInfo.FileName = @"Assets for Sudoku\Exe_Files\test_exe.exe";
                                //p.Start();
                                //p.WaitForExit();
                            }*/
                        }

                        // Tworzenie macierzy do sudoku C# bo c++ nie chce współpracować
                        Generate_Sudoku();
                        // od razu wpisuje do właściwej macierzy

                        // Relict
                        // Załadowanie macierzy utworzonej przez C++
                        {
                            /*
                            string input = File.ReadAllText(@"Assets for Sudoku\Exe_Files\matrix.txt");

                            int i = 0, j = 0;
                            foreach (char a in input)
                            {
                                if (a == '\n')
                                {
                                    i++;
                                    j = 0;
                                }
                                else if (a != ' ')
                                {
                                    sudoku_matrix[i, j] = (int)(a) - 48;
                                    j++;
                                }
                            }
                            */
                        }

                        // wkleja numery we właściwe miejsca -> tworzy solutions.jpg
                        {
                            System.Drawing.Image solution_jpg = new Bitmap(imageBackground.Width, imageBackground.Height);
                            using (Graphics gr = Graphics.FromImage(solution_jpg))
                            {
                                gr.DrawImage(imageBackground, new Point(0, 0));

                                // wcześniejsze
                                {
                                    // wcześniejsze
                                    {
                                        /*
                                        int adding_heigth = 0;
                                        for (int i = 0; i < 3; i++)
                                        {
                                            gr.DrawImage(numbers[0], new Point(3, 3 + adding_heigth));
                                            gr.DrawImage(numbers[1], new Point(3 + 65, 3 + adding_heigth));
                                            gr.DrawImage(numbers[2], new Point(3 + 65 * 2 + 1, 3 + adding_heigth));

                                            gr.DrawImage(numbers[3], new Point(3 + 65 * 3 + 4, 3 + adding_heigth));
                                            gr.DrawImage(numbers[4], new Point(3 + 65 * 4 + 5, 3 + adding_heigth));
                                            gr.DrawImage(numbers[5], new Point(3 + 65 * 5 + 6, 3 + adding_heigth));

                                            gr.DrawImage(numbers[6], new Point(3 + 65 * 6 + 9, 3 + adding_heigth));
                                            gr.DrawImage(numbers[7], new Point(3 + 65 * 7 + 10, 3 + adding_heigth));
                                            gr.DrawImage(numbers[7], new Point(3 + 65 * 8 + 14, 3 + adding_heigth));

                                            adding_heigth += 65 + i;                    
                                        }


                                        adding_heigth++;
                                        for (int i = 0; i < 3; i++)
                                        {
                                            gr.DrawImage(numbers[0], new Point(3, 3 + adding_heigth));
                                            gr.DrawImage(numbers[1], new Point(3 + 65, 3 + adding_heigth));
                                            gr.DrawImage(numbers[2], new Point(3 + 65 * 2 + 1, 3 + adding_heigth));

                                            gr.DrawImage(numbers[3], new Point(3 + 65 * 3 + 4, 3 + adding_heigth));
                                            gr.DrawImage(numbers[4], new Point(3 + 65 * 4 + 5, 3 + adding_heigth));
                                            gr.DrawImage(numbers[5], new Point(3 + 65 * 5 + 6, 3 + adding_heigth));

                                            gr.DrawImage(numbers[6], new Point(3 + 65 * 6 + 9, 3 + adding_heigth));
                                            gr.DrawImage(numbers[7], new Point(3 + 65 * 7 + 10, 3 + adding_heigth));
                                            gr.DrawImage(numbers[8], new Point(3 + 65 * 8 + 14, 3 + adding_heigth));

                                            adding_heigth += 65 + i;
                                            adding_heigth++;
                                        }

                                        adding_heigth--;
                                        for (int i = 0; i < 2; i++)
                                        {
                                            gr.DrawImage(numbers[0], new Point(3, 3 + adding_heigth));
                                            gr.DrawImage(numbers[1], new Point(3 + 65, 3 + adding_heigth));
                                            gr.DrawImage(numbers[2], new Point(3 + 65 * 2 + 1, 3 + adding_heigth));

                                            gr.DrawImage(numbers[3], new Point(3 + 65 * 3 + 4, 3 + adding_heigth));
                                            gr.DrawImage(numbers[4], new Point(3 + 65 * 4 + 5, 3 + adding_heigth));
                                            gr.DrawImage(numbers[5], new Point(3 + 65 * 5 + 6, 3 + adding_heigth));

                                            gr.DrawImage(numbers[6], new Point(3 + 65 * 6 + 9, 3 + adding_heigth));
                                            gr.DrawImage(numbers[7], new Point(3 + 65 * 7 + 10, 3 + adding_heigth));
                                            gr.DrawImage(numbers[7], new Point(3 + 65 * 8 + 14, 3 + adding_heigth));

                                            adding_heigth += 65 + i;
                                            adding_heigth++;                   
                                        }

                                        // ostatni rządek
                                        //adding_heigth-=2;
                                        gr.DrawImage(numbers[0], new Point(3, 3 + adding_heigth));
                                        gr.DrawImage(numbers[1], new Point(3 + 65, 3 + adding_heigth));
                                        gr.DrawImage(numbers[2], new Point(3 + 65 * 2 + 1, 3 + adding_heigth));

                                        gr.DrawImage(numbers[3], new Point(3 + 65 * 3 + 4, 3 + adding_heigth));
                                        gr.DrawImage(numbers[4], new Point(3 + 65 * 4 + 5, 3 + adding_heigth));
                                        gr.DrawImage(numbers[5], new Point(3 + 65 * 5 + 6, 3 + adding_heigth));

                                        gr.DrawImage(numbers[6], new Point(3 + 65 * 6 + 9, 3 + adding_heigth));
                                        gr.DrawImage(numbers[7], new Point(3 + 65 * 7 + 10, 3 + adding_heigth));
                                        gr.DrawImage(numbers[8], new Point(3 + 65 * 8 + 14, 3 + adding_heigth));

                                        */
                                    }
                                    // wcześniejsza koncepcja
                                    {
                                        /*
                                        for(int i = 0; i < 9; i++)
                                        {
                                            for (int j = 0; j < 9; j++)
                                            {
                                                // sudoku_matrix[i,j]-1 jaka liczba odczytana z macierzy
                                                gr.DrawImage(numbers[sudoku_matrix[i,j]-1], new Point(co_ordinet_matrix[i, j, 0], co_ordinet_matrix[i, j, 1]));
                                            }
                                        }
                                        */
                                    }
                                }
                                // Ustawianie numerów na właściwe miejsca
                                {
                                    int ii = 0, jj = 0;
                                    int index_i = 0;

                                    int adding_heigth = 0;
                                    for (int i = 0; i < 3; i++)
                                    {
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 2 + 1, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 2 + 1; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;

                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 3 + 4, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 3 + 4; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 4 + 5, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 4 + 5; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 5 + 6, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 5 + 6; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;

                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 6 + 9, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 6 + 9; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 7 + 10, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 7 + 10; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 8 + 14, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 8 + 14; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        index_i++;

                                        ii++;
                                        jj = 0;

                                        adding_heigth += 65 + i;
                                    }


                                    adding_heigth++;
                                    for (int i = 0; i < 3; i++)
                                    {
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 2 + 1, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 2 + 1; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;

                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 3 + 4, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 3 + 4; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 4 + 5, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 4 + 5; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 5 + 6, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 5 + 6; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;

                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 6 + 9, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 6 + 9; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 7 + 10, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 7 + 10; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 8 + 14, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 8 + 14; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        index_i++;

                                        ii++;
                                        jj = 0;

                                        adding_heigth += 65 + i;
                                        adding_heigth++;
                                    }

                                    adding_heigth--;
                                    for (int i = 0; i < 2; i++)
                                    {
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 2 + 1, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 2 + 1; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;

                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 3 + 4, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 3 + 4; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 4 + 5, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 4 + 5; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 5 + 6, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 5 + 6; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;

                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 6 + 9, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 6 + 9; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 7 + 10, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 7 + 10; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 8 + 14, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 8 + 14; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        index_i++;

                                        ii++;
                                        jj = 0;

                                        adding_heigth += 65 + i;
                                        adding_heigth++;
                                    }

                                    // ostatni rządek
                                    //adding_heigth-=2;
                                    gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3, 3 + adding_heigth)); jj++; coordinate_matrix[8, 0, 0] = 3; coordinate_matrix[8, 0, 1] = 3 + adding_heigth;
                                    gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65, 3 + adding_heigth)); jj++; coordinate_matrix[8, 1, 0] = 3 + 65; coordinate_matrix[8, 1, 1] = 3 + adding_heigth;
                                    gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 2 + 1, 3 + adding_heigth)); jj++; coordinate_matrix[8, 2, 0] = 3 + 65 * 2 + 1; coordinate_matrix[8, 2, 1] = 3 + adding_heigth;

                                    gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 3 + 4, 3 + adding_heigth)); jj++; coordinate_matrix[8, 3, 0] = 3 + 65 * 3 + 4; coordinate_matrix[8, 3, 1] = 3 + adding_heigth;
                                    gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 4 + 5, 3 + adding_heigth)); jj++; coordinate_matrix[8, 4, 0] = 3 + 65 * 4 + 5; coordinate_matrix[8, 4, 1] = 3 + adding_heigth;
                                    gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 5 + 6, 3 + adding_heigth)); jj++; coordinate_matrix[8, 5, 0] = 3 + 65 * 5 + 6; coordinate_matrix[8, 5, 1] = 3 + adding_heigth;

                                    gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 6 + 9, 3 + adding_heigth)); jj++; coordinate_matrix[8, 6, 0] = 3 + 65 * 6 + 9; coordinate_matrix[8, 6, 1] = 3 + adding_heigth;
                                    gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 7 + 10, 3 + adding_heigth)); jj++; coordinate_matrix[8, 7, 0] = 3 + 65 * 7 + 10; coordinate_matrix[8, 7, 1] = 3 + adding_heigth;
                                    gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 8 + 14, 3 + adding_heigth)); jj++; coordinate_matrix[8, 8, 0] = 3 + 65 * 8 + 14; coordinate_matrix[8, 8, 1] = 3 + adding_heigth;
                                }


                                // Macierz współrzędnych dla tego wszytkiego

                                // i teraz tylko, żeby przeszedł przez macierz zapisaną w pliku txt (stworzoną przez c++)
                                // do macierzy ją przepisze, i potem będzie badał i na tej podstawie będzie brał z tablicy ImageOverlay
                                // gdzie będą otwarte wszystkie numbery i będzie patrzył na to w jakim miejscu teraz powinien wpisać na podstawie
                                // macierzy współrzędnych

                                // niech najpierw zrobi solution, a potem na jego podstawie
                                // w losowych miejscach pododaje blank spaces i też będzie używał macierzy współrzędnych
                                // i essa
                            }
                            solution = "solution_" + (x + 1).ToString() + ".jpg";
                            solution_jpg.Save(solution, ImageFormat.Jpeg);
                            solution_jpg.Dispose();

                            solutions_tab[x] = System.Drawing.Image.FromFile(solution);
                        }                        
                    }

                    // tworzy sudoku --- wstawia blank spoty
                    for (int x = 0; x < 6; x++)
                    {
                        // if (cancellation_token.IsCancellationRequested) cancellation_token.ThrowIfCancellationRequested();
                        for (int i = 0; i < 9; i++) for (int j = 0; j < 9; j++) blank_matrix[i, j] = false;

                        int random_i = 0, random_j = 0;
                        for (int i = 0; i < how_many_blanks; i++)
                        {                            
                            random_i = r.Next(0, 9); random_j = r.Next(0, 9);

                            while (blank_matrix[random_i, random_j] == true)
                            {
                                // if (cancellation_token.IsCancellationRequested) cancellation_token.ThrowIfCancellationRequested();
                                random_i = r.Next(0, 9); random_j = r.Next(0, 9);
                            }

                            blank_matrix[random_i, random_j] = true;
                        }

                        System.Drawing.Image sudoku_jpg = new Bitmap(solutions_tab[x].Width, solutions_tab[x].Height);
                        using (Graphics gr = Graphics.FromImage(sudoku_jpg))
                        {
                            gr.DrawImage(solutions_tab[x], new Point(0, 0));

                            for (int i = 0; i < 9; i++)
                            {
                                for (int j = 0; j < 9; j++)
                                {
                                    // if (cancellation_token.IsCancellationRequested) cancellation_token.ThrowIfCancellationRequested();
                                    // sudoku_matrix[i,j]-1 jaka liczba odczytana z macierzy
                                    //random = r.Next(1, 100);
                                    //if (how_many_percent <= random)

                                    if (!blank_matrix[i, j] == true)
                                    {
                                        gr.DrawImage(blank, new Point(coordinate_matrix[i, j, 0], coordinate_matrix[i, j, 1]));
                                    }
                                }
                            }
                        }
                        sudoku = "sudoku_" + (x + 1).ToString() + ".jpg";
                        sudoku_jpg.Save(sudoku, ImageFormat.Jpeg);
                        sudoku_jpg.Dispose();

                        sudokus_tab[x] = System.Drawing.Image.FromFile(sudoku);
                    }
                }

                // sklejanie jpg - 1 combined_sudoku i 1 combined_solution              
                {
                    int odstep_poziomo = 150;
                    int odstep_pionowo = 150;

                    int starting_point_x = 50;
                    int starting_point_y = 50;

                    // tworzenie jpg z wszystkimi sudoku                                 poprawne współrzędne 1208, 1806
                    System.Drawing.Image combined_sudoku_jpg = new Bitmap(1208 + (2 * odstep_poziomo) + 2 * starting_point_x - 100, 1806 + (2 * odstep_pionowo) + 2 * starting_point_y);
                    using (Graphics gr = Graphics.FromImage(combined_sudoku_jpg))
                    {
                        gr.DrawImage(background_pdf, new Point(0, 0));

                        int x = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                // if (cancellation_token.IsCancellationRequested) cancellation_token.ThrowIfCancellationRequested();

                                // + 0 == odstęp
                                gr.DrawImage(sudokus_tab[x], new Point(starting_point_x + (j * (sudokus_tab[0].Width + odstep_poziomo)),
                                                                       starting_point_y + (i * (sudokus_tab[0].Height + odstep_pionowo))));

                                x++;
                            }
                        }
                    }
                    combined_sudoku_jpg.Save("combined_sudoku.jpg", ImageFormat.Jpeg);
                    combined_sudoku_jpg.Dispose();

                    // tworzenie jpg z wszystkimi solutions
                    System.Drawing.Image combined_solutions_jpg = new Bitmap(1208 + (2 * odstep_poziomo) + 2 * starting_point_x - 100, 1806 + (2 * odstep_pionowo) + 2 * starting_point_y);
                    using (Graphics gr = Graphics.FromImage(combined_solutions_jpg))
                    {
                        gr.DrawImage(background_pdf, new Point(0, 0));

                        int x = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                // if (cancellation_token.IsCancellationRequested) cancellation_token.ThrowIfCancellationRequested();

                                // + 0 == odstęp
                                gr.DrawImage(solutions_tab[x], new Point(starting_point_x + (j * (solutions_tab[0].Width + odstep_poziomo)),
                                                                         starting_point_y + (i * (solutions_tab[0].Height + odstep_pionowo))));

                                x++;
                            }
                        }
                    }
                    combined_solutions_jpg.Save("combined_solutions.jpg", ImageFormat.Jpeg);
                    combined_solutions_jpg.Dispose();
                }

                // jpg -> pdf - 2 strony
                {
                    iTextSharp.text.Document document = new iTextSharp.text.Document(new iTextSharp.text.Rectangle(1684 - 150, 2384 - 100));
                    using (var stream = new FileStream("for_resize.pdf", FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        PdfWriter.GetInstance(document, stream);
                        document.Open();

                        // dodaje jpg z sudoku
                        using (var imageStream = new FileStream("combined_sudoku.jpg", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            var sudokus_var = iTextSharp.text.Image.GetInstance(imageStream);
                            document.Add(sudokus_var);
                        }

                        // dodaje jpg z solutions
                        using (var imageStream = new FileStream("combined_solutions.jpg", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            var solutions_var = iTextSharp.text.Image.GetInstance(imageStream);
                            document.Add(solutions_var);
                        }

                        document.Close();
                    }
                }

                // Przeskalowywanie pdf do rozmiarów A4 - tworzenie tmp_
                {
                    string original = "for_resize.pdf";

                    string inPDF = original;
                    PdfReader pdfr = new PdfReader(inPDF);

                    iTextSharp.text.Document doc = new iTextSharp.text.Document(PageSize.A4);
                    iTextSharp.text.Document.Compress = true;

                    string outPDF = "tmp_" + (big_iterator + 1).ToString() + ".pdf";
                    PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(outPDF, FileMode.Create));

                    doc.Open();
                    PdfContentByte cb = writer.DirectContent;
                    PdfImportedPage page;

                    for (int i = 1; i < pdfr.NumberOfPages + 1; i++)
                    {
                        page = writer.GetImportedPage(pdfr, i);
                        cb.AddTemplate(page, PageSize.A4.Width / pdfr.GetPageSize(i).Width, 0, 0, PageSize.A4.Height / pdfr.GetPageSize(i).Height, 0, 0);
                        doc.NewPage();
                    }
                    doc.Close();

                    pdfr.Dispose();
                    pdfr.Close();
                    File.Delete("for_resize.pdf");
                }

                // usuwanie wszystkich jpg
                {
                    for (int x = 0; x < 6; x++)
                    {
                        solutions_tab[x].Dispose();
                        sudokus_tab[x].Dispose();
                    }

                    for (int x = 0; x < 6; x++)
                    {
                        if (cancellation_token.IsCancellationRequested) cancellation_token.ThrowIfCancellationRequested();

                        solution = "solution_" + (x + 1).ToString() + ".jpg";
                        sudoku = "sudoku_" + (x + 1).ToString() + ".jpg";

                        File.Delete(sudoku);
                        File.Delete(solution);
                    }

                    File.Delete("combined_sudoku.jpg");
                    File.Delete("combined_solutions.jpg");
                }

                // progress

                progress_tracker.Precentage = ((big_iterator + 1) * 100) / (how_many_pages + 4);
                progress.Report(progress_tracker);

                // cancellation
                if (cancellation_token.IsCancellationRequested)
                {
                    // usuwanie wszystkich jpg
                    /*
                    {
                        for (int c = 0; c < 6; c++)
                        {
                            solutions_tab[c].Dispose();
                            sudokus_tab[c].Dispose();
                        }

                        for (int c = 0; c < 6; c++)
                        {
                            solution = "solution_" + (c + 1).ToString() + ".jpg";
                            sudoku = "sudoku_" + (c + 1).ToString() + ".jpg";

                            if (File.Exists(sudoku)) File.Delete(sudoku);
                            if (File.Exists(solution)) File.Delete(solution);
                        }

                        if (File.Exists("combined_sudoku.jpg")) File.Delete("combined_sudoku.jpg");
                        if (File.Exists("combined_solutions.jpg")) File.Delete("combined_solutions.jpg");
                    }
                    */

                    cancellation_token.ThrowIfCancellationRequested();
                }
            }

            // dodajemy wszystkie pliki tmp_1 _2 _3 i tworzymy for_reorder
            {
                string tmp_pdf;
                iTextSharp.text.Document doc_pdf = new iTextSharp.text.Document();
                PdfCopy writer_pdf = new PdfCopy(doc_pdf, new FileStream("for_reorder.pdf", FileMode.Create));

                doc_pdf.Open();
                for (int x = 0; x < how_many_pages; x++)
                {
                    tmp_pdf = "tmp_" + (x + 1).ToString() + ".pdf";

                    PdfReader reader = new PdfReader(tmp_pdf);
                    reader.ConsolidateNamedDestinations();
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        PdfImportedPage page = writer_pdf.GetImportedPage(reader, i);
                        writer_pdf.AddPage(page);
                    }
                    reader.Close();
                }
                writer_pdf.Close();
                doc_pdf.Close();
            }
            progress_tracker.Precentage = ((how_many_pages + 1) * 100) / (how_many_pages + 4); progress.Report(progress_tracker);            
            if (cancellation_token.IsCancellationRequested) cancellation_token.ThrowIfCancellationRequested();

            // zmiana kolejności
            {
                iTextSharp.text.Document doc_pdf = new iTextSharp.text.Document();
                PdfCopy writer_pdf = new PdfCopy(doc_pdf, new FileStream("Final_100%_pure_product.pdf", FileMode.Create));

                doc_pdf.Open();

                PdfReader reader = new PdfReader("for_reorder.pdf");
                reader.ConsolidateNamedDestinations();
                for (int i = 1; i <= reader.NumberOfPages; i += 2)
                {
                    PdfImportedPage page = writer_pdf.GetImportedPage(reader, i);
                    writer_pdf.AddPage(page);
                }

                for (int i = 2; i <= reader.NumberOfPages; i += 2)
                {
                    PdfImportedPage page = writer_pdf.GetImportedPage(reader, i);
                    writer_pdf.AddPage(page);
                }

                reader.Close();

                writer_pdf.Close();
                doc_pdf.Close();
            }
            progress_tracker.Precentage = ((how_many_pages + 2) * 100) / (how_many_pages + 4); progress.Report(progress_tracker);
            if (cancellation_token.IsCancellationRequested) cancellation_token.ThrowIfCancellationRequested();

            // usuwanie wszystkich pdf tmp_ + reorder.pdf
            {
                string path_to_tmp;
                for (int i = 0; i < how_many_pages; i++)
                {
                    path_to_tmp = "tmp_" + (i + 1).ToString() + ".pdf";

                    File.Delete(path_to_tmp);
                }

                File.Delete("for_reorder.pdf");
            }
            progress_tracker.Precentage = ((how_many_pages + 3) * 100) / (how_many_pages + 4); progress.Report(progress_tracker);
            if (cancellation_token.IsCancellationRequested) cancellation_token.ThrowIfCancellationRequested();

            // kopiowanie ostatecznego produktu do wybranej przez użytkowniak ścieżki + nazwa pliku użytkownika
            {
                if (textBox_file_path.Text != "" && Directory.Exists(textBox_file_path.Text))
                {
                    string path = @textBox_file_path.Text;
                    path += @"\";
                    if (textBox_file_name.Text != "")
                    {
                        if (File.Exists(path + textBox_file_name.Text + ".pdf")) File.Delete(path + textBox_file_name.Text + ".pdf");

                        File.Copy("Final_100%_pure_product.pdf", path + textBox_file_name.Text + ".pdf");
                    }
                    else
                    {
                        if (File.Exists(path + "sudoku.pdf")) File.Delete(path + "sudoku.pdf");

                        File.Copy("Final_100%_pure_product.pdf", path + "sudoku.pdf");
                    }
                }
            }
            progress_tracker.Precentage = ((how_many_pages + 4) * 100) / (how_many_pages + 4); progress.Report(progress_tracker);
            if (cancellation_token.IsCancellationRequested) cancellation_token.ThrowIfCancellationRequested();
        }


        // wcześniejsze
        public void Back_up()
        {
            // używane zmienne
            string sudoku = "";
            string solution = "";
            int how_many_pages = 1;
            int how_many_percent = 50;
            double tmp = 0;
            int how_many_blanks = 0;

            // Ile kartek
            {
                if (textBox_liczba_stron.Text != "" && int.TryParse(textBox_liczba_stron.Text, out int c))
                {
                    how_many_pages = int.Parse(textBox_liczba_stron.Text);
                }
            }
            // Poziom trudności
            {
                if (hardness_level.Text != "" && int.TryParse(hardness_level.Text, out int b))
                {
                    how_many_percent = int.Parse(hardness_level.Text);
                }
            }
            // Ile blanków ma wstawić
            {
                tmp = (double)how_many_percent / 100;
                how_many_blanks = (int)(((double)81) * tmp);
            }


            // Gdzieś można jeszcze wstawić numerację stron, albo na samym końcu

            for (int big_iterator = 0; big_iterator < how_many_pages; big_iterator++)
            {

                // tworzy jpg - 6 sudoku i 6 solution                
                {
                    // tworzy solutions                     
                    for (int x = 0; x < 6; x++)
                    {
                        // wywołania c++
                        {
                            //Make_Sudoku();
                            //Save_Matrix_to_File();

                            // wywołanie c++ .exe - strasznie wolno ---> nawet Parallel procesy nie są tu przydatne
                            /*{
                                ProcessStartInfo startInfo = new ProcessStartInfo();
                                startInfo.FileName = @"Assets for Sudoku\Exe_Files\test_exe.exe";

                                startInfo.CreateNoWindow = true;
                                startInfo.UseShellExecute = true;
                                //startInfo.UseShellExecute = false;
                                startInfo.WindowStyle = ProcessWindowStyle.Hidden;                            

                                //startInfo.Arguments = "";

                                using (Process create_matrix_exe = Process.Start(startInfo))
                                {
                                    create_matrix_exe.WaitForExit();
                                }

                                //Process p = new Process();
                                //p.StartInfo.FileName = @"Assets for Sudoku\Exe_Files\test_exe.exe";
                                //p.Start();
                                //p.WaitForExit();
                            }*/
                        }

                        // Tworzenie macierzy do sudoku C# bo c++ nie chce współpracować
                        Generate_Sudoku();
                        // od razu wpisuje do właściwej macierzy

                        // Relict
                        // Załadowanie macierzy utworzonej przez C++
                        {
                            /*
                            string input = File.ReadAllText(@"Assets for Sudoku\Exe_Files\matrix.txt");

                            int i = 0, j = 0;
                            foreach (char a in input)
                            {
                                if (a == '\n')
                                {
                                    i++;
                                    j = 0;
                                }
                                else if (a != ' ')
                                {
                                    sudoku_matrix[i, j] = (int)(a) - 48;
                                    j++;
                                }
                            }
                            */
                        }

                        // wkleja numery we właściwe miejsca -> tworzy solutions.jpg
                        {
                            System.Drawing.Image solution_jpg = new Bitmap(imageBackground.Width, imageBackground.Height);
                            using (Graphics gr = Graphics.FromImage(solution_jpg))
                            {
                                gr.DrawImage(imageBackground, new Point(0, 0));

                                // wcześniejsze
                                {
                                    // wcześniejsze
                                    {
                                        /*
                                        int adding_heigth = 0;
                                        for (int i = 0; i < 3; i++)
                                        {
                                            gr.DrawImage(numbers[0], new Point(3, 3 + adding_heigth));
                                            gr.DrawImage(numbers[1], new Point(3 + 65, 3 + adding_heigth));
                                            gr.DrawImage(numbers[2], new Point(3 + 65 * 2 + 1, 3 + adding_heigth));

                                            gr.DrawImage(numbers[3], new Point(3 + 65 * 3 + 4, 3 + adding_heigth));
                                            gr.DrawImage(numbers[4], new Point(3 + 65 * 4 + 5, 3 + adding_heigth));
                                            gr.DrawImage(numbers[5], new Point(3 + 65 * 5 + 6, 3 + adding_heigth));

                                            gr.DrawImage(numbers[6], new Point(3 + 65 * 6 + 9, 3 + adding_heigth));
                                            gr.DrawImage(numbers[7], new Point(3 + 65 * 7 + 10, 3 + adding_heigth));
                                            gr.DrawImage(numbers[7], new Point(3 + 65 * 8 + 14, 3 + adding_heigth));

                                            adding_heigth += 65 + i;                    
                                        }


                                        adding_heigth++;
                                        for (int i = 0; i < 3; i++)
                                        {
                                            gr.DrawImage(numbers[0], new Point(3, 3 + adding_heigth));
                                            gr.DrawImage(numbers[1], new Point(3 + 65, 3 + adding_heigth));
                                            gr.DrawImage(numbers[2], new Point(3 + 65 * 2 + 1, 3 + adding_heigth));

                                            gr.DrawImage(numbers[3], new Point(3 + 65 * 3 + 4, 3 + adding_heigth));
                                            gr.DrawImage(numbers[4], new Point(3 + 65 * 4 + 5, 3 + adding_heigth));
                                            gr.DrawImage(numbers[5], new Point(3 + 65 * 5 + 6, 3 + adding_heigth));

                                            gr.DrawImage(numbers[6], new Point(3 + 65 * 6 + 9, 3 + adding_heigth));
                                            gr.DrawImage(numbers[7], new Point(3 + 65 * 7 + 10, 3 + adding_heigth));
                                            gr.DrawImage(numbers[8], new Point(3 + 65 * 8 + 14, 3 + adding_heigth));

                                            adding_heigth += 65 + i;
                                            adding_heigth++;
                                        }

                                        adding_heigth--;
                                        for (int i = 0; i < 2; i++)
                                        {
                                            gr.DrawImage(numbers[0], new Point(3, 3 + adding_heigth));
                                            gr.DrawImage(numbers[1], new Point(3 + 65, 3 + adding_heigth));
                                            gr.DrawImage(numbers[2], new Point(3 + 65 * 2 + 1, 3 + adding_heigth));

                                            gr.DrawImage(numbers[3], new Point(3 + 65 * 3 + 4, 3 + adding_heigth));
                                            gr.DrawImage(numbers[4], new Point(3 + 65 * 4 + 5, 3 + adding_heigth));
                                            gr.DrawImage(numbers[5], new Point(3 + 65 * 5 + 6, 3 + adding_heigth));

                                            gr.DrawImage(numbers[6], new Point(3 + 65 * 6 + 9, 3 + adding_heigth));
                                            gr.DrawImage(numbers[7], new Point(3 + 65 * 7 + 10, 3 + adding_heigth));
                                            gr.DrawImage(numbers[7], new Point(3 + 65 * 8 + 14, 3 + adding_heigth));

                                            adding_heigth += 65 + i;
                                            adding_heigth++;                   
                                        }

                                        // ostatni rządek
                                        //adding_heigth-=2;
                                        gr.DrawImage(numbers[0], new Point(3, 3 + adding_heigth));
                                        gr.DrawImage(numbers[1], new Point(3 + 65, 3 + adding_heigth));
                                        gr.DrawImage(numbers[2], new Point(3 + 65 * 2 + 1, 3 + adding_heigth));

                                        gr.DrawImage(numbers[3], new Point(3 + 65 * 3 + 4, 3 + adding_heigth));
                                        gr.DrawImage(numbers[4], new Point(3 + 65 * 4 + 5, 3 + adding_heigth));
                                        gr.DrawImage(numbers[5], new Point(3 + 65 * 5 + 6, 3 + adding_heigth));

                                        gr.DrawImage(numbers[6], new Point(3 + 65 * 6 + 9, 3 + adding_heigth));
                                        gr.DrawImage(numbers[7], new Point(3 + 65 * 7 + 10, 3 + adding_heigth));
                                        gr.DrawImage(numbers[8], new Point(3 + 65 * 8 + 14, 3 + adding_heigth));

                                        */
                                    }
                                    // wcześniejsza koncepcja
                                    {
                                        /*
                                        for(int i = 0; i < 9; i++)
                                        {
                                            for (int j = 0; j < 9; j++)
                                            {
                                                // sudoku_matrix[i,j]-1 jaka liczba odczytana z macierzy
                                                gr.DrawImage(numbers[sudoku_matrix[i,j]-1], new Point(co_ordinet_matrix[i, j, 0], co_ordinet_matrix[i, j, 1]));
                                            }
                                        }
                                        */
                                    }
                                }
                                // Ustawianie numerów na właściwe miejsca
                                {
                                    int ii = 0, jj = 0;
                                    int index_i = 0;

                                    int adding_heigth = 0;
                                    for (int i = 0; i < 3; i++)
                                    {
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 2 + 1, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 2 + 1; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;

                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 3 + 4, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 3 + 4; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 4 + 5, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 4 + 5; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 5 + 6, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 5 + 6; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;

                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 6 + 9, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 6 + 9; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 7 + 10, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 7 + 10; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 8 + 14, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 8 + 14; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        index_i++;

                                        ii++;
                                        jj = 0;

                                        adding_heigth += 65 + i;
                                    }


                                    adding_heigth++;
                                    for (int i = 0; i < 3; i++)
                                    {
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 2 + 1, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 2 + 1; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;

                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 3 + 4, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 3 + 4; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 4 + 5, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 4 + 5; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 5 + 6, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 5 + 6; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;

                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 6 + 9, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 6 + 9; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 7 + 10, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 7 + 10; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 8 + 14, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 8 + 14; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        index_i++;

                                        ii++;
                                        jj = 0;

                                        adding_heigth += 65 + i;
                                        adding_heigth++;
                                    }

                                    adding_heigth--;
                                    for (int i = 0; i < 2; i++)
                                    {
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 2 + 1, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 2 + 1; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;

                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 3 + 4, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 3 + 4; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 4 + 5, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 4 + 5; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 5 + 6, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 5 + 6; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;

                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 6 + 9, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 6 + 9; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 7 + 10, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 7 + 10; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 8 + 14, 3 + adding_heigth)); jj++; coordinate_matrix[index_i, jj - 1, 0] = 3 + 65 * 8 + 14; coordinate_matrix[index_i, jj - 1, 1] = 3 + adding_heigth;
                                        index_i++;

                                        ii++;
                                        jj = 0;

                                        adding_heigth += 65 + i;
                                        adding_heigth++;
                                    }

                                    // ostatni rządek
                                    //adding_heigth-=2;
                                    gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3, 3 + adding_heigth)); jj++; coordinate_matrix[8, 0, 0] = 3; coordinate_matrix[8, 0, 1] = 3 + adding_heigth;
                                    gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65, 3 + adding_heigth)); jj++; coordinate_matrix[8, 1, 0] = 3 + 65; coordinate_matrix[8, 1, 1] = 3 + adding_heigth;
                                    gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 2 + 1, 3 + adding_heigth)); jj++; coordinate_matrix[8, 2, 0] = 3 + 65 * 2 + 1; coordinate_matrix[8, 2, 1] = 3 + adding_heigth;

                                    gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 3 + 4, 3 + adding_heigth)); jj++; coordinate_matrix[8, 3, 0] = 3 + 65 * 3 + 4; coordinate_matrix[8, 3, 1] = 3 + adding_heigth;
                                    gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 4 + 5, 3 + adding_heigth)); jj++; coordinate_matrix[8, 4, 0] = 3 + 65 * 4 + 5; coordinate_matrix[8, 4, 1] = 3 + adding_heigth;
                                    gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 5 + 6, 3 + adding_heigth)); jj++; coordinate_matrix[8, 5, 0] = 3 + 65 * 5 + 6; coordinate_matrix[8, 5, 1] = 3 + adding_heigth;

                                    gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 6 + 9, 3 + adding_heigth)); jj++; coordinate_matrix[8, 6, 0] = 3 + 65 * 6 + 9; coordinate_matrix[8, 6, 1] = 3 + adding_heigth;
                                    gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 7 + 10, 3 + adding_heigth)); jj++; coordinate_matrix[8, 7, 0] = 3 + 65 * 7 + 10; coordinate_matrix[8, 7, 1] = 3 + adding_heigth;
                                    gr.DrawImage(numbers[sudoku_matrix[ii, jj] - 1], new Point(3 + 65 * 8 + 14, 3 + adding_heigth)); jj++; coordinate_matrix[8, 8, 0] = 3 + 65 * 8 + 14; coordinate_matrix[8, 8, 1] = 3 + adding_heigth;
                                }


                                // Macierz współrzędnych dla tego wszytkiego

                                // i teraz tylko, żeby przeszedł przez macierz zapisaną w pliku txt (stworzoną przez c++)
                                // do macierzy ją przepisze, i potem będzie badał i na tej podstawie będzie brał z tablicy ImageOverlay
                                // gdzie będą otwarte wszystkie numbery i będzie patrzył na to w jakim miejscu teraz powinien wpisać na podstawie
                                // macierzy współrzędnych

                                // niech najpierw zrobi solution, a potem na jego podstawie
                                // w losowych miejscach pododaje blank spaces i też będzie używał macierzy współrzędnych
                                // i essa
                            }
                            solution = "solution_" + (x + 1).ToString() + ".jpg";
                            solution_jpg.Save(solution, ImageFormat.Jpeg);
                            solution_jpg.Dispose();

                            solutions_tab[x] = System.Drawing.Image.FromFile(solution);
                        }
                    }

                    // tworzy sudoku --- wstawia blank spoty
                    for (int x = 0; x < 6; x++)
                    {
                        for (int i = 0; i < 9; i++) for (int j = 0; j < 9; j++) blank_matrix[i, j] = false;

                        int random_i = 0, random_j = 0;
                        for (int i = 0; i < how_many_blanks; i++)
                        {
                            random_i = r.Next(0, 9); random_j = r.Next(0, 9);

                            while (blank_matrix[random_i, random_j] == true)
                            {
                                random_i = r.Next(0, 9); random_j = r.Next(0, 9);
                            }

                            blank_matrix[random_i, random_j] = true;
                        }

                        System.Drawing.Image sudoku_jpg = new Bitmap(solutions_tab[x].Width, solutions_tab[x].Height);
                        using (Graphics gr = Graphics.FromImage(sudoku_jpg))
                        {
                            gr.DrawImage(solutions_tab[x], new Point(0, 0));

                            for (int i = 0; i < 9; i++)
                            {
                                for (int j = 0; j < 9; j++)
                                {
                                    // sudoku_matrix[i,j]-1 jaka liczba odczytana z macierzy
                                    //random = r.Next(1, 100);
                                    //if (how_many_percent <= random)

                                    if (!blank_matrix[i, j] == true)
                                    {
                                        gr.DrawImage(blank, new Point(coordinate_matrix[i, j, 0], coordinate_matrix[i, j, 1]));
                                    }
                                }
                            }
                        }
                        sudoku = "sudoku_" + (x + 1).ToString() + ".jpg";
                        sudoku_jpg.Save(sudoku, ImageFormat.Jpeg);
                        sudoku_jpg.Dispose();

                        sudokus_tab[x] = System.Drawing.Image.FromFile(sudoku);
                    }
                }

                // sklejanie jpg - 1 combined_sudoku i 1 combined_solution              
                {
                    int odstep_poziomo = 150;
                    int odstep_pionowo = 150;

                    int starting_point_x = 50;
                    int starting_point_y = 50;

                    // tworzenie jpg z wszystkimi sudoku                                 poprawne współrzędne 1208, 1806
                    System.Drawing.Image combined_sudoku_jpg = new Bitmap(1208 + (2 * odstep_poziomo) + 2 * starting_point_x - 100, 1806 + (2 * odstep_pionowo) + 2 * starting_point_y);
                    using (Graphics gr = Graphics.FromImage(combined_sudoku_jpg))
                    {
                        gr.DrawImage(background_pdf, new Point(0, 0));

                        int x = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                // + 0 == odstęp
                                gr.DrawImage(sudokus_tab[x], new Point(starting_point_x + (j * (sudokus_tab[0].Width + odstep_poziomo)),
                                                                       starting_point_y + (i * (sudokus_tab[0].Height + odstep_pionowo))));

                                x++;
                            }
                        }
                    }
                    combined_sudoku_jpg.Save("combined_sudoku.jpg", ImageFormat.Jpeg);
                    combined_sudoku_jpg.Dispose();

                    // tworzenie jpg z wszystkimi solutions
                    System.Drawing.Image combined_solutions_jpg = new Bitmap(1208 + (2 * odstep_poziomo) + 2 * starting_point_x - 100, 1806 + (2 * odstep_pionowo) + 2 * starting_point_y);
                    using (Graphics gr = Graphics.FromImage(combined_solutions_jpg))
                    {
                        gr.DrawImage(background_pdf, new Point(0, 0));

                        int x = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                // + 0 == odstęp
                                gr.DrawImage(solutions_tab[x], new Point(starting_point_x + (j * (solutions_tab[0].Width + odstep_poziomo)),
                                                                         starting_point_y + (i * (solutions_tab[0].Height + odstep_pionowo))));

                                x++;
                            }
                        }
                    }
                    combined_solutions_jpg.Save("combined_solutions.jpg", ImageFormat.Jpeg);
                    combined_solutions_jpg.Dispose();
                }

                // jpg -> pdf - 2 strony
                {
                    iTextSharp.text.Document document = new iTextSharp.text.Document(new iTextSharp.text.Rectangle(1684 - 150, 2384 - 100));
                    using (var stream = new FileStream("for_resize.pdf", FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        PdfWriter.GetInstance(document, stream);
                        document.Open();

                        // dodaje jpg z sudoku
                        using (var imageStream = new FileStream("combined_sudoku.jpg", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            var sudokus_var = iTextSharp.text.Image.GetInstance(imageStream);
                            document.Add(sudokus_var);
                        }

                        // dodaje jpg z solutions
                        using (var imageStream = new FileStream("combined_solutions.jpg", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            var solutions_var = iTextSharp.text.Image.GetInstance(imageStream);
                            document.Add(solutions_var);
                        }

                        document.Close();
                    }
                }

                // Przeskalowywanie pdf do rozmiarów A4 - tworzenie tmp_
                {
                    string original = "for_resize.pdf";

                    string inPDF = original;
                    PdfReader pdfr = new PdfReader(inPDF);

                    iTextSharp.text.Document doc = new iTextSharp.text.Document(PageSize.A4);
                    iTextSharp.text.Document.Compress = true;

                    string outPDF = "tmp_" + (big_iterator + 1).ToString() + ".pdf";
                    PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(outPDF, FileMode.Create));

                    doc.Open();
                    PdfContentByte cb = writer.DirectContent;
                    PdfImportedPage page;

                    for (int i = 1; i < pdfr.NumberOfPages + 1; i++)
                    {
                        page = writer.GetImportedPage(pdfr, i);
                        cb.AddTemplate(page, PageSize.A4.Width / pdfr.GetPageSize(i).Width, 0, 0, PageSize.A4.Height / pdfr.GetPageSize(i).Height, 0, 0);
                        doc.NewPage();
                    }
                    doc.Close();

                    pdfr.Dispose();
                    pdfr.Close();
                    File.Delete("for_resize.pdf");
                }

                // usuwanie wszystkich jpg
                {
                    for (int x = 0; x < 6; x++)
                    {
                        solutions_tab[x].Dispose();
                        sudokus_tab[x].Dispose();
                    }

                    for (int x = 0; x < 6; x++)
                    {
                        solution = "solution_" + (x + 1).ToString() + ".jpg";
                        sudoku = "sudoku_" + (x + 1).ToString() + ".jpg";

                        File.Delete(sudoku);
                        File.Delete(solution);
                    }

                    File.Delete("combined_sudoku.jpg");
                    File.Delete("combined_solutions.jpg");
                }
            }

            // dodajemy wszystkie pliki tmp_1 _2 _3 i tworzymy for_reorder
            {
                string tmp_pdf;
                iTextSharp.text.Document doc_pdf = new iTextSharp.text.Document();
                PdfCopy writer_pdf = new PdfCopy(doc_pdf, new FileStream("for_reorder.pdf", FileMode.Create));

                doc_pdf.Open();
                for (int x = 0; x < how_many_pages; x++)
                {
                    tmp_pdf = "tmp_" + (x + 1).ToString() + ".pdf";

                    PdfReader reader = new PdfReader(tmp_pdf);
                    reader.ConsolidateNamedDestinations();
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        PdfImportedPage page = writer_pdf.GetImportedPage(reader, i);
                        writer_pdf.AddPage(page);
                    }
                    reader.Close();
                }
                writer_pdf.Close();
                doc_pdf.Close();
            }

            // zmiana kolejności
            {
                iTextSharp.text.Document doc_pdf = new iTextSharp.text.Document();
                PdfCopy writer_pdf = new PdfCopy(doc_pdf, new FileStream("Final_100%_pure_product.pdf", FileMode.Create));

                doc_pdf.Open();

                PdfReader reader = new PdfReader("for_reorder.pdf");
                reader.ConsolidateNamedDestinations();
                for (int i = 1; i <= reader.NumberOfPages; i += 2)
                {
                    PdfImportedPage page = writer_pdf.GetImportedPage(reader, i);
                    writer_pdf.AddPage(page);
                }

                for (int i = 2; i <= reader.NumberOfPages; i += 2)
                {
                    PdfImportedPage page = writer_pdf.GetImportedPage(reader, i);
                    writer_pdf.AddPage(page);
                }

                reader.Close();

                writer_pdf.Close();
                doc_pdf.Close();
            }

            // usuwanie wszystkich pdf tmp_ + reorder.pdf
            {
                string path_to_tmp;
                for (int i = 0; i < how_many_pages; i++)
                {
                    path_to_tmp = "tmp_" + (i + 1).ToString() + ".pdf";

                    File.Delete(path_to_tmp);
                }

                File.Delete("for_reorder.pdf");
            }

            // kopiowanie ostatecznego produktu do wybranej przez użytkowniak ścieżki + nazwa pliku użytkownika 
            {
                if (textBox_file_path.Text != "" && Directory.Exists(textBox_file_path.Text))
                {
                    string path = @textBox_file_path.Text;
                    path += @"\";
                    if (textBox_file_name.Text != "")
                    {
                        if (File.Exists(path + textBox_file_name.Text + ".pdf")) File.Delete(path + textBox_file_name.Text + ".pdf");

                        File.Copy("Final_100%_pure_product.pdf", path + textBox_file_name.Text + ".pdf");
                    }
                    else
                    {
                        if (File.Exists(path + "sudoku.pdf")) File.Delete(path + "sudoku.pdf");

                        File.Copy("Final_100%_pure_product.pdf", path + "sudoku.pdf");
                    }
                }
            }
        }


        // Sudoku generation
        public void Generate_Sudoku()
        {
            // załadowanie patternu
            Sudoku_Pattern();
            int safe_check = 0;

            while (true)
            {
                // czyszczenie list
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (available_numbers_row[i].Count() > 0) available_numbers_row[i].Clear();
                        if (available_numbers_column[i].Count() > 0) available_numbers_column[i].Clear();
                        if (available_numbers_group[i].Count() > 0) available_numbers_group[i].Clear();
                    }
                }

                // inicjowanie sudoku_matrix na -1
                {
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            sudoku_matrix[i, j] = -1;
                        }
                    }
                }

                // dodawanie wszystkich liczb row, column, group
                {
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            available_numbers_row[i].Add(j + 1);
                            available_numbers_column[i].Add(j + 1);
                            available_numbers_group[i].Add(j + 1);
                        }
                    }
                }

                // ustawianie na właściwe miejsca
                {
                    int list_size;
                    int random_index;
                    int random_value;
                    int current_pi = 0;

                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            current_pi = pattern_for_sudoku_matrix[i, j];

                            list_size = available_numbers_row[i].Count();
                            random_index = r.Next(0, list_size);
                            random_value = available_numbers_row[i].ElementAt(random_index);

                            // wcześniej
                            //while (!Check_if_Good_Cross(i, j, available_numbers_row[pattern_for_sudoku_matrix[i, j]].ElementAt(random_index))) random_index = r.Next(0, list_size);
                            safe_check = 0;
                            while (!(available_numbers_row[i].Contains(random_value) && available_numbers_column[j].Contains(random_value) && available_numbers_group[current_pi].Contains(random_value)))
                            {
                                random_index = r.Next(0, list_size);
                                random_value = available_numbers_row[i].ElementAt(random_index);

                                safe_check++;
                                if (safe_check > 10) break;
                            }
                            if (safe_check > 10) break;


                            sudoku_matrix[i, j] = random_value;

                            available_numbers_row[i].Remove(random_value);
                            available_numbers_column[j].Remove(random_value);
                            available_numbers_group[current_pi].Remove(random_value);
                        }
                        if (safe_check > 10) break;
                    }
                }
                // ta na prawdę nie potrzebne jest to continue;
                if (safe_check > 10) continue;
                if (Check_if_Good_Sudoku()) break;
            }

            //check.Text
            // Sprawdzam Sudoku
            {
                /*
                {
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            check.Text += sudoku_matrix[i, j].ToString() + " ";
                        }
                        check.Text += "\n";
                    }

                    check.Text += "\n";
                    if (Check_if_Good_Sudoku()) check.Text += "good";
                    else check.Text += "bad";
                }
                */
            }
        }
        public bool Check_if_Good_Sudoku()
        {
            int[] sum_group = new int[9];
            int[] sum_column = new int[9];
            int[] sum_row = new int[9];

            // Inicjowanie na 0
            {
                for (int i = 0; i < 9; i++)
                {
                    sum_group[i] = 0;
                    sum_column[i] = 0;
                    sum_row[i] = 0;
                }
            }

            // Wpisywanie do właściwych tablic
            {
                int current_gi;
                int current_vs;

                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        current_gi = pattern_for_sudoku_matrix[i, j];
                        current_vs = sudoku_matrix[i, j];

                        sum_group[current_gi] += current_vs;
                        sum_row[i] += current_vs;
                        sum_column[j] += current_vs;
                    }
                }
            }

            for (int i = 0; i < 9; i++)
                if ((sum_group[i] != 45) || (sum_column[i] != 45) || (sum_column[i] != 45)) return false;

            return true;
        }
        public bool Check_if_Good_for_Column(int jj, int value)
        {
            for (int i = 0; i < 9; i++)
            {
                if (sudoku_matrix[i, jj] == value) return false;
            }
            return true;
        }
        public bool Check_if_Good_for_Row(int ii, int value)
        {
            for (int j = 0; j < 9; j++)
            {
                if (sudoku_matrix[ii, j] == value) return false;
            }
            return true;
        }
        public bool Check_if_Good_Cross(int i, int j, int value)
        {
            return (Check_if_Good_for_Row(i, value) && Check_if_Good_for_Column(j, value));
        }


        // Pattern for sudoku
        public void Sudoku_Pattern()
        {
            // custom
            if (checkBox_custom_pattern.Checked && Is_Pattern_Chosen_Correctly()) Load_Custom_Pattern();
            // standard
            else Load_Standard_Pattern();
        }
        public void Load_Standard_Pattern()
        {
            string input = File.ReadAllText(@"Assets for Sudoku\Exe_Files\sudoku_pattern.txt");

            int i = 0, j = 0;
            foreach (char a in input)
            {
                if (a == '\n')
                {
                    i++;
                    j = 0;
                }
                else if (a != ' ')
                {
                    pattern_for_sudoku_matrix[i, j] = (int)(a) - 48;
                    j++;
                }
            }
        }
        // from user unput
        public void Load_Custom_Pattern()
        {
            Color current = new Color();
            //int exact_index = 0;

            for (int i = 0; i < 9; i++) for (int j = 0; j < 9; j++)
                {
                    current = buttons_matrix[i, j].BackColor;
                    pattern_for_sudoku_matrix[i, j] = color_list.IndexOf(current);

                    //pattern_for_sudoku_matrix[i, j] = exact_index;
                }
            //pattern_for_sudoku_matrix[i, j] = 
        }
        bool Is_Pattern_Chosen_Correctly()
        {
            for (int i = 0; i < 9; i++) for (int j = 0; j < 9; j++)
                    //if (color_pattern_matrix[i, j] == -1)
                    if (pattern_for_sudoku_matrix[i, j] == -1)
                    {
                        label_exeptions.Text = "Pattern not complete";
                        return false;
                    }



            return Does_each_group_have_right_amount();
        }
        bool Does_each_group_have_right_amount()
        {
            int[] sum_tab = new int[9]; for (int i = 0; i < 9; i++) sum_tab[i] = 0;

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    //if (color_pattern_matrix[i, j] != -1) sum_tab[color_pattern_matrix[i, j]]++;
                    if (pattern_for_sudoku_matrix[i, j] != -1) sum_tab[pattern_for_sudoku_matrix[i, j]]++;
                }

                for (int x = 0; x < 9; x++) if (sum_tab[x] > 9)
                    {
                        label_exeptions.Text = "Wrong size of groups";
                        return false;
                    }
            }

            for (int x = 0; x < 9; x++) if (sum_tab[x] != 9)
                {
                    label_exeptions.Text = "Wrong size of groups";
                    return false;
                }

            label_exeptions.Text = "";
            return true;
        }


        // On click for all buttons
        void OnGenenicButtonClick(object sender, EventArgs e)
        {
            // nie wybrano koloru
            if (current_chosen_paint == -1) return;


            var btn = sender as Button;

            string name = btn.Name;
            //check.Text = "";
            //check.Text += name;

            name = name.Remove(0, 1);

            int coordinet = int.Parse(name);
            int i = 0;
            int j = 0;

            coordinet--;
            i = coordinet / 9;
            j = coordinet % 9;

            int previously_set_color = -1;
            // kliknięcie 
            if (color_pattern_matrix[i, j] != current_chosen_paint)
            {
                // color lock
                if (lock_color.Checked && black_list_for_color_limits.Contains(current_chosen_paint)) return;

                previously_set_color = color_pattern_matrix[i, j];
                color_pattern_matrix[i, j] = current_chosen_paint;
                btn.BackColor = color_tab[current_chosen_paint];

                paint_buttons_limits_tab[current_chosen_paint]++;
                if (paint_buttons_limits_tab[current_chosen_paint] > 8) black_list_for_color_limits.Add(current_chosen_paint);


                // zminiejszenie to co tak był wcześniej
                if (previously_set_color != -1)
                {
                    paint_buttons_limits_tab[previously_set_color]--;
                    if (black_list_for_color_limits.Contains(previously_set_color)) black_list_for_color_limits.Remove(previously_set_color);
                }
            }
            // odkliknięcie
            else
            {
                // set back to default
                var btn_exeption = sender as Button;
                btn_exeption.BackColor = SystemColors.ButtonFace;
                btn_exeption.UseVisualStyleBackColor = true;

                color_pattern_matrix[i, j] = -1;
                paint_buttons_limits_tab[current_chosen_paint]--;
                if (black_list_for_color_limits.Contains(current_chosen_paint)) black_list_for_color_limits.Remove(current_chosen_paint);
            }



            // każdy blok, może sprawdzać czy sąsiaduje z co najmniej 1 blokiem do góry, w dół, w prawo, w lewo o takiej samej wartości (liczby jako kolory)


            // najpierw liczba--;
            // rząd ---> podzielić przez 9
            // kolumna ---> reszta z dzielenia przez 9
        }


        // Reset button
        private void reset_button_Click(object sender, EventArgs e)
        {
            Reset_Buttons();
        }
        // Reset
        void Reset_Buttons()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    buttons_matrix[i, j].BackColor = SystemColors.ButtonFace;
                    buttons_matrix[i, j].UseVisualStyleBackColor = true;
                }
            }
            for (int i = 0; i < 9; i++) paint_buttons_limits_tab[i] = 0;
            if (black_list_for_color_limits.Count() > 1) black_list_for_color_limits.Clear();
            //current_chosen_paint = -1;
            for (int i = 0; i < 9; i++) for (int j = 0; j < 9; j++) color_pattern_matrix[i, j] = -1;
            label_exeptions.Text = "";
        }


        // Random Group button
        private void button_random_group_Click(object sender, EventArgs e)
        {
            Random_Pattern_for_Sudoku();
            Apply_to_buttons();
        }
        // Random Group generation
        void Random_Pattern_for_Sudoku()
        {
            // algorytm tworzenia patternu
            // return pattern_for_sudoku_matrix

            bool[,] available_spaces_matrix = new bool[9, 9];
            Random random = new Random();
            int i, j;

            // jeszcze to jest to dopracowania ale jest bliżej
            // jeszcze to jest to dopracowania ale jest bliżej
            // jeszcze to jest to dopracowania ale jest bliżej
            // jeszcze to jest to dopracowania ale jest bliżej
            // jeszcze to jest to dopracowania ale jest bliżej
            // jeszcze to jest to dopracowania ale jest bliżej

            // touching sides
            if (checkBox_generation_modyfier.Checked)
            {
                // 3 level array dostępnych współrzędnych
                // int[,,] multilevel_available_spots = new int[9, 9, 9]; for (int z = 0; z < 9; z++) for (int y = 0; y < 9; y++) for (int x = 0; x < 9; x++) multilevel_available_spots[z, y, x] = 0;

                // reset                
                Reset_Buttons();

                // jak się już raz wykona to, musi skasować wcześniej zrobione
                for (int y = 0; y < 9; y++) for (int x = 0; x < 9; x++) pattern_for_sudoku_matrix[y, x] = -1;
                List<Coordinet>[] available_cords = new List<Coordinet>[9]; for (int c = 0; c < 9; c++) available_cords[c] = new List<Coordinet>();
                int random_index = 0;

                while (!Is_Pattern_Chosen_Correctly())
                {
                    // setup
                    for (int y = 0; y < 9; y++) for (int x = 0; x < 9; x++) available_spaces_matrix[y, x] = true;
                    for (int c = 0; c < 9; c++) if(available_cords[c].Count() > 0) available_cords[c].Clear();

                    for (int counter = 0; counter < 9; counter++)
                    {
                        for (int group_id = 0; group_id < 9; group_id++)
                        {
                            // to, żeby spawn pointy nie były obok siebie
                            if (counter == 0)
                            {
                                i = random.Next(0, 9);
                                j = random.Next(0, 9);

                                while (!available_spaces_matrix[i, j])
                                {
                                    i = random.Next(0, 9);
                                    j = random.Next(0, 9);
                                }

                                pattern_for_sudoku_matrix[i, j] = group_id;

                                // if (i - 1 > -1) multilevel_available_spots[group_id, i - 1, j] = 1;
                                // if (i + 1 < 9) multilevel_available_spots[group_id, i + 1, j] = 1;

                                // if (j - 1 > -1) multilevel_available_spots[group_id, i, j - 1] = 1;
                                // if (j + 1 < 9) multilevel_available_spots[group_id, i, j + 1] = 1;


                                if (i - 1 > -1) available_cords[group_id].Add(new Coordinet(i - 1, j));
                                if (i + 1 <  9) available_cords[group_id].Add(new Coordinet(i + 1, j));

                                if (j - 1 > -1) available_cords[group_id].Add(new Coordinet(i, j - 1));
                                if (j + 1 <  9) available_cords[group_id].Add(new Coordinet(i, j + 1));

                                Mark_Every_Cube_Around(available_spaces_matrix, i, j);
                            }
                            else
                            {
                                // tak, żeby mógł dodawać tylko do tych obok spawn pointa
                                // Macierz 3 poziomowa - losowa współrzędna
                                {
                                    /*
                                    i = random.Next(0, 9);
                                    j = random.Next(0, 9);
                                    while (multilevel_available_spots[group_id, i, j] != 1)
                                    {
                                        i = random.Next(0, 9);
                                        j = random.Next(0, 9);
                                    }
                                    */
                                }
                                // Macierz 3 poziomowa
                                {
                                    /*
                                    // usuwanie ze wszystkich pozostałych macierzach
                                    for (int group = 0; group < 9; group++) multilevel_available_spots[group, i, j] = 0;

                                    // dodaje możliwe współrzędne
                                    if (i - 1 > -1 && available_spaces_matrix[i - 1, j]) multilevel_available_spots[group_id, i - 1, j] = 1;
                                    if (i + 1 < 9 && available_spaces_matrix[i + 1, j]) multilevel_available_spots[group_id, i + 1, j] = 1;

                                    if (j - 1 > -1 && available_spaces_matrix[i, j - 1]) multilevel_available_spots[group_id, i, j - 1] = 1;
                                    if (j + 1 < 9 && available_spaces_matrix[i, j + 1]) multilevel_available_spots[group_id, i, j + 1] = 1;
                                    */
                                }                                                               

                                // Lista współrzędnych - losowa współrzędna
                                {
                                    int list_size = available_cords[group_id].Count();
                                    
                                    ////////////////////////////////////////////////////////
                                    //////////////// Kasowanie nie działa i nigdy nie generuje patternu
                                    //////////////// Kasowanie nie działa i nigdy nie generuje patternu
                                    //////////////// Kasowanie nie działa i nigdy nie generuje patternu
                                    //////////////// Kasowanie nie działa i nigdy nie generuje patternu
                                    if(list_size == 0)
                                    {
                                        // resets all
                                        for (int y = 0; y < 9; y++) for (int x = 0; x < 9; x++) available_spaces_matrix[y, x] = true;
                                        for (int c = 0; c < 9; c++) if (available_cords[c].Count() > 0) available_cords[c].Clear();

                                        counter = 0;
                                        group_id = 0;
                                        continue;
                                    }
                                    random_index = random.Next(0, list_size);                                    
                                    Coordinet tmp = available_cords[group_id].ElementAt(random_index);

                                    i = tmp.First();
                                    j = tmp.Second();
                                }
                                pattern_for_sudoku_matrix[i, j] = group_id;
                                available_spaces_matrix[i, j] = false;


                                ////////////// Listy //////////////
                                {
                                    // usuwanie z list jeśli na niej jest
                                    for (int c = 0; c < 9; c++) if (available_cords[c].Any(obj => (obj.First() == i && obj.Second() == j))) available_cords[c].RemoveAll(obj => (obj.First() == i && obj.Second() == j));

                                    // dodaje możliwe współrzędne do list
                                    if (i - 1 > -1 && available_spaces_matrix[i - 1, j] && !available_cords[group_id].Any(obj => (obj.First() == i - 1 && obj.Second() == j))) available_cords[group_id].Add(new Coordinet(i - 1, j));
                                    if (i + 1 <  9 && available_spaces_matrix[i + 1, j] && !available_cords[group_id].Any(obj => (obj.First() == i + 1 && obj.Second() == j))) available_cords[group_id].Add(new Coordinet(i + 1, j));

                                    if (j - 1 > -1 && available_spaces_matrix[i, j - 1] && !available_cords[group_id].Any(obj => (obj.First() == i && obj.Second() == j - 1))) available_cords[group_id].Add(new Coordinet(i, j - 1));
                                    if (j + 1 <  9 && available_spaces_matrix[i, j + 1] && !available_cords[group_id].Any(obj => (obj.First() == i && obj.Second() == j + 1))) available_cords[group_id].Add(new Coordinet(i, j + 1));
                                }
                            }
                        }

                        // blokowanie tylko konkretnych miejsc po ustanowieniu spawnpointów
                        if (counter == 0)
                        {
                            for (int a = 0; a < 9; a++) for (int b = 0; b < 9; b++) available_spaces_matrix[a, b] = true;
                            for (int a = 0; a < 9; a++) for (int b = 0; b < 9; b++) if (pattern_for_sudoku_matrix[a, b] != -1) available_spaces_matrix[a, b] = false;
                        }

                        // Visualize
                        if (checkBox_visualize.Checked)
                        {
                            Apply_to_buttons();
                            Thread.Sleep(100);
                        }
                    }
                }
            }

            // zupełnie losowe ustawienie grup <--- jedyne ograniczenie to ilość pól w każdej grupie
            else
            {
                // setup
                for (int y = 0; y < 9; y++) for (int x = 0; x < 9; x++) available_spaces_matrix[y, x] = true;

                for (int c = 0; c < 9; c++)
                {
                    for (int group = 0; group < 9; group++)
                    {
                        i = random.Next(0, 9);
                        j = random.Next(0, 9);

                        while (!available_spaces_matrix[i, j])
                        {
                            i = random.Next(0, 9);
                            j = random.Next(0, 9);
                        }

                        available_spaces_matrix[i, j] = false;
                        pattern_for_sudoku_matrix[i, j] = group;

                    }
                    // Visualize
                    if (checkBox_visualize.Checked)
                    {
                        Apply_to_buttons();
                        check.Text += c.ToString() + " ";

                        for (int aaa = 0; aaa < 100000000; aaa++) ;

                        {
                            /*
                            Thread.Sleep(500);
                            check.Text = "";
                            for (int a = 0; a < 9; a++)
                            {
                                for (int b = 0; b < 9; b++) check.Text += pattern_for_sudoku_matrix[a, b] + " ";
                                check.Text += "\n";
                            }
                            */
                        }
                    }
                }

            }
        }
        // for spawn points
        void Mark_Every_Cube_Around(bool[,] available_spaces, int i, int j)
        {
            int for_j = -1;
            int for_i = -1;

            for (int x = 0; x < 3; x++)
            {
                for_j = -1;
                for (int y = 0; y < 3; y++)
                {
                    if ((i + for_i >= 0 && i + for_i < 9) &&
                        (j + for_j >= 0 && j + for_j < 9))

                        available_spaces[i + for_i, j + for_j] = false;                        
                    for_j++;
                }
                for_i++;
            }
            
        }
        // Apply to buttons - z pattern_for_sudoku_matrix
        void Apply_to_buttons()
        {
            //Load_Custom_Pattern();

            // load custom from pattern_for_sudoku_matrix

            // bierze macierz wygenerowaną przez Random_Pattern_for_Sudoku
            if (Is_Pattern_Chosen_Correctly())
            {
                check.Text += "chosen correctly" + "\n";                
            }
            else
            {
                check.Text += "chosen uncorrenctly" + "\n";
                //return;
            }

            // reset for safety
            Reset_Buttons();

            // applys colors
            {
                int current = 0;
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        current = pattern_for_sudoku_matrix[i, j];

                        //check.Text += current + " ";

                        if (current > -1) buttons_matrix[i, j].BackColor = color_tab[current];
                    }
                    //check.Text += "\n";
                }
            }
        }




        // Directory.Exists








        




        // Paints
        private void paints_1_Click(object sender, EventArgs e) { current_chosen_paint = 0; }
        private void paints_2_Click(object sender, EventArgs e) { current_chosen_paint = 1; }
        private void paints_3_Click(object sender, EventArgs e) { current_chosen_paint = 2; }
        private void paints_4_Click(object sender, EventArgs e) { current_chosen_paint = 3; }
        private void paints_5_Click(object sender, EventArgs e) { current_chosen_paint = 4; }
        private void paints_6_Click(object sender, EventArgs e) { current_chosen_paint = 5; }
        private void paints_7_Click(object sender, EventArgs e) { current_chosen_paint = 6; }
        private void paints_8_Click(object sender, EventArgs e) { current_chosen_paint = 7; }
        private void paints_9_Click(object sender, EventArgs e) { current_chosen_paint = 8; }





        // i do tego jeszcze, żeby dodawał je pod sobą, tak żeby na górze było 
        // sudoku niżej mała przerwa i rozwiązanie
        // img.Save("solution.jpg", ImageFormat.Jpeg);
        // nie na górze tylko tak obok siebie --------------------------------------> co będzie można je tak wydrukować i zagiąć
        // potem można zrobić, żeby zrobił całą stronę z tymi sudoku (gotową do wydrukowania)
        // tak po 6 na stronę i kolejna strona z rozwiązaniami
        // strony ponumerowane i każde sudoku też ponumerowane

        // A TE POŁĄCZONE ZDJĘCIA, BĘDZIE ZAMIENIAŁ NA PDF         EEEEEEESSSSSSSSSAAAAAAAAAAAAAAAA




        // jak już bardzo chcę to może też zrobić, żeby dla niektórych losował to customowe ułożenie tylko tak żeby nie dotykały krawędziami
        // nie tak totalnie losowo
    }
}
