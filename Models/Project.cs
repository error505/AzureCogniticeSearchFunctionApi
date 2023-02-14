using System;
using System.Collections.Generic;
using Azure.Search.Documents.Indexes;

namespace AzureCogniticeSearchApi.Models
{
	public class Project
	{
		[SimpleField(IsKey = true, IsFilterable = true)]
		public string Id { get; set; }
		[SearchableField(IsFilterable = true, IsSortable = true)]
		public string Name { get; set; }
		[SearchableField(IsFilterable = true, IsSortable = true)]
		public string Type { get; set; }
		[SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
		public string Category { get; set; }
		[SearchableField(IsFilterable = true, IsSortable = true)]
		public string Customer { get; set; }
		[SimpleField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
		public DateTimeOffset CreatedDate { get; set; }
	}
}
