using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostCode
{
    public enum ParallelType
    {
        SingleSeparate,
        MultipleSeparate
    }

    public class GhostLine<T>
    {
        private List<GhostLine<T>> Consumers { get; set; }

        private BlockingCollection<T> WorkUnits { get; set; }

        public GhostLine<T> Parent { get; protected set; }

        public bool ExecuteInParallel { get; set; }

        // To pass lambdas for arbitrary processing
        public delegate T ProcessItemDelegate(T itemToProcess);
        public ProcessItemDelegate Process = delegate { return default(T); };

        // To notify outside processes when complete
        public delegate void ProcessCompletedHandler(object sender, EventArgs e);
        public event ProcessCompletedHandler ProcessCompleted = delegate { };

        public bool PassValuesIfNoProcessDelegate { get; set; }

        public GhostLine()
        {
            Consumers = new List<GhostLine<T>>();
            WorkUnits = new BlockingCollection<T>();
            ExecuteInParallel = true;
            PassValuesIfNoProcessDelegate = true;
        }

        ///<summary>
        ///Instructs child processes to stop processing work units once there are no work units left to be processed.
        ///</summary>
        public void CompleteAdding()
        {
            WorkUnits.CompleteAdding();
        }

        ///<summary>
        ///Appends another data work unit for processing.
        ///</summary>
        public void AddWorkUnit(T workUnit)
        {
            WorkUnits.Add(workUnit);
        }

        private bool DescendantDuplicate(GhostLine<T> candidate)
        {
            if (candidate == this)
            {
                return true;
            }
            else
            {
                foreach(var child in candidate.Consumers)
                {
                    if (DescendantDuplicate(child))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool AncestorDuplicate(GhostLine<T> candidate)
        {
            if (candidate.Parent == null)
            {
                return false;
            }
            if (candidate.Parent == this)
            {
                return true;
            }
            else
            {
                return AncestorDuplicate(candidate.Parent);
            }
        }

        ///<summary>
        ///Adds another ProducerConsumer to the list of processing consumers.
        ///<para>This method will throw a CycleException if a consumer would be tasked to be its own producer.</para>
        ///</summary>
        public void AddConsumer(GhostLine<T> consumer) {
            if (AncestorDuplicate(consumer) || DescendantDuplicate(consumer))
            {
                throw new CycleException("Adding this consumer would create a cycle.");
            }

            consumer.Parent = this;
            Consumers.Add(consumer);
        }

        ///<summary>
        ///Iterates through all the WorkUnits until the producer calls CompleteAdding() and all added WorkUnits have been processed.
        ///</summary>
        public void Begin()
        {
            // if the external process doesn't set up a process, just pass the value along
            if (ProcessCompleted.GetInvocationList().Count() == 0 && PassValuesIfNoProcessDelegate)
            {
                Process = w =>
                {
                    return w;
                };
            }

            Task.Run(() =>
            {
                InnerWorkLoop();
            });
        }

        private void InnerWorkLoop()
        {
            foreach (var workUnit in WorkUnits.GetConsumingEnumerable())
            {
                // requires WorkUnits to be empty and IsCompleted to be true
                var transformedWorkUnit = Process(workUnit);

                if (!EqualityComparer<T>.Default.Equals(transformedWorkUnit, default(T)))
                {
                    foreach (var consumer in Consumers)
                    {
                        consumer.WorkUnits.Add(transformedWorkUnit);
                    }
                }

                if (ExecuteInParallel)
                {
                    Parallel.ForEach(Consumers, (consumer) =>
                    {
                        consumer.Begin();
                    });
                }
                else
                {
                    foreach (var consumer in Consumers)
                    {
                        consumer.Begin();
                    }
                }
            }
            OnProcessCompleted(EventArgs.Empty);
        }

        ///<summary>
        ///This method is called when the Begin() method completes.
        ///</para>If the ProcessCompleted handler has any delegates they will be called here.</para>
        ///</summary>
        protected virtual void OnProcessCompleted(EventArgs e)
        {
            if (ProcessCompleted != null)
                ProcessCompleted(this, e);
        }
    }
}
