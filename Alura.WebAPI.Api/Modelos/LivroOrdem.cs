using Alura.ListaLeitura.Modelos;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Alura.WebAPI.Api.Modelos
{
    public static class LivroOrdemExtensions
    {
        public static IQueryable<Livro> AplicaOrdem(this IQueryable<Livro> query, LivroOrdem ordem)
        {
            if(ordem != null)
            {
                //order by nao esta disponivel no system.linq
                //instalar System.Linq.Dynamic.Core
                query = query.OrderBy(ordem.OrdernarPor); 
            }
            return query;
        }

    }

    public class LivroOrdem
    {
        public string OrdernarPor { get; set; }
    }
}
