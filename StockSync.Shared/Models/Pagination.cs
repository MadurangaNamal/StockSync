namespace StockSync.Shared.Models;

public record PagedResult<T>(IEnumerable<T> Items, int PageNumber, int PageSize, int TotalCount, int TotalPages, bool HasPreviousPage, bool HasNextPage)
{
    public static PagedResult<T> Create(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<T>(items, pageNumber, pageSize, totalCount, totalPages, pageNumber > 1, pageNumber < totalPages);
    }
}

public record PaginationParams
{
    private const int MaxPageSize = 50;
    private int _pageSize = 20;

    public int PageNumber { get; init; } = 1;

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}
