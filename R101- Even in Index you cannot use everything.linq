<Query Kind="Program">
  <NuGetReference Version="3.0.3660">RavenDB.Client</NuGetReference>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Raven.Client.Document</Namespace>
  <Namespace>Raven.Client.Indexes</Namespace>
  <Namespace>Raven.Abstractions.Indexing</Namespace>
</Query>

void Main()
{
	using (var store = new DocumentStore { Url = "http://localhost:8080/", DefaultDatabase = "ddd"})
	{
		store.Initialize();	
		using (var session = store.OpenSession())
		{				
			IndexCreation.CreateIndexes(typeof(Authors_ProblemIndex1).Assembly, store);
			
			session
				.Query<Authors_ProblemIndex1.ReduceResult, Authors_ProblemIndex1>()
				.Dump();
		}
	}
}

public class Authors_ProblemIndex1 : AbstractIndexCreationTask<Author, Authors_ProblemIndex1.ReduceResult>
{
	public class ReduceResult
	{
		public string Name {get; set;}
		public string Surname {get; set;}
		public int ValueWithError {get; set;}
	}

	public Authors_ProblemIndex1()
	{
		Map = authors => from author in authors	
		
						// the map or reduce functions must be referentially transparent, 
						// that is, for the same set of values, they always return the same results
						// where author.Books.Any(x => x.PublishedOn.Value < DateTime.Now)						
						
						
						// Please take a look at the index on the server which will quietly fail for some records
						select new ReduceResult() { 
							Name = author.Name,
							Surname = author.Surname,
							ValueWithError = 1 / (author.Books.Length - 2)
						};
							
		Stores.Add(x => x.Name, FieldStorage.Yes);
		Stores.Add(x => x.Surname, FieldStorage.Yes);	
		Stores.Add(x => x.ValueWithError, FieldStorage.Yes);	
	}
}

// Define other methods and classes here
public class Author {
	public string Name { get; set; }
	public string Surname { get; set; }
	public Book[] Books { get; set; }
}
public class Book {
	public string Title { get; set; }
	public string Genre { get; set; }
	public DateTime? PublishedOn { get; set; }
	public decimal? Price { get; set; }
}