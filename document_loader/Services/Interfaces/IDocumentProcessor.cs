using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace document_loader.Services.Interfaces;

public interface IDocumentProcessor
{
    IAsyncEnumerable<string> ProcessDocument(string? path);

    Task<string> Test(string? path);
}
