using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
/*using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;*/
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace puzzleRV
{

    public partial class Form1 : Form
    {
     private System.Speech.Recognition.SpeechRecognitionEngine _recognizer = 
        new SpeechRecognitionEngine();
        private SpeechSynthesizer synth = new SpeechSynthesizer();
        private Label label1 = new Label();
        private int[,] mapa = new int[,] {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 3, 0, 0, 0, 1, 1, 0, 0, 2, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1 },
            { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        };
        private bool cambio_mapa = true;
        private String frase = "Escuchando tus indicaciones...";
        private int[] posjug = new int[2];
        private Direccion d;
        private Thread t;
        private Single single = 32F;
        private int stringY = 20;
        private int refresco = 500;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //synth.Speak("Bienvenido. Está a punto de resolver un increíble puzzle, tienes que hacer que el cuadrado naranja se acople al cuadrado azul." +
            //    " Espera mientras cargamos el mapa");

            Grammar grammar = CreateGrammarBuilderRGBSemantics2(null);
            _recognizer.SetInputToDefaultAudioDevice();
            _recognizer.UnloadAllGrammars();
            // Nivel de confianza del reconocimiento 50%
            _recognizer.UpdateRecognizerSetting("CFGConfidenceRejectionThreshold", 50);
            grammar.Enabled = true;
            _recognizer.LoadGrammar(grammar);
            _recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(_recognizer_SpeechRecognized);
            //reconocimiento asíncrono y múltiples veces
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            NuevaPartida();
            //synth.Speak("Aplicación preparada para reconocer su voz, indica hacia donde quieres moverte.");
        }

        private void NuevaPartida()
        {
            single = 32F;
            stringY = 20;
            refresco = 500;
            mapa = new int[,] {
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 1 },
                { 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1 },
                { 1, 0, 0, 3, 0, 0, 0, 1, 1, 0, 0, 2, 0, 1 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                { 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1 },
                { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 1, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
            };
            this.cambio_mapa = true;
        }

        private void NuevaPartidaGenerada(TipoPartida tp)
        {
            int N = 16;
            double pObs = 0.2;
            int numMetas = 1;

            if (tp.Equals(TipoPartida.Normal))
            {
                N = 32;
                numMetas = 2;
                single = 16F;
                stringY = 15;
                refresco = 250;
            }
            else if (tp.Equals(TipoPartida.Dificil))
            {
                N = 64;
                numMetas = 4;
                single = 8F;
                stringY = 7;
                refresco = 100;
            }

            int M = N / 2;
            mapa = new int[M + 2, N + 2];
            int rows = M + 2;
            int cols = N + 2;
            Random r = new Random();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (i == 0 || i == rows - 1 || j == 0 || j == cols - 1)
                    {
                        mapa[i, j] = 1;
                    }
                    else if (r.NextDouble() > pObs)
                    {
                        mapa[i, j] = 0;
                    }
                    else
                    {
                        mapa[i, j] = 1;
                    }
                }
            }

            int iJug = r.Next(1, rows - 1);
            int jJug = r.Next(1, cols - 1);
            while (mapa[iJug, jJug] != 0)
            {
                iJug = r.Next(1, rows - 1);
                jJug = r.Next(1, cols - 1);
            }
            mapa[iJug, jJug] = 2;

            for (int n = numMetas; n > 0; n--)
            {
                int iMeta = r.Next(1, rows - 1);
                int jMeta = r.Next(1, cols - 1);
                while (mapa[iMeta, jMeta] != 0)
                {
                    iMeta = r.Next(1, rows - 1);
                    jMeta = r.Next(1, cols - 1);
                }
                mapa[iMeta, jMeta] = 3;
            }


            this.cambio_mapa = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (this.cambio_mapa)
            {
                Size size = this.ClientSize;
                int width = size.Width;
                int height = size.Height;
                Color fondo = System.Drawing.Color.FromArgb(255, 107, 241, 120);
                Color obstaculos = System.Drawing.Color.FromArgb(255, 68, 99, 63);
                Color jugador = System.Drawing.Color.FromArgb(255, 255, 127, 17);
                Color meta = System.Drawing.Color.FromArgb(255, 108, 207, 246);
                Color blanco = System.Drawing.Color.FromArgb(255, 255, 255, 255);
                Color c = new Color();
                System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(c);
                int rows = this.mapa.GetLength(0);
                int cols = this.mapa.GetLength(1);
                int tileWidth = (width / cols) + 1;
                int tileHeight = (height / rows) + 1;

                for (int i = 0; i < rows; i++)
                {
                    int y1 = tileHeight * i;
                    for (int j = 0; j < cols; j++)
                    {
                        int v = this.mapa[i, j];
                        int x1 = tileWidth * j;
                        if (v == 0)
                        {
                            myBrush.Color = fondo;
                        }
                        else if (v == 1)
                        {
                            myBrush.Color = obstaculos;
                        }
                        else if (v == 2)
                        {
                            myBrush.Color = jugador;
                            this.posjug = new int[] { i, j };
                        }
                        else
                        {
                            myBrush.Color = meta;
                        }
                        e.Graphics.FillRectangle(myBrush, new Rectangle(x1, y1, tileWidth, tileHeight));
                    }
                }
                myBrush.Color = blanco;
                Font font = new System.Drawing.Font("Microsoft Sans Serif", single, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                e.Graphics.DrawString(frase, font, myBrush, width / 2, stringY, stringFormat);
                myBrush.Dispose();
                cambio_mapa = false;
            }
        }

        void _recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            SemanticValue semantics = e.Result.Semantics;

            string rawText = e.Result.Text;
            Console.WriteLine(rawText);
            RecognitionResult result = e.Result;


            this.frase = rawText;

            if (semantics.ContainsKey("direc"))
            {
                this.d = (Direccion)semantics["direc"].Value;
                this.t = new Thread(new ThreadStart(this.mover_jugador));
                t.Start();
                Update();
            }
            else if (semantics.ContainsKey("nueva_partida"))
            {
                if (semantics.ContainsKey("tipo_partida"))
                {
                    TipoPartida tp = (TipoPartida)semantics["tipo_partida"].Value;
                    NuevaPartidaGenerada(tp);
                }
                else
                {
                    NuevaPartida();
                }
                this.Invalidate();
                Update();
            }
        }

        private void mover_jugador()
        {
            int j_row = this.posjug[0];
            int j_col = this.posjug[1];
            bool es_meta = false;
            if (d.Equals(Direccion.Izquierda))
            {
                while (j_col > 0 && this.mapa[j_row, j_col - 1] != 1)
                {
                    es_meta = this.mapa[j_row, j_col - 1] == 3;
                    this.mapa[j_row, j_col - 1] = 2;
                    this.mapa[j_row, j_col] = 0;
                    j_col--;
                    if (ActualizarMapa(es_meta))
                    {
                        break;
                    }
                }
            }
            else if (d.Equals(Direccion.Arriba))
            {
                while (j_row > 0 && this.mapa[j_row - 1, j_col] != 1)
                {
                    es_meta = this.mapa[j_row - 1, j_col] == 3;
                    this.mapa[j_row - 1, j_col] = 2;
                    this.mapa[j_row, j_col] = 0;
                    j_row--;
                    if (ActualizarMapa(es_meta)) 
                    {
                        break;
                    }

                }
            }
            else if (d.Equals(Direccion.Derecha))
            {
                while (j_col < (this.mapa.GetLength(1) - 1) && this.mapa[j_row, j_col + 1] != 1)
                {
                    es_meta = this.mapa[j_row, j_col + 1] == 3;
                    this.mapa[j_row, j_col + 1] = 2;
                    this.mapa[j_row, j_col] = 0;
                    j_col++;
                    if (ActualizarMapa(es_meta))
                    {
                        break;
                    }
                }
            }
            else if (d.Equals(Direccion.Abajo))
            {
                while (j_row < (this.mapa.GetLength(0) - 1) && this.mapa[j_row + 1, j_col] != 1)
                {
                    es_meta = this.mapa[j_row + 1, j_col] == 3;
                    this.mapa[j_row + 1, j_col] = 2;
                    this.mapa[j_row, j_col] = 0;
                    j_row++;
                    if (ActualizarMapa(es_meta))
                    {
                        break;
                    }
                }
            }
        }

        private bool ActualizarMapa(bool es_meta)
        {
            this.cambio_mapa = true;
            if (es_meta)
            {
                this.frase = "Has llegado a la meta";
                this.Invalidate();
                return true;
            }
            this.Invalidate();
            Thread.Sleep(refresco);
            return false;
        }
        
      
        private Grammar CreateGrammarBuilderRGBSemantics2(params int[] info)
        {
            Choices direcChoice = new Choices();

            SemanticResultValue choiceResultValue =
                    new SemanticResultValue("Izquierda", 0);
            GrammarBuilder resultValueBuilder = new GrammarBuilder(choiceResultValue);
            direcChoice.Add(resultValueBuilder);

            choiceResultValue =
                    new SemanticResultValue("Arriba", 1);
            resultValueBuilder = new GrammarBuilder(choiceResultValue);
            direcChoice.Add(resultValueBuilder);

            choiceResultValue =
                    new SemanticResultValue("Derecha", 2);
            resultValueBuilder = new GrammarBuilder(choiceResultValue);
            direcChoice.Add(resultValueBuilder);

            choiceResultValue =
                    new SemanticResultValue("Abajo", 3);
            resultValueBuilder = new GrammarBuilder(choiceResultValue);
            direcChoice.Add(resultValueBuilder);

            SemanticResultKey choiceResultKey = new SemanticResultKey("direc", direcChoice);
            GrammarBuilder direcciones = new GrammarBuilder(choiceResultKey);


            GrammarBuilder mover = "Mover";
            GrammarBuilder mover1 = "Muevete";
            GrammarBuilder mover2 = "Movete";
            GrammarBuilder desplazar ="Desplazar";
            GrammarBuilder desplazar1 = "Desplazate";
            GrammarBuilder desplazar3 = "Desplazando";

            GrammarBuilder conector1 = "A";
            GrammarBuilder conector2 = "A la";
            GrammarBuilder conector3 = "Hacia";
            GrammarBuilder conector4 = "Hacia la";

            Choices verbos = new Choices(mover, mover1, mover2, desplazar, desplazar1, desplazar3);
            Choices conectores = new Choices(conector1, conector2, conector3, conector4);
            GrammarBuilder desplazamiento = new GrammarBuilder();
            desplazamiento.Append(verbos, 0, 1);
            desplazamiento.Append(conectores, 0, 1);
            desplazamiento.Append(direcciones);


            Choices tipoPartidaChoice = new Choices();

            choiceResultValue =
                    new SemanticResultValue("Fácil", 0);
            resultValueBuilder = new GrammarBuilder(choiceResultValue);
            tipoPartidaChoice.Add(resultValueBuilder);

            choiceResultValue =
                    new SemanticResultValue("Normal", 1);
            resultValueBuilder = new GrammarBuilder(choiceResultValue);
            tipoPartidaChoice.Add(resultValueBuilder);

            choiceResultValue =
                    new SemanticResultValue("Difícil", 2);
            resultValueBuilder = new GrammarBuilder(choiceResultValue);
            tipoPartidaChoice.Add(resultValueBuilder);

            SemanticResultKey tipoPartidaKey = new SemanticResultKey("tipo_partida", tipoPartidaChoice);
            GrammarBuilder tipoPartida = new GrammarBuilder(tipoPartidaKey);

            Choices nuevaPChoice = new Choices();
            GrammarBuilder nuevaP = "Nueva partida";
            nuevaPChoice.Add(nuevaP);
            GrammarBuilder nuevoM = "Nuevo mapa";
            nuevaPChoice.Add(nuevoM);

            SemanticResultKey nuevaPartidaKey = new SemanticResultKey("nueva_partida", nuevaPChoice);
            GrammarBuilder nuevaPartida = new GrammarBuilder(nuevaPartidaKey);
            nuevaPartida.Append(tipoPartida, 0, 1);

            Choices posiblesAcciones = new Choices(desplazamiento, nuevaPartida);
            GrammarBuilder frase = new GrammarBuilder(posiblesAcciones);

            Grammar grammar = new Grammar(frase);            
            grammar.Name = "Puzzle";
 
            return grammar;
        }
    }

    public enum Direccion
    {
        Izquierda,
        Arriba,
        Derecha,
        Abajo
    }

    public enum TipoPartida
    {
        Facil,
        Normal,
        Dificil
    }
}