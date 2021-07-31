using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rails.Collections
{
    /// <summary>
    /// A collection of ordered items. Allows insertion, peeking
    /// and popping the minimum weight item off the queue.
    /// </summary>
    /// <typeparam name="T">An IComparable type</typeparam>
    public class PriorityQueue<T> where T: IComparable<T>
    {
        private List<T> items;
        public PriorityQueue() => items = new List<T>();
        
        /// <summary>
        /// Get the minimum weight item on the queue.
        /// </summary>
        /// <returns>The minimum weight item, or type default if the queue is empty.</returns>
        public T Peek() => items.FirstOrDefault();

        /// <summary>
        /// Removes the minimum weight item off the queue, and returns it.
        /// </summary>
        /// <returns>The minimum weight item, or type default if the queue is empty.</returns>
        public T Pop()
        {
            var item = items.FirstOrDefault();

            if(items.Count > 0)
            {
                // Move the max-weight item to the top of the queue
                items[0] = items.Last();

                int index = 0;
                int childIndex = 1;

                bool traversed = true;
                
                // While the max-weight item is not balanced, continue swapping
                // its position with its children
                while(traversed)
                { 
                    traversed = false;
                    
                    // If the max-weight node is in an appropriate position
                    // end the loop
                    if(childIndex > items.Count - 1) 
                        break;
                    
                    // Select the min-weight child to compare with the parent
                    if(childIndex + 1 < items.Count && items[childIndex].CompareTo(items[childIndex + 1]) > 0)
                        childIndex += 1;
                    
                    // If the max-weight (parent) element is larger than the child element
                    // swap the elements and run the loop again.
                    if(items[index].CompareTo(items[childIndex]) > 0)
                    {
                        T temp = items[index];
                        items[index] = items[childIndex];
                        items[childIndex] = temp;

                        traversed = true;
                    }

                    index = childIndex;
                    childIndex = 2 * childIndex + 1;
                } 

                items.RemoveAt(items.Count - 1);
            }

            return item;
        }
        
        /// <summary>
        /// Adds a new item to the queue.
        /// </summary>
        /// <param name="item">The new item to insert into the queue.</param>
        public void Insert(T item)
        {
            // The new element's index
            int index = items.Count;

            // The parent of the maximum element in the queue
            int parent = Mathf.FloorToInt((index - 1) / 2);

            items.Add(item);
            
            // While the element is smaller than its parent, swap
            // the element with it's parents and compare with its
            // new parent
            while(items[index].CompareTo(items[parent]) < 0)
            {
                var temp = items[parent];
                items[parent] = items[index];
                items[index] = temp;

                index = parent;
                parent = Mathf.FloorToInt((index - 1) / 2);
            }
        }
    }
}
