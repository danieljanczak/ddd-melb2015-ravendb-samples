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
			session
				.Query<Author>()
				.Where(author => author.Name == "Dan" && author.Surname == "Simmons")
				.Dump();
		}
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