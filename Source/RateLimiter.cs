namespace GameServer.Source
{
    public class RateLimiter
    {
        private int _counter = 0;
        private DateTime _resetTime;

        public RateLimiter(int threshold, TimeSpan duration)
        {
            Threshold = threshold;
            Duration = duration;
            _resetTime = DateTime.UtcNow + duration;
        }

        public int Threshold { get; }
        public TimeSpan Duration { get; }

        public bool CheckLimit()
        {
            lock (this)
            {
                if (_resetTime <= DateTime.UtcNow)
                {
                    _counter = 0;
                    _resetTime = DateTime.UtcNow + Duration;
                }

                if (_counter < Threshold)
                {
                    _counter++;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
