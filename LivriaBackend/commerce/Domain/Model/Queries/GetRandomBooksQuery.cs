using System.Collections.Generic;

namespace LivriaBackend.commerce.Domain.Model.Queries
{
    public record GetRandomBooksQuery(int Count, List<int>? Exclude = null);
}
