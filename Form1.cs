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
        private int [,] mapa = new int[,] { 
            { 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0 }, 
            { 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0 },
            { 0, 0, 3, 0, 0, 0, 1, 1, 0, 0, 2, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1 }, 
            { 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 1 } 
        };
        private bool cambio_mapa = true;
        private int[] posjug = new int[2];

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //synth.Speak("Bienvenido al diseño de interfaces avanzadas. Inicializando la Aplicación");

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
            //synth.Speak("Aplicación preparada para reconocer su voz");
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (this.cambio_mapa)
            {
                Size size = this.Size;
                int width = size.Width;
                int height = size.Height;
                Color fondo = System.Drawing.Color.FromArgb(255, 107, 241, 120);
                Color obstaculos = System.Drawing.Color.FromArgb(255, 68, 99, 63);
                Color jugador = System.Drawing.Color.FromArgb(255, 255, 127, 17);
                Color meta = System.Drawing.Color.FromArgb(255, 108, 207, 246);
                Color c = new Color();
                System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(c);
                int rows = this.mapa.GetLength(0);
                int cols = this.mapa.GetLength(1);

                for (int i = 0; i < rows; i++)
                {
                    int y1 = height * i / rows;
                    int y2 = height * (i + 1) / rows;
                    for (int j = 0; j < cols; j++)
                    {
                        int v = this.mapa[i, j];
                        int x1 = width * j / cols;
                        int x2 = width * (j + 1) / cols;
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
                        e.Graphics.FillRectangle(myBrush, new Rectangle(x1, y1, x2, y2));
                    }
                }

                myBrush.Dispose();
                cambio_mapa = false;
            }
        }

        void _recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            ////obtenemos un diccionario con los elementos semánticos
            //SemanticValue semantics = e.Result.Semantics;
          
            //string rawText = e.Result.Text;
            //RecognitionResult result = e.Result;

            //if (!semantics.ContainsKey("rgb"))
            //{
            //    this.label1.Text = "No info provided.";
            //}
            //else
            //{
            //    this.label1.Text = rawText;
            //    this.BackColor = Color.FromArgb((int)semantics["rgb"].Value);
            //    Update();
            //    //synth.Speak(rawText);
            //}

            SemanticValue semantics = e.Result.Semantics;

            string rawText = e.Result.Text;
            Console.WriteLine(rawText);
            RecognitionResult result = e.Result;

            if (!semantics.ContainsKey("direc"))
            {
                this.label1.Text = "No info provided.";
            }
            else
            {
                Direccion d = (Direccion)semantics["direc"].Value;
                this.mover_jugador(d);
                Update();
                //synth.Speak(rawText);
            }
        }

        private void mover_jugador(Direccion d)
        {
            int j_row = this.posjug[0];
            int j_col = this.posjug[1];
            if (d.Equals(Direccion.Izquierda))
            {
                while (j_col > 0 && this.mapa[j_row, j_col - 1] == 0)
                {
                    this.mapa[j_row, j_col - 1] = 2;
                    this.mapa[j_row, j_col] = 0;
                    j_col--;
                    this.cambio_mapa = true;
                    this.Invalidate();
                }
            }
            else if (d.Equals(Direccion.Arriba))
            {
                while (j_row > 0 && this.mapa[j_row - 1, j_col] == 0)
                {
                    this.mapa[j_row - 1, j_col] = 2;
                    this.mapa[j_row, j_col] = 0;
                    j_row--;
                    this.cambio_mapa = true;
                    this.Invalidate();
                }
            }
            else if (d.Equals(Direccion.Derecha))
            {
                while (j_col < (this.mapa.GetLength(1) - 1) && this.mapa[j_row, j_col + 1] == 0)
                {
                    this.mapa[j_row, j_col + 1] = 2;
                    this.mapa[j_row, j_col] = 0;
                    j_col++;
                    this.cambio_mapa = true;
                    this.Invalidate();
                }
            }
            else if (d.Equals(Direccion.Abajo))
            {
                while (j_row < (this.mapa.GetLength(0) - 1) && this.mapa[j_row + 1, j_col] == 0)
                {
                    this.mapa[j_row + 1, j_col] = 2;
                    this.mapa[j_row, j_col] = 0;
                    j_row++;
                    this.cambio_mapa = true;
                    this.Invalidate();
                }
            }
        }
        
      
        private Grammar CreateGrammarBuilderRGBSemantics2(params int[] info)
        {
            //synth.Speak("Creando ahora la gramática");
            //Choices colorChoice = new Choices();


            //SemanticResultValue choiceResultValue =
            //        new SemanticResultValue("Rojo", Color.FromName("Red").ToArgb());
            //GrammarBuilder resultValueBuilder = new GrammarBuilder(choiceResultValue);
            //colorChoice.Add(resultValueBuilder);

            //choiceResultValue =
            //       new SemanticResultValue("Azul", Color.FromName("Blue").ToArgb());
            //resultValueBuilder = new GrammarBuilder(choiceResultValue);
            //colorChoice.Add(resultValueBuilder);

            //choiceResultValue =
            //       new SemanticResultValue("Verde", Color.FromName("Green").ToArgb());
            //resultValueBuilder = new GrammarBuilder(choiceResultValue);
            //colorChoice.Add(resultValueBuilder);

            //SemanticResultKey choiceResultKey = new SemanticResultKey("rgb", colorChoice);
            //GrammarBuilder colores = new GrammarBuilder(choiceResultKey);

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
            GrammarBuilder mover2 = "Mover a";
            GrammarBuilder mover3 = "Mover a la";
            GrammarBuilder desplazar ="Desplazar";
            GrammarBuilder desplazar2 = "Desplazar a";
            GrammarBuilder desplazar3 = "Desplazar a la";

            Choices verbos = new Choices(mover, mover2, mover3, desplazar, desplazar2, desplazar3);
            GrammarBuilder frase = new GrammarBuilder(verbos);
            frase.Append(direcciones);
            //frase.Append(fondo);
            //frase.Append(colores);
            Grammar grammar = new Grammar(frase);            
            grammar.Name = "Desplazar jugador";

            //Grammar grammar = new Grammar("so.xml.txt");
 
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
}