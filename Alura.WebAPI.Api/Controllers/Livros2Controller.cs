using Alura.ListaLeitura.Modelos;
using Alura.ListaLeitura.Persistencia;
using Alura.WebAPI.Api.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Alura.ListaLeitura.Api.Controllers
{
    public static class LivroFiltroExtensions
    {
        public static IQueryable<Livro> AplicarFiltro(this IQueryable<Livro> query, LivroFiltro filtro)
        {
            if (filtro != null)
            {
                if (!string.IsNullOrEmpty(filtro.Titulo))
                {
                    query = query.Where(l => l.Titulo.Contains(filtro.Titulo));
                }

                if (!string.IsNullOrEmpty(filtro.Subtitulo))
                {
                    query = query.Where(l => l.Subtitulo.Contains(filtro.Subtitulo));
                }

                if (!string.IsNullOrEmpty(filtro.Autor))
                {
                    query = query.Where(l => l.Autor.Contains(filtro.Autor));
                }

                if (!string.IsNullOrEmpty(filtro.Lista))
                {
                    query = query.Where(l => l.Lista == filtro.Lista.ParaTipo());
                }
            }
            return query;
        }
    }

    [ApiController]
    [Authorize]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/livros")]
    public class Livros2Controller : ControllerBase
    {
        private readonly IRepository<Livro> _repo;

        public Livros2Controller(IRepository<Livro> repository)
        {
            _repo = repository;
        }

        [HttpGet]
        public IActionResult ListaDeLivros(
            [FromQuery] LivroFiltro filtro,
            [FromQuery] LivroOrdem ordem,
            [FromQuery] LivroPaginacao paginacao)
        {
            var livroPaginado = _repo.All
                .AplicarFiltro(filtro)
                .AplicaOrdem(ordem)
                .Select(l => l.ToApi())
                .ToLivroPaginado(paginacao);
            return Ok(livroPaginado);
        }

        [HttpGet("{id}")]
        public IActionResult Recuperar(int id)
        {
            var model = _repo.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            return Ok(model);
        }

        [HttpGet("{id}/capa")]
        public IActionResult ImagemCapa(int id)
        {
            byte[] img = _repo.All
                .Where(l => l.Id == id)
                .Select(l => l.ImagemCapa)
                .FirstOrDefault();
            if (img != null)
            {
                return File(img, "image/png");
            }
            return File("~/images/capas/capa-vazia.png", "image/png");
        }

        [HttpPost]
        public IActionResult Incluir([FromForm] LivroUpload model)
        {
            if (ModelState.IsValid)
            {
                var livro = model.ToLivro();
                 _repo.Incluir(livro);
                var uri = Url.Action("Recuperar", new { id = livro.Id });
                return Created(uri, livro); //201
            }
            return BadRequest(ErrorResponse.FromModelState(ModelState));//objeto de sobrecarga que representa o erro a partir do modelState
        }

        [HttpPut]
        public IActionResult Alterar([FromForm] LivroUpload model)
        {
            if (ModelState.IsValid)
            {
                var livro = model.ToLivro();
                if (model.Capa == null)
                {
                    livro.ImagemCapa = _repo.All
                        .Where(l => l.Id == livro.Id)
                        .Select(l => l.ImagemCapa)
                        .FirstOrDefault();
                }
                _repo.Alterar(livro);
                return Ok(); //200
            }
            return BadRequest();
        }

        [HttpDelete("{id}")]
        public IActionResult Remover(int id)
        {
            var model = _repo.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            _repo.Excluir(model);
            return NoContent(); //203
        }
    }
}
