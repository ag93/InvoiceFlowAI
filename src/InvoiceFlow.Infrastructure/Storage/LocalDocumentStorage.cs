using InvoiceFlow.Application.Interfaces;

namespace InvoiceFlow.Infrastructure.Storage;

public sealed class LocalDocumentStorage : IDocumentStorage
{
    private readonly string _storageDirectory;

    public LocalDocumentStorage(string storageDirectory)
    {
        _storageDirectory = storageDirectory;
        Directory.CreateDirectory(_storageDirectory);
    }

    public async Task<Guid> SaveAsync(Stream content, CancellationToken cancellationToken)
    {
        var documentId = Guid.NewGuid();

        var filePath = Path.Combine(_storageDirectory, $"{documentId:N}.pdf");

        await using var fileStream = new FileStream(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        try
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }
        catch
        {
            fileStream.Dispose();
            File.Delete(filePath);
            throw;
        }

        return documentId;
    }

    public Task<Stream?> OpenReadAsync(Guid documentId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var filePath = Path.Combine(_storageDirectory, $"{documentId:N}.pdf");

        try
        {
            Stream stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                useAsync: true);

            return Task.FromResult<Stream?>(stream);
        }
        catch (FileNotFoundException)
        {
            return Task.FromResult<Stream?>(null);
        }
    }
}