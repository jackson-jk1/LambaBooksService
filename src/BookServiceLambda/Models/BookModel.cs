using Amazon.DynamoDBv2.DataModel;

namespace BookService.Models
{
    [DynamoDBTable("BooksTable")]
    public class Book
    {
        [DynamoDBHashKey] // Chave primária
        public string Id { get; set; }// ID automático

        [DynamoDBProperty]
        public string Name { get; set; }

        [DynamoDBProperty]
        public string Author { get; set; }

        [DynamoDBProperty]
        public DateTime PublishedDate { get; set; }
    }
}
