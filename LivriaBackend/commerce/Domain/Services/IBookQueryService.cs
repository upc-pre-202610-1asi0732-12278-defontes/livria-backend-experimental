using LivriaBackend.commerce.Domain.Model.Aggregates;
using LivriaBackend.commerce.Domain.Model.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LivriaBackend.commerce.Domain.Model.Services
{
    public interface IBookQueryService
    {
        Task<Book> Handle(GetBookByIdQuery query);
        Task<IEnumerable<Book>> Handle(GetAllBooksQuery query);
        Task<IEnumerable<Book>> Handle(GetDeletedBooksQuery query);
        Task<IEnumerable<Book>> Handle(GetBooksByGenreQuery query);
        Task<IEnumerable<Book>> Handle(GetRandomBooksQuery query);
    }
}