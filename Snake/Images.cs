using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Snake
{
    public static class Images
    {
        public readonly static ImageSource SnakeEmpty = LoadSnakeImage("Empty.png");
        public readonly static ImageSource SnakeBody = LoadSnakeImage("Body.png");
        public readonly static ImageSource SnakeHead = LoadSnakeImage("Head.png");
        public readonly static ImageSource SnakeFood = LoadSnakeImage("Food.png");
        public readonly static ImageSource SnakeDeadBody = LoadSnakeImage("DeadBody.png");
        public readonly static ImageSource SnakeDeadHead = LoadSnakeImage("DeadHead.png");


        private static ImageSource LoadImage(string fileName)
        {
            return new BitmapImage(new Uri($"Assets/{fileName}", UriKind.Relative));
        }

        private static ImageSource LoadSnakeImage(string filename)
        {
            return new BitmapImage(new Uri($"Assets/SnakeAssets/{filename}", UriKind.Relative));
        }

    }
}
