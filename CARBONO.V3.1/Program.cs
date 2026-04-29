using Microsoft.EntityFrameworkCore;
using CARBONO.V3._1.Data;
using CARBONO.V3._1.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Banco de Dados SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=database.db"));

builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// --- INÍCIO DA POPULAÇÃO DO BANCO (SEED DATA) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    // Garante a criação do banco database.db
    context.Database.EnsureCreated();

    // Popula apenas se a tabela estiver vazia
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
}
// --- FIM DA POPULAÇÃO DO BANCO ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();