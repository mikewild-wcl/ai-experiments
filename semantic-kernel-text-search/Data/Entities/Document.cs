using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace semantic_kernel_text_search.Data.Entities;

public class Document
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public DateTimeOffset CreationDate { get; set; }

    public virtual ICollection<DocumentContent> Contents { get; set; } = [];
}
