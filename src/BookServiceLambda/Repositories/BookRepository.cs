using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using BookService.Models;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace BookService.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly DynamoDBContext _context;
        private readonly IAmazonSimpleSystemsManagement _ssmClient;

        // Instância estática para a conexão com DynamoDB
        private static IAmazonDynamoDB? _dynamoDbClient;
        
        // Construtor da classe BookRepository
        public BookRepository(IAmazonSimpleSystemsManagement ssmClient)
        {
            _ssmClient = ssmClient;

            // Inicializa o cliente DynamoDB apenas na primeira invocação
            if (_dynamoDbClient == null)
            {
                _dynamoDbClient = InitializeDynamoDbClient().Result;
                _context = new DynamoDBContext(_dynamoDbClient);
            }
            else
            {
                _context = new DynamoDBContext(_dynamoDbClient);
            }
        }

        // Método que busca o parâmetro de conexão do SSM (Parameter Store)
        private async Task<IAmazonDynamoDB> InitializeDynamoDbClient()
        {
            var parameter = await _ssmClient.GetParameterAsync(new GetParameterRequest
            {
                Name = "BooksTable",
                WithDecryption = true
            });

            string connectionString = parameter.Parameter.Value;
            var dynamoDbClient = new AmazonDynamoDBClient(); // Pode incluir o endpoint de connection string, se necessário.
            return dynamoDbClient;
        }

        public async Task SaveBookAsync(Book book) => await _context.SaveAsync(book);

        public async Task<Book?> GetBookAsync(string id) => await _context.LoadAsync<Book>(id);

        public async Task<Book> UpdateBookAsync(string id, Book book)
        {
            var existingBook = await _context.LoadAsync<Book>(id);
            if (existingBook == null)
            {
                return null; // Retorna null se o livro não for encontrado
            }

            // Atualiza as propriedades do livro existente com os novos valores
            existingBook.Name = book.Name;
            existingBook.Author = book.Author;
            existingBook.PublishedDate = book.PublishedDate;

            await _context.SaveAsync(existingBook); // Salva o livro atualizado no banco

            return existingBook; // Retorna o livro atualizado
        }

        public async Task<List<Book>> GetAllBooksAsync() => await _context.ScanAsync<Book>(new List<ScanCondition>()).GetRemainingAsync();

        public async Task DeleteBookAsync(string id) => await _context.DeleteAsync<Book>(id);
    }
}
