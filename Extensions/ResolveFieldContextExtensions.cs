using Gateway.Interfaces;
using GraphQL.Builders;
using GraphQL.Types.Relay.DataObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Gateway.Extensions
{
    public static class ResolveFieldContextExtensions
    {
        public static Connection<U> GetPagedResults<T, U>(this ResolveConnectionContext<T> context, IEnumerable<U> data) where U : IEntity
        {
            IEnumerable<U> items;
            var pageSize = context.PageSize ?? 20;

            if (context.IsUnidirectional || context.After != null || context.Before == null)
            {
                if (context.After != null)
                {
                    items = data
                        .SkipWhile(x => x.Id != context.After)
                        .Skip(1)
                        .Take(context.First ?? pageSize).ToList();
                }
                else
                {
                    items = data
                        .Take(context.First ?? pageSize).ToList();
                }
            }
            else
            {
                if (context.Before != null)
                {
                    items = data
                        .Reverse()
                        .SkipWhile(x => x.Id != context.Before)
                        .Skip(1)
                        .Take(context.Last ?? pageSize).ToList();
                }
                else
                {
                    items = data
                        .Reverse()
                        .Take(context.Last ?? pageSize).ToList();
                }
            }
    
            var endCursor = data.Count() > 0 ? items.Last().Id : null;

            return new Connection<U>
            {
                Edges = items.Select(x => new Edge<U>() { Cursor = x.Id, Node = x }).ToList(),
                TotalCount = data.Count(),
                PageInfo = new PageInfo()
                {
                    StartCursor = items.FirstOrDefault()?.Id,
                    EndCursor = items.LastOrDefault()?.Id,
                    HasPreviousPage = items.FirstOrDefault()?.Id != data.FirstOrDefault()?.Id,
                    HasNextPage = items.LastOrDefault()?.Id != data.LastOrDefault()?.Id
                }
            };
        }
    }
}