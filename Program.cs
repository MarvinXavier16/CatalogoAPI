using CatalogoAPI.Context;
using CatalogoAPI.Models;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));


        var app = builder.Build();

        // Definir endpoints de categorias.

        app.MapPost("/categorias", async (Categoria categoria, AppDbContext db) =>
        {
            db.Categorias.Add(categoria);
            await db.SaveChangesAsync();

            return Results.Created($"/Categorias/{categoria.CategoriaId}", categoria);
        })
             .WithTags("Categoria");


        app.MapGet("/categorias", async (AppDbContext db) => await db.Categorias.ToListAsync()).WithTags("Categoria");


        app.MapGet("/categorias/{id:int}", async (int id, AppDbContext db) =>
        {
            return await db.Categorias.FindAsync(id)
                is Categoria categoria
                        ? Results.Ok(categoria) :
                         Results.NotFound();

        })
             .WithTags("Categoria");


        app.MapPut("/categorias/{id:int}", async (int id, Categoria categoria, AppDbContext db) =>
        {
            if (categoria.CategoriaId != id)
            {
                return Results.BadRequest();
            }
            var categoriaDB = await db.Categorias.FindAsync(id);
            if (categoriaDB is null) return Results.NotFound();
            categoriaDB.Nome = categoria.Nome;
            categoriaDB.Descricao = categoria.Descricao;
            await db.SaveChangesAsync();
            return Results.Ok(categoriaDB);
        })
             .WithTags("Categoria");





        app.MapDelete("/categorias/{id:int}", async (int id, AppDbContext db) =>
        {
            var categoria = await db.Categorias.FindAsync(id);

            if (categoria is null)
                return Results.NotFound();

            db.Categorias.Remove(categoria);
            await db.SaveChangesAsync();
            return Results.NoContent();

        })
        .WithTags("Categoria");

        //-----Endpoints para produtos

        app.MapPost("/produtos", async (Produto produto, AppDbContext db) =>
        {
            db.Produtos.Add(produto);
            await db.SaveChangesAsync();

            return Results.Created($"/produtos/{produto.ProdutoId}", produto);

        })

        .Produces<Produto>(StatusCodes.Status201Created)
        .WithName("CriarNovoProduto")
        .WithTags("Produtos");


        app.MapGet("/produtos", async (AppDbContext db) =>
       await db.Produtos.ToListAsync())

       .Produces<List<Produto>>(StatusCodes.Status200OK)
       .WithTags("Produtos");


        app.MapGet("/produtos/{id:int}", async (int id, AppDbContext db) =>
        {
            return await db.Produtos.FindAsync(id)
                            is Produto produto
                            ? Results.Ok(produto)
                            : Results.NotFound("Produto não encontrado");
        })   
        
        .Produces<Produto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithTags("Produtos");

        app.MapPut("/produtos", async (int produtoId, string produtoNome, AppDbContext db) =>
        
        {
            var produtoDB = db.Produtos.SingleOrDefault(s => s.ProdutoId == produtoId);

            if (produtoDB == null) return Results.NotFound("Produto não encontrado");

            produtoDB.Nome = produtoNome;

            await db.SaveChangesAsync();
            return Results.Ok(produtoDB);


        })  .Produces<Produto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("AtualizaNomeProduto")
            .WithTags("Produtos");
        
        app.MapDelete("/produtos/{id:int}", async (int Id, AppDbContext db) =>

        {
            var produtoDB = await db.Produtos.FindAsync(Id);

            if (produtoDB == null)
            {
                return Results.NotFound("Produto não encontrado");
            }


            db.Produtos.Remove(produtoDB);
            await db.SaveChangesAsync();
            return Results.Ok(produtoDB);


        }).Produces<Produto>(StatusCodes.Status200OK)
                   .Produces(StatusCodes.Status404NotFound)
                   .WithName("DeletaNomeProduto")
                   .WithTags("Produtos");



        app.MapGet("/produtos/nome/{criterio}", (string criterio, AppDbContext db) =>
        {
            var produtosSelecionados = db.Produtos.Where(x => x.Nome
                                        .ToLower().Contains(criterio.ToLower()))
                                        .ToList();


            return produtosSelecionados.Count > 0
                            ? Results.Ok(produtosSelecionados)
                            : Results.NotFound(Array.Empty<Produto>());
        })

        .Produces<List<Produto>>(StatusCodes.Status200OK)
        .WithName ("ProdutoPorNomeCriterio")
        .WithTags ("Produtos");

        // Configure the HTTP request pipeline 
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.Run();
    }
}