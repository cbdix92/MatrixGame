using System;
using System.Threading;
using System.Diagnostics;
using System.Windows.Input;

namespace MatrixGame
{
    class MatrixGame
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Game Loop flag
            bool gameOver = false;

            // Matrix Dimensions
            int matrixWidth = 10;
            int matrixHeight = 20;

            // Matrix parameters
            int goalOffset = 3;
            int maximunActiveSprites = 20;

            // Matrix Instance
            Matrix matrix = new Matrix(matrixWidth, matrixHeight, goalOffset, maximunActiveSprites);

            // FPSCounter Instance
            SpeedController speedController = new SpeedController();

            // Game Loop
            while (!gameOver)
            {

                // Check for player key input
                matrix.KeyDownHandler();

                // Draw Matrix to screen
                if (speedController.ControlCall(300))
                {
                    matrix.Draw();
                }

                // Escape loop upon ESC key press
                if (Keyboard.IsKeyDown(Key.Escape))
                {
                    gameOver = true;
                }

                // Wait to loop.
                speedController.Wait(200);
            }
        }
    }

    class SpeedController
    {
        // Current rate in milliseconds of screen refresh
        private int refreshRate;

        // Updates per second
        private int updateCounter;

        private Stopwatch loopTimer;

        private Stopwatch drawTimer;

        public SpeedController()
        {
            // CONSTRUCTOR
            //

            // loopTimer Instance (See System.Diagnostics)
            this.loopTimer = new Stopwatch();
            this.loopTimer.Start();

            // drawTimer Instance (See System.Diagnostics)
            this.drawTimer = new Stopwatch();
            this.drawTimer.Start();

            // Member variables
            this.refreshRate = 10; // Initial refresh rate in milliseconds
            this.updateCounter = 0;
        }

        public void Wait(int loopSpeed)
        {
            // @loopSpeed -> Targeted loops per second
            // Increment the update counter
            updateCounter++;
            // Slow GameLoop
            Thread.Sleep(refreshRate);
            // Check stopWatch to if one second has passed yet
            if (loopTimer.ElapsedMilliseconds >= 1000)
            {
                // Reset loopTimer
                loopTimer.Reset();
                loopTimer.Start();

                // Adjust refreshRate
                if (updateCounter > loopSpeed)
                {
                    refreshRate += 10;
                }
                else
                {
                    refreshRate -= 10;
                }
                updateCounter = 0;
            }
        }

        public bool ControlCall(int frequency)
        {
            if (drawTimer.ElapsedMilliseconds >= frequency)
            {
                // Reset drawTimer
                drawTimer.Reset();
                drawTimer.Start();

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    class Sprite
    {
        // Random and letterKey Instance shared across all Sprite Instances
        static Random rand = new Random();

        // Sprite letter to be drawn to the screen
        public char letter;
        static string letterKey = "123";

        // Position of the Sprite within the matrix
        public int posX;
        public int posY;

        // Is the Sprite currently spawned
        bool alive = false;

        // Width of the matrix used for assigning random x position (See Sprite.Spawn)
        int matrixWidth;


        public Sprite(int matrixWidth)
        {
            // CONSTRUCTOR
            //

            this.matrixWidth = matrixWidth;

        }

        public bool Alive
        {
            set { this.alive = value; }
            get { return alive; }
        }

        public void Spawn()
        {
            this.Alive = true;

            // Set random X position
            this.posX = rand.Next(matrixWidth);

            // Assign random letter based on randomly indexed letter key.
            this.letter = letterKey[rand.Next(letterKey.Length)];

        }

        public void Kill()
        {
            this.Alive = false;
            this.posX = 0;
            this.posY = 0;
        }
    }

    class Matrix
    {
        // Player score
        int score = 0;

        // Matrix dimensions
        private int matrixWidth;
        private int matrixHeight;

        // Position of the goal line
        private int GOAL_OFFSET;

        // Max Number of active sprites
        int maximunActiveSprites;
        int activeSprites;

        // Matrix and spriteList Array
        private string[,] matrix;
        private Sprite[] spriteList;


        public Matrix(int matrixWidth, int matrixHeight, int goalOffset, int maximunActiveSprites)
        {
            // CONSTRUCTOR method for Matrix Class
            //

            // Class member variables
            this.matrixWidth = matrixWidth;
            this.matrixHeight = matrixHeight;
            this.GOAL_OFFSET = matrixHeight - goalOffset;
            this.maximunActiveSprites = maximunActiveSprites;
            this.activeSprites = 0;

            // Matrix Array Instance
            this.matrix = new string[matrixHeight, matrixWidth];

            // Initial Matrix Fill
            this.Refresh();

            // Initialize spriteList
            this.spriteList = new Sprite[maximunActiveSprites];
            for (int index = 0; index < spriteList.Length; index++)
            {
                spriteList[index] = new Sprite(matrixWidth);
            }
        }

        private void Refresh()// Resets the matrix. This is done prior to adding updated sprites back into the matrix.
        {
            // Fill Matrix with blank space and create goal line
            for (int row = 0; row < matrixHeight; row++)
            {
                for (int column = 0; column < matrixWidth; column++)
                {
                    // Create the Goal line
                    if (row == GOAL_OFFSET)
                    {
                        matrix[row, column] = "***";
                    }
                    else
                    {
                        matrix[row, column] = "   ";
                    }
                }
            }
        }

        public void Draw()
        {
            // Refresh the matrix
            Refresh();

            // Update the spriteList
            UpdateSpriteList();

            // Add living sprites into the matrix
            for (int item = 0; item < spriteList.Length; item++)
            {
                if (spriteList[item].Alive == true)
                {
                    matrix[spriteList[item].posY, spriteList[item].posX] = " " + Convert.ToString(spriteList[item].letter) + " ";
                }
            }

            // Clear the command window (Must clear the window right before drawing to reduce flickering!)
            Console.Clear();

            // Draw Matrix to the screen
            for (int row = 0; row < matrixHeight; row++)
            {
                for (int column = 0; column < matrixWidth; column++)
                {
                    Console.Write(matrix[row, column]);
                }
                Console.WriteLine();
            }
            Console.WriteLine("Score: " + score);
        }

        public void UpdateSpriteList()
        {
            // Spawn one sprite if available per method call.
            // Move all living sprites down one cordinate and kill sprites that move past the goal line.
            //

            if (activeSprites < maximunActiveSprites)
            {
                // Check for a dead sprite and bring it back to life
                for (int item = 0; item < spriteList.Length; item++)
                {
                    if (!spriteList[item].Alive)
                    {
                        spriteList[item].Spawn();
                        break;
                    }
                }

                // Move all living sprites down one, and kill sprites that pass the goal line.
                for (int item = 0; item < spriteList.Length; item++)
                {

                    if (spriteList[item].Alive)
                    {
                        spriteList[item].posY += 1;
                    }

                    // Sprite has passed the goal. Kill it and decrement the score.
                    if (spriteList[item].posY > GOAL_OFFSET + 2)
                    {
                        spriteList[item].Kill();
                        score--;
                    }
                }
            }
        }

        public void KeyDownHandler()
        {
            if (Keyboard.IsKeyDown(Key.NumPad1))
            {
                CheckGoal(Convert.ToChar("1"));
            }
            else if (Keyboard.IsKeyDown(Key.NumPad2))
            {
                CheckGoal(Convert.ToChar("2"));
            }
            else if (Keyboard.IsKeyDown(Key.NumPad3))
            {
                CheckGoal(Convert.ToChar("3"));
            }
        }

        // Called by KeyDownHandler() when a key is pressed.
        // Target is passed as the pressed key.
        public void CheckGoal(char target)
        {
            // Iterate through all sprites
            for (int item = 0; item < spriteList.Length; item++)
            {
                // Check if the current sprite is in the goal line
                if (spriteList[item].posY == GOAL_OFFSET || spriteList[item].posY == GOAL_OFFSET + 1)
                {
                    if (spriteList[item].letter == target)
                    {
                        spriteList[item].Kill();
                        score++;
                    }
                }
            }
        }
    }
}