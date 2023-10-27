using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameUtils.Profiling
{
    public class TimeLogger
    {
        public static TimeLogger Instance = new();

        public bool enabled = true;

        public double unitScale = 1f;

        private Stack<Scope> scopes = new Stack<Scope>();

        public void Push(string name)
        {
            if (!enabled)
            {
                return;
            }
            var lastName = scopes.TryPeek(out var s) ? s.name : "";
            scopes.Push(new Scope { stopwatch = new Stopwatch(), id = name, name = lastName + "/" + name });
            scopes.Peek().stopwatch.Start();
        }

        public void Pop(string name)
        {
            if (!enabled)
            {
                return;
            }
            var scope = scopes.Pop();
            if (scope.id != name)
            {
                throw new ArgumentException($"Unbalanced profiling scopes, expected: {scope.name}, got: {name}");
            }
            var time = scope.stopwatch.Elapsed;
            Debug.WriteLine(scope.name + ": " + $"{time} >> {time.TotalSeconds / unitScale}");
            scope.stopwatch.Stop();
        }

        private struct Scope 
        {
            public Stopwatch stopwatch;
            public string name; 
            public string id;
        }
    }
}
