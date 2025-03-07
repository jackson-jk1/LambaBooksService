FROM public.ecr.aws/lambda/dotnet:8
COPY publish/ /var/task/
CMD ["BookService.Functions.BookHandler::Handler"]
