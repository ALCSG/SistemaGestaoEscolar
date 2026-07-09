using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SGE.Data;

namespace SGE.Api.Controllers
{
    [RoutePrefix("api/professors")]
    public class ProfessorsController : ApiController
    {
        // GET api/professors
        // Lists all professors ordered by surname.
        [Route("")]
        public IHttpActionResult GetAll()
        {
            using (var context = new DataClasses1DataContext())
            {
                var professors = context.vw_Professors
                    .OrderBy(p => p.Surname)
                    .ThenBy(p => p.Name)
                    .ToList();

                return Ok(professors);
            }
        }

        // GET api/professors/5
        // Returns a specific professor by Id.
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            using (var context = new DataClasses1DataContext())
            {
                var professor = context.vw_Professors.FirstOrDefault(p => p.ProfessorId == id);

                if (professor == null)
                {
                    return NotFound();
                }

                return Ok(professor);
            }
        }

        // POST api/professors
        // Creates a new professor.
        [Route("")]
        public IHttpActionResult Create([FromBody] Professor newProfessor)
        {
            if (newProfessor == null)
            {
                return BadRequest("O corpo do pedido não pode estar vazio..");
            }

            if (string.IsNullOrWhiteSpace(newProfessor.Name) || string.IsNullOrWhiteSpace(newProfessor.Surname))
            {
                return BadRequest("Nome e apelido são obrigatórios.");
            }

            if (string.IsNullOrWhiteSpace(newProfessor.Email) || !newProfessor.Email.Contains("@"))
            {
                return BadRequest("Email inválido.");
            }

            if (string.IsNullOrWhiteSpace(newProfessor.Contact))
            {
                return BadRequest("Contacto é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(newProfessor.Specialization))
            {
                return BadRequest("Área de ensino é obrigatório.");
            }

            using (var context = new DataClasses1DataContext())
            {
                bool emailExists = context.Professors.Any(p => p.Email == newProfessor.Email);
                
                if (emailExists)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Professor com este email já existente"));
                }

                context.Professors.InsertOnSubmit(newProfessor);

                try
                {
                    context.SubmitChanges();
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }

                return Created($"api/professors/{newProfessor.ProfessorId}", new
                {
                    newProfessor.ProfessorId,
                    newProfessor.Name,
                    newProfessor.Surname,
                    newProfessor.Contact,
                    newProfessor.Email,
                    newProfessor.Specialization
                });
            }
        }

        // PUT api/professors/5
        // Updates an existing professor.
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] Professor updatedProfessor)
        {
            if (updatedProfessor == null)
            {
                return BadRequest("O corpo do pedido não pode estar vazio.");
            }

            if (string.IsNullOrWhiteSpace(updatedProfessor.Name) || string.IsNullOrWhiteSpace(updatedProfessor.Surname))
            {
                return BadRequest("Nome e apelido são obrigatórios.");
            }

            if (string.IsNullOrWhiteSpace(updatedProfessor.Email) || !updatedProfessor.Email.Contains("@"))
            {
                return BadRequest("Email inválido");
            }

            if (string.IsNullOrWhiteSpace(updatedProfessor.Specialization))
            {
                return BadRequest("Área de ensino obrigatório.");
            }

            using (var context = new DataClasses1DataContext())
            {
                var existing = context.Professors.FirstOrDefault(p => p.ProfessorId == id);
                
                if (existing == null)
                {
                    return NotFound();
                }

                bool emailConflict = context.Professors.Any(p => p.Email == updatedProfessor.Email && p.ProfessorId != id);
                
                if (emailConflict)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Professor com este email já existente."));
                }

                existing.Name = updatedProfessor.Name;
                existing.Surname = updatedProfessor.Surname;
                existing.Contact = updatedProfessor.Contact;
                existing.Email = updatedProfessor.Email;
                existing.Specialization = updatedProfessor.Specialization;

                try
                {
                    context.SubmitChanges();
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }

                return Ok(new
                {
                    existing.ProfessorId,
                    existing.Name,
                    existing.Surname,
                    existing.Contact,
                    existing.Email,
                    existing.Specialization
                });
            }
        }

        // DELETE api/professors/5
        // Deletes a professor. 
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            using (var context = new DataClasses1DataContext())
            {
                var existing = context.Professors.FirstOrDefault(p => p.ProfessorId == id);
                
                if (existing == null)
                {
                    return NotFound();
                }

                context.Professors.DeleteOnSubmit(existing);

                try
                {
                    context.SubmitChanges();
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }

                return StatusCode(HttpStatusCode.NoContent);
            }
        }

        // GET api/professors/5/assignments
        // Lists all teaching assignments for a specific professor (courses and groups).
        [Route("{id:int}/assignments")]
        public IHttpActionResult GetAssignments(int id)
        {
            using (var context = new DataClasses1DataContext())
            {
                bool professorExists = context.Professors.Any(p => p.ProfessorId == id);
                
                if (!professorExists)
                {
                    return NotFound();
                }

                var assignments = context.TeachingAssignments
                    .Where(ta => ta.ProfessorId == id)
                    .Select(ta => new
                    {
                        ta.TeachingAssignmentId,
                        CourseName = ta.Course.Name,
                        GroupName = ta.StudentClassGroup.Name
                    })
                    .ToList();

                return Ok(assignments);
            }
        }
    }
}