
namespace Code.Tools
{
    public class Timer
    {
        private float _max;
        private float _current;

        public Timer(float max)
        {
            _max = max;
        }

        public bool Update(float value)
        {
            _current += value;

            if (_current < _max)
            {
                return false;
            }
            
            _current = 0;
            return true;
        }

        public void Finish()
        {
            _current = _max;
        }
        
        public void Reset()
        {
            _current = 0;
        }
    }
}