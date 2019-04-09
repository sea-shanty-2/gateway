using System.Collections.Generic;
using Gateway.Models;
using GraphQL.Builders;
using GraphQL.Types.Relay.DataObjects;
using System.Linq;

namespace Gateway.GraphQL.Extensions
{
    public static class EnumerableExtensions
    {
        public static Connection<T> ToConnection<T, U>(this IEnumerable<T> entities, ResolveConnectionContext<U> context) where T : IEntity
        {
            IEnumerable<T> items;
            var totalCount = entities.Count();
            var pageSize = context.PageSize ?? totalCount;

            if (context.IsUnidirectional || context.After != null || context.Before == null)
            {
                if (context.After != null)
                {
                    items = entities
                        .SkipWhile(x => x.Id != context.After)
                        .Skip(1)
                        .Take(context.First ?? pageSize).ToList();
                }
                else
                {
                    items = entities
                        .Take(context.First ?? pageSize).ToList();
                }
            }
            else
            {
                if (context.Before != null)
                {
                    items = entities
                        .Reverse()
                        .SkipWhile(x => x.Id != context.Before)
                        .Skip(1)
                        .Take(context.Last ?? pageSize).ToList();
                }
                else
                {
                    items = entities
                        .Reverse()
                        .Take(context.Last ?? pageSize).ToList();
                }
            }

            var endCursor = totalCount > 0 ? items.Last().Id : null;

            return new Connection<T>
            {
                Edges = items.Select(x => new Edge<T>() { Cursor = x.Id, Node = x }).ToList(),
                TotalCount = totalCount,
                PageInfo = new PageInfo()
                {
                    StartCursor = items.FirstOrDefault()?.Id,
                    EndCursor = items.LastOrDefault()?.Id,
                    HasPreviousPage = items.FirstOrDefault()?.Id != entities.FirstOrDefault()?.Id,
                    HasNextPage = items.LastOrDefault()?.Id != entities.LastOrDefault()?.Id
                }
            };
        }
    }
}