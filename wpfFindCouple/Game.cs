namespace wpfFindCouple
{
    internal class Game
    {
        public int Level { get; set; }
        public int MonsterCount { get; set; }
        public int CoupleCount { get; set; }
        public int PartWin { get; set; }
        public int MaxSeconds { get; set; }

        public void DoReset()
        {
            Level = 0;
            MonsterCount = 0;
            CoupleCount = 0;
            PartWin = 0;
        }

        public void StartGame()
        {
            switch (Level)
            {
                case 1:
                    MonsterCount = CoupleCount = 2;
                    MaxSeconds = 20;
                    break;
                case 2:
                    MonsterCount = CoupleCount = 5;
                    MaxSeconds = 40;
                    break;
                case 3:
                    MonsterCount = CoupleCount = 10;
                    MaxSeconds = 80;
                    break;
            }
        }
    }
}