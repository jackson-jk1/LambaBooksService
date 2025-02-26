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
            var book = JsonSerializer.Deserialize<Book>(request.Body);
            if (book == null)
                return new APIGatewayProxyResponse { StatusCode = 400, Body = "Dados inválidos" };

            book.Id = Guid.NewGuid().ToString(); // Gera ID automaticamente
            await _bookRepository.SaveBookAsync(book);

            return new APIGatewayProxyResponse { StatusCode = 201, Body = JsonSerializer.Serialize(book) };
        }

        private async Task<APIGatewayProxyResponse> GetBook(APIGatewayProxyRequest request)
        {
            if (!request.QueryStringParameters.TryGetValue("id", out string bookId))
                return new APIGatewayProxyResponse { StatusCode = 400, Body = "ID é obrigatório" };

            var book = await _bookRepository.GetBookAsync(bookId);
            if (book == null)
                return new APIGatewayProxyResponse { StatusCode = 404, Body = "Livro não encontrado" };

            return new APIGatewayProxyResponse { StatusCode = 200, Body = JsonSerializer.Serialize(book) };
        }

        private async Task<APIGatewayProxyResponse> UpdateBook(APIGatewayProxyRequest request)
        {   
            if(!request.QueryStringParameters.TryGetValue("id", out string bookId))
                return new APIGatewayProxyResponse { StatusCode = 400, Body = "ID é obrigatório" };

            var book = JsonSerializer.Deserialize<Book>(request.Body);
            if (book == null || string.IsNullOrEmpty(book.Id))
                return new APIGatewayProxyResponse { StatusCode = 400, Body = "ID é obrigatório" };

            await _bookRepository.UpdateBookAsync(bookId, book);
            return new APIGatewayProxyResponse { StatusCode = 200, Body = "Livro atualizado com sucesso" };
        }

        private async Task<APIGatewayProxyResponse> DeleteBook(APIGatewayProxyRequest request)
        {
            if (!request.QueryStringParameters.TryGetValue("id", out string bookId))
                return new APIGatewayProxyResponse { StatusCode = 400, Body = "ID é obrigatório" };

            await _bookRepository.DeleteBookAsync(bookId);
            return new APIGatewayProxyResponse { StatusCode = 200, Body = "Livro deletado com sucesso" };
        }
    }
}
