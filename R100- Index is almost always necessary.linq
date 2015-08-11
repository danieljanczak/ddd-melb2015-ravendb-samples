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
				.Where(x => 
					// normal comparisons are OK
					x.Name != "Alan" || x.Name == "Stephen" || x.Surname == "Simmons"
				)				
				.Where(x => 
					// even going deep into structure
					x.Books.Length <= 3
					&& x.Books.Any(y => y.Genre != "Comedy")
				)				
				.Where(x => 
					// You can query on dates
					x.Books.Any(y => y.PublishedOn.Value < DateTime.Now.AddYears(-10))
				)
				.Select(x => new {
					Name = x.Name, // ok, projection
					Surname = x.Surname, // ok, projection
					FullName = x.Name + " " + x.Surname, // not OK, no combining fields
					AnotherFullName = string.Join(" ", x.Name, x.Surname), // not in any way
					BooksLength = x.Books.Length, // no operations on collections
					PublishedInLast100Years = x.Books.Any(y => y.PublishedOn.Value.AddYears(100) > DateTime.Now) // No dates either
					// and some others will fail, quite often quietly
				})
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