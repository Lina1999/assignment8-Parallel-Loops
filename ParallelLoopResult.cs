using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Parallel_Loops
{
    /// <summary>
    /// class ParallelLoopResult
    /// </summary>
    class ParallelLoopResult
    {
        /// <summary>
        /// List of tasks.
        /// </summary>
        private List<Task> tasks;

        /// <summary>
        /// Parameterless constructor.
        /// </summary>
        public ParallelLoopResult()
        {
            tasks = new List<Task>();
        }

        /// <summary>
        /// Parallel For with start and end indexes.
        /// </summary>
        /// <param name="fromInclusive"></param>
        /// <param name="toExclusive"></param>
        /// <param name="body"></param>
        public void ParallelFor(int fromInclusive, int toExclusive, Action<int> body)
        {
            for (int i = fromInclusive; i < toExclusive; ++i)
            {
                tasks.Add(Task.Factory.StartNew(state => body((int)state), i));
            }
            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// Parallel ForEach on data source.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="body"></param>
        public void ParallelForEach<TSource>(IEnumerable<TSource> source, Action<TSource> body)
        {
            foreach (var elem in source)
            {
                tasks.Add(Task.Factory.StartNew(state => body((TSource)state), elem));
            }
            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// Parallel ForEach with options on data source.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="parallelOptions"></param>
        /// <param name="body"></param>
        public void ParallelForEachWithOptions<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource> body)
        {
            var lcts = new myTaskScheduler(parallelOptions.MaxDegreeOfParallelism);
            TaskFactory factory = new TaskFactory(lcts);
            foreach (var elem in source)
            {
                tasks.Add(factory.StartNew(state => body((TSource)state), elem));
            }
            Task.WaitAll(tasks.ToArray());
        }

        private class myTaskScheduler : TaskScheduler
        {

            /// <summary>
            /// Maximum degree of parallelism.
            /// </summary>
            private int maxDegOfParallelism;

            /// <summary>
            /// Scheduler executing items count.
            /// </summary>
            private int running = 0;

            /// <summary>
            /// List of tasks.
            /// </summary>
            private LinkedList<Task> tasks = new LinkedList<Task>();

            /// <summary>
            /// Shows if this thread is executing tasks.(unique for each thread)
            /// </summary>
            [ThreadStatic]
            private static bool threadIsRunning;

            /// <summary>
            /// Constructor with maximum degree of parallelism.
            /// </summary>
            /// <param name="maxDegreeOfParallelism"></param>
            public myTaskScheduler(int maxDegOfParallelism)
            {
                this.maxDegOfParallelism = maxDegOfParallelism;
            }

            /// <summary>
            /// Adding task to scheduler list.
            /// </summary>
            /// <param name="task"></param>
            protected sealed override void QueueTask(Task task)
            {
                tasks.AddLast(task);
                if (running < maxDegOfParallelism)
                {
                    ++running;
                    run();
                }

            }

            /// <summary>
            /// Running task from scheduler. 
            /// </summary>
            private void run()
            {
                ThreadPool.QueueUserWorkItem(obj =>
                {
                    threadIsRunning = true;
                    while (true)
                    {
                        Task task;
                        lock (tasks)
                        {
                            if (tasks.Count == 0)
                            {
                                --running;
                                break;
                            }
                            task = tasks.First.Value;
                            tasks.RemoveFirst();
                        }
                        TryExecuteTask(task);
                    }
                    threadIsRunning = false;
                }, null);
            }

            /// <summary>
            ///  Trying to execute task on currently running thread. 
            /// </summary>
            /// <param name="task"></param>
            /// <param name="taskWasPreviouslyQueued"></param>
            /// <returns></returns>
            protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                if (!threadIsRunning)
                {
                    return false;
                }
                if (taskWasPreviouslyQueued)
                {
                    if (TryDequeue(task))
                    {
                        return TryExecuteTask(task);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return TryExecuteTask(task);
                }
            }

            /// <summary>
            /// Removing task from scheduler.
            /// </summary>
            /// <param name="task"></param>
            /// <returns></returns>
            protected sealed override bool TryDequeue(Task task)
            {
                return tasks.Remove(task);
            }

            /// <summary>
            /// Getting tasks to be executed.
            /// </summary>
            /// <returns></returns>
            protected sealed override IEnumerable<Task> GetScheduledTasks()
            {
                return tasks;
            }
        }
    }
}
