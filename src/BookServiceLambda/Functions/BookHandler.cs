using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using BookService.Models;
using BookService.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BookService.Functions
{
    public class BookHandler
    {
        private readonly IBookRepository _bookRepository;

        public BookHandler()
        {
            var serviceProvider = new Startup().ConfigureServices();
            _bookRepository = serviceProvider.GetRequiredService<IBookRepository>();
        }

        public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                switch (request.HttpMethod.ToUpper())
                {
                    case "POST":
                        return await CreateBook(request);
                    case "GET":
                        return await GetBook(request);
                    case "PUT":
                        return await UpdateBook(request);
                    case "DELETE":
                        return await DeleteBook(request);
                    default:
                        return new APIGatewayProxyResponse { StatusCode = (int)HttpStatusCode.MethodNotAllowed, Body = "Método não suportado" };
                }
            }
            catch (Exception ex)
            {
                return new APIGatewayProxyResponse { StatusCode = (int)HttpStatusCode.InternalServerError, Body = $"Erro: {ex.Message}" };
            }
        }

        private async Task<APIGatewayProxyResponse> CreateBook(APIGatewayProxyRequest request)
        {
            var body = request.Body;
            using var jsonDocument = JsonDocument.Parse(body);
            var root = jsonDocument.RootElement;

            var book = new Book
            {
                Name = root.GetProperty("name").GetString(),
                Author = root.GetProperty("author").GetString(),
                PublishedDate = root.GetProperty("publishedDate").GetDateTime()
            };
            return new APIGatewayProxyResponse { StatusCode = 201, Body = JsonSerializer.Serialize(book) };
            if (book == null)
                return new APIGatewayProxyResponse { StatusCode = 400, Body = "Dados inválidos" };
            
            book.Id = Guid.NewGuid().ToString(); // Gera ID automaticamente
            await _bookRepository.SaveBookAsync(book);

            return new APIGatewayProxyResponse { StatusCode = 201, Body = JsonSerializer.Serialize(book) };
        }

      private async Task<APIGatewayProxyResponse> GetBook(APIGatewayProxyRequest request)
        {
            // Verificar se o id foi fornecido na query string
            if (request.QueryStringParameters == null)
            {
                // Se o id não foi fornecido, retornar todos os livros
                var books = await _bookRepository.GetAllBooksAsync(); // Método que retorna todos os livros
                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = JsonSerializer.Serialize(books) // Serializar a lista de livros para o corpo da resposta
                };
            }

            // Caso o id seja fornecido, buscar o livro específico
            request.QueryStringParameters.TryGetValue("id", out string bookId);
            var book = await _bookRepository.GetBookAsync(bookId);
            if (book == null)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 404,
                    Body = "Livro não encontrado"
                };
            }

            // Se o livro for encontrado, retornar o livro específico
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(book)
            };
        }

        private async Task<APIGatewayProxyResponse> UpdateBook(APIGatewayProxyRequest request)
        {
            // Obtendo o parâmetro 'id' da rota
            if (!request.PathParameters.TryGetValue("id", out string bookId))
                return new APIGatewayProxyResponse { StatusCode = 400, Body = "ID é obrigatório" };
            
            // Deserializar o corpo da requisição
            var body = request.Body;
            using var jsonDocument = JsonDocument.Parse(body);
            var root = jsonDocument.RootElement;
          

            List<string> missingProperties = new List<string>();

            if (!root.TryGetProperty("id", out var idProperty))
                missingProperties.Add("id");
            
            if (!root.TryGetProperty("name", out var nameProperty))
                missingProperties.Add("name");

            if (!root.TryGetProperty("author", out var authorProperty))
                missingProperties.Add("author");

            if (!root.TryGetProperty("publishedDate", out var publishedDateProperty))
                missingProperties.Add("publishedDate");

            // Se faltarem propriedades, retornar as que estão ausentes
            if (missingProperties.Any())
            {
                var missingPropertiesMessage = string.Join(", ", missingProperties);
                return new APIGatewayProxyResponse { StatusCode = 400, Body = $"Faltando as seguintes propriedades: {missingPropertiesMessage}" };
            }

            // Criar o objeto Book com os dados recebidos
            var book = new Book
            {
                Id = idProperty.GetString(),
                Name = nameProperty.GetString(),
                Author = authorProperty.GetString(),
                PublishedDate = publishedDateProperty.GetDateTime()
            };
            return new APIGatewayProxyResponse { StatusCode = 201, Body = JsonSerializer.Serialize(book) };

            if (book == null || string.IsNullOrEmpty(book.Id))
                return new APIGatewayProxyResponse { StatusCode = 400, Body = "Dados do livro inválidos" };

            // Chamar o repositório para atualizar o livro
            await _bookRepository.UpdateBookAsync(bookId, book);
            return new APIGatewayProxyResponse { StatusCode = 200, Body = "Livro atualizado com sucesso" };
        }

        private async Task<APIGatewayProxyResponse> DeleteBook(APIGatewayProxyRequest request)
        {
            // Obtendo o parâmetro 'id' da rota
            if (!request.PathParameters.TryGetValue("id", out string bookId))
                return new APIGatewayProxyResponse { StatusCode = 400, Body = "ID é obrigatório" };

            // Chamar o repositório para deletar o livro
            await _bookRepository.DeleteBookAsync(bookId);
            return new APIGatewayProxyResponse { StatusCode = 200, Body = "Livro deletado com sucesso" };
        }

    }
}
