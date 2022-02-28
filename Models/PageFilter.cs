using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RussianSitesStatus.Models;

public class PaginationFilter
{
    [FromQuery(Name = "pageNumber")]
    public int PageNumber { get; }
    
    [FromQuery(Name = "pageSize")]
    public int PageSize { get; }

    [BindNever]
    public int CountToSkip => (PageNumber - 1) * PageSize;
    
    public PaginationFilter()
    {
        PageNumber = 1;
        PageSize = 1000;
    }
    
    public PaginationFilter(int pageNumber,int pageSize)
    {
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize > 1000 ? 1000 : pageSize;
    }
}