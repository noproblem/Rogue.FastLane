﻿using Rogue.FastLane.Collections.Items;
using Rogue.FastLane.Queries;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rogue.FastLane.Collections.Mixins;
using Rogue.FastLane.Items;
using Rogue.FastLane.Infrastructure;
using Rogue.FastLane.Queries.States;
using Rogue.FastLane.Queries.Dispatchers;
using Rogue.FastLane.Config;

namespace Rogue.FastLane.Collections
{
    public class OptmizedStructure<TItem> : BasicStructure<TItem>
    {
        public OptmizedStructure(params IQuery<TItem>[] queries)
            : base(queries) { }

        protected ValueNode<TItem> CurrentNode;

        public int Count { get; set; }

        public void Add(TItem item)
        {
            var node =
                new ValueNode<TItem>
                {
                    Value = item,
                };

            if (CurrentNode != null)
            {
                CurrentNode.Next = node;
                node.Prior = CurrentNode;
            }

            CurrentNode = node;

            Count++;

            //Parallel.ForEach(Queries, sel => sel.AfterAdd(node, newState));
            foreach (var dispatcher in Dispatchers)
            {
                dispatcher.AddNode(this, node);
            }            
        }


        public void Remove<TKey>(IQuery<TItem> selector)
        {
            var node =
                selector.First();

            if (node.Prior != null && node.Next != null)
            {
                var next =
                    node.Next;
                var prior =
                    node.Prior;

                next.Prior = prior;
                prior.Next = next;

                Count--;

                foreach (var dispatcher in Dispatchers)
                {
                    dispatcher.RemoveNode(this, node);
                }

                Task.Factory.StartNew(
                    () => GC.SuppressFinalize(node));
            }
        }
    }
}