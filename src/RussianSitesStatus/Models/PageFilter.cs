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
    public int CountToSkip => PageNumber * PageSize;
    
    public PaginationFilter()
    {
        PageNumber = 0;
        PageSize = 1000;
    }
    
    public PaginationFilter(int pageNumber,int pageSize)
    {
        PageNumber = pageNumber < 0 ? 0 : pageNumber;
        PageSize = pageSize > 1000 ? 1000 : pageSize;
    }
}