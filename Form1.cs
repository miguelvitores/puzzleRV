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
        
        public Form1()
        {

            /*
            // Create a StackPanel to contain the shape.
            StackPanel myStackPanel = new StackPanel();

            // Create a red Ellipse.
            Ellipse myEllipse = new Ellipse();

            // Create a SolidColorBrush with a red color to fill the
            // Ellipse with.
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();

            // Describes the brush's color using RGB values.
            // Each value has a range of 0-255.
            mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 0);
            myEllipse.Fill = mySolidColorBrush;
            myEllipse.StrokeThickness = 2;
            myEllipse.Stroke = Brushes.Black;

            // Set the width and height of the Ellipse.
            myEllipse.Width = 200;
            myEllipse.Height = 100;

            // Add the Ellipse to the StackPanel.
            myStackPanel.Children.Add(myEllipse);

            this.Content = myStackPanel;*/



            InitializeComponent();

            this.pictureBox1.Image = this.Draw(this.pictureBox1.Width, this.pictureBox1.Height);
        }

        public Bitmap Draw(int width, int height)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.FillRectangle(new SolidBrush(Color.Tomato), 10, 10, 100, 100);

            return bitmap;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            synth.Speak("Bienvenido al diseño de interfaces avanzadas. Inicializando la Aplicación");

            Grammar grammar= CreateGrammarBuilderRGBSemantics2(null);
            _recognizer.SetInputToDefaultAudioDevice();
            _recognizer.UnloadAllGrammars();
            // Nivel de confianza del reconocimiento 70%
            _recognizer.UpdateRecognizerSetting("CFGConfidenceRejectionThreshold", 50);
            grammar.Enabled = true;
            _recognizer.LoadGrammar(grammar);
            _recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(_recognizer_SpeechRecognized);
            //reconocimiento asíncrono y múltiples veces
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            synth.Speak("Aplicación preparada para reconocer su voz");
         }

     

        void _recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
                      //obtenemos un diccionario con los elementos semánticos
                      SemanticValue semantics = e.Result.Semantics;
          
                      string rawText = e.Result.Text;
                      RecognitionResult result = e.Result;

                      if (!semantics.ContainsKey("rgb"))
                      {
                          this.label1.Text = "No info provided.";
                      }
                      else
                      {
                          this.label1.Text = rawText;
                          this.BackColor = Color.FromArgb((int)semantics["rgb"].Value);
                          Update();
                          //synth.Speak(rawText);
                      }
        }
        
      
        private Grammar CreateGrammarBuilderRGBSemantics2(params int[] info)
        {
            //synth.Speak("Creando ahora la gramática");
            Choices colorChoice = new Choices();
     
            
            SemanticResultValue choiceResultValue =
                    new SemanticResultValue("Rojo", Color.FromName("Red").ToArgb());
            GrammarBuilder resultValueBuilder = new GrammarBuilder(choiceResultValue);
            colorChoice.Add(resultValueBuilder);
            
            choiceResultValue =
                   new SemanticResultValue("Azul", Color.FromName("Blue").ToArgb());
            resultValueBuilder = new GrammarBuilder(choiceResultValue);
            colorChoice.Add(resultValueBuilder);
            
            choiceResultValue =
                   new SemanticResultValue("Verde", Color.FromName("Green").ToArgb());
            resultValueBuilder = new GrammarBuilder(choiceResultValue);
            colorChoice.Add(resultValueBuilder);

            SemanticResultKey choiceResultKey = new SemanticResultKey("rgb", colorChoice);
            GrammarBuilder colores = new GrammarBuilder(choiceResultKey);


            GrammarBuilder poner= "Poner";
            GrammarBuilder cambiar ="Cambiar";
            GrammarBuilder fondo = "Fondo";

            Choices dos_alternativas = new Choices(poner, cambiar);
            GrammarBuilder frase = new GrammarBuilder(dos_alternativas);
            frase.Append(fondo);
            frase.Append(colores);
            Grammar grammar = new Grammar(frase);            
            grammar.Name = "Poner/Cambiar Fondo";

            //Grammar grammar = new Grammar("so.xml.txt");
 
            return grammar;


       
        }
    }
}