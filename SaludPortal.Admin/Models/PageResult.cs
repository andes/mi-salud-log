namespace SaludPortal.Admin.Models;

public class PageResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
}
