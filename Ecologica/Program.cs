using Microsoft.EntityFrameworkCore;
using Ecologica.Data;
using Ecologica.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração do Banco de Dados SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=database.db"));

builder.Services.AddControllersWithViews();

// 2. Configuração ESSENCIAL para a Session funcionar
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// 3. --- BLOCO ÚNICO DE POPULAÇÃO DO BANCO (SEED DATA) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    // context.Database.Migrate();

    // context.Database.EnsureCreated();

    // Popula as atividades principais se estiver vazio
    if (!context.Atividades.Any())
    {
        context.Atividades.AddRange(
            new Atividade { Nome = "Viagem de carro (Gasolina)", FatorEmissao = 0.21 },
            new Atividade { Nome = "Energia Elétrica (kWh)", FatorEmissao = 0.092 },
            new Atividade { Nome = "Consumo de Gás (GLP)", FatorEmissao = 2.9 },
            new Atividade { Nome = "Alimentação (Carne Bovina)", FatorEmissao = 27.0 },
            new Atividade { Nome = "Alimentação (Frango/Suíno)", FatorEmissao = 6.9 },
            new Atividade { Nome = "Alimentação (Vegetariana)", FatorEmissao = 2.0 },
            new Atividade { Nome = "Viagem de Avião", FatorEmissao = 0.25 }
        );
        context.SaveChanges();
    }

    // Verifica especificamente a atividade de lixo para não duplicar
    if (!context.Atividades.Any(a => a.Nome.Contains("Lixo")))
    {
        context.Atividades.Add(new Atividade
        {
            Nome = "Lixo Doméstico (Orgânico)",
            FatorEmissao = 0.5,
            UnidadeMedida = "kg"
        });
        context.SaveChanges();
    }
}

// 4. Configurações de Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// O UseSession DEVE vir depois do UseRouting e antes do UseAuthorization
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=database.db";
Console.WriteLine("--------------------------------------------------");
Console.WriteLine("O CAMINHO DO BANCO QUE O SISTEMA ESTÁ USANDO É: " + Path.GetFullPath("database.db"));
Console.WriteLine("--------------------------------------------------");




app.Run();