using BookService.Models;

namespace BookService.Repositories
{
    public interface IBookRepository
    {
        Task SaveBookAsync(Book book);
        Task<Book?> GetBookAsync(string id);
        Task<List<Book>> GetAllBooksAsync();
        Task<Book> UpdateBookAsync(string id , Book book);
        Task DeleteBookAsync(string id);
    }
}
