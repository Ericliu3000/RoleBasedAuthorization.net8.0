using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace ContosoUniversityRBAC
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;

        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
        // 新增方法：获取属性的显示名称（通过表达式树）
        public string GetDisplayName<U>(Expression<Func<T, U>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                var property = memberExpression.Member as PropertyInfo;
                if (property != null)
                {
                    var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
                    return displayAttribute?.Name ?? property.Name;
                }
            }
            throw new ArgumentException("无效的表达式");
        }

        // 新增方法：获取属性的显示名称（通过属性名字符串）
        public string GetDisplayName(string propertyName)
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property == null)
                throw new ArgumentException($"属性 {propertyName} 不存在");

            var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.Name ?? property.Name;
        }
    }


}