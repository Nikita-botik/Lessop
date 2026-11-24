using System;

namespace Lessop
{
    public static class GlobalEvents
    {
        public static event Action? LessonsCountChanged;

        public static void RaiseLessonsCountChanged()
        {
            LessonsCountChanged?.Invoke();
        }
    }
}
