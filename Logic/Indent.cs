namespace EFCodeGenerator.Logic
{
    public class Indent
    {
        private int _width;

        public void Increment()
        {
            _width += 4;
        }

        public void Decrement()
        {
            _width -= 4;
        }

        public override string ToString()
        {
            return new string(' ', _width);
        }
    }
}