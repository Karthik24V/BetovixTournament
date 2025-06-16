namespace Tournament.Common.DTOs;
public class PagedResult<T>
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public ICollection<T> Items { get; set; }
}