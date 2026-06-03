using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YourAppName.Core.Wrappers
{
    public static class QueryableExtensions
    {
        // ToPaginatedListAsync method is an Extension Method for IQueryable<T>
        public static async Task<PaginatedResult<T>> ToPaginatedListAsync<T>(
            this IQueryable<T> source, int pageNumber, int pageSize) where T : class
        {
            if (source == null) throw new Exception("Empty source");

            // Prevent negative or zero pages
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            // 1. Get the total count of items in the database
            int count = await source.CountAsync();

            if (count == 0) return PaginatedResult<T>.Success(new List<T>(), count, pageNumber, pageSize);

            // 2. Fetch only the requested chunk of data
            var items = await source.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            // 3. Return the nicely wrapped result
            return PaginatedResult<T>.Success(items, count, pageNumber, pageSize);
        }
    }
}