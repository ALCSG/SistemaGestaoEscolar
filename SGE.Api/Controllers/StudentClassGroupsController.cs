using SGE.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SGE.Api.Controllers
{
    [RoutePrefix("api/studentclassgroups")]
    public class StudentClassGroupsController : ApiController
    {
        // GET api/<studentclassgroups>
        // Lists all student class groups.
        [Route("")]
        public IHttpActionResult GetAll()
        {
            using (var context = new DataClasses1DataContext())
            {
                var studentClassGroups = context.vw_StudentClassGroups.OrderBy(c => c.Name).ToList();

                return Ok(studentClassGroups);
            }
        }

        // GET api/<studentclassgroups>/5
        // Returns a specific group by Id.
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            using (var context = new DataClasses1DataContext())
            {
                var studentClassGroup = context.vw_StudentClassGroups.FirstOrDefault(s => s.StudentClassGroupId == id);

                if (studentClassGroup == null)
                {
                    return NotFound();
                }

                return Ok(studentClassGroup);
            }
        }

        // GET api/studentclassgroups/5/students
        // Lists all students in a specific group.
        [Route("{id:int}/students")]
        public IHttpActionResult GetStudents(int id)
        {
            using (var context = new DataClasses1DataContext())
            {
                bool groupExists = context.StudentClassGroups.Any(g => g.StudentClassGroupId == id);
                
                if (!groupExists)
                {
                    return NotFound();

                }

                var students = context.vw_Students
                    .Where(s => s.GroupId == id)
                    .OrderBy(s => s.Surname)
                    .ThenBy(s => s.Name)
                    .ToList();

                return Ok(students);
            }
        }

        // POST api/<studentclassgroups>
        // Creates a new group. 
        [Route("")]
        public IHttpActionResult Create([FromBody] StudentClassGroup newGroup)
        {
            if (newGroup == null)
            {
                return BadRequest("O corpo do pedido não pode estar vazio.");

            }

            if (string.IsNullOrWhiteSpace(newGroup.Name))
            {
                return BadRequest("Nome da turma obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(newGroup.Year))
            {
                return BadRequest("Ano da turma obrigatório");
            }

            using (var context = new DataClasses1DataContext())
            {
                bool nameExists = context.StudentClassGroups.Any(g => g.Name == newGroup.Name && g.Year == newGroup.Year);
                
                if (nameExists)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Turma com este nome no ano escolhido já existente."));
                }

                context.StudentClassGroups.InsertOnSubmit(newGroup);

                try
                {
                    context.SubmitChanges();
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }

                return Created($"api/studentclassgroups/{newGroup.StudentClassGroupId}", new
                {
                    newGroup.StudentClassGroupId,
                    newGroup.Name,
                    newGroup.Year,
                    newGroup.NightShift
                });
            }
        }

        // PUT api/studentclassgroups/5
        // Updates an existing group.
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] StudentClassGroup updatedGroup)
        {
            if (updatedGroup == null)
            {
                return BadRequest("O corpo do pedido não pode estar vazio.");
            }

            if (string.IsNullOrWhiteSpace(updatedGroup.Name))
            {
                return BadRequest("Nome da turma obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(updatedGroup.Year))
            {
                return BadRequest("Ano da turma obrigatório.");
            }

            using (var context = new DataClasses1DataContext())
            {
                var existing = context.StudentClassGroups.FirstOrDefault(g => g.StudentClassGroupId == id);
                
                if (existing == null)
                {
                    return NotFound();
                }

                bool nameConflict = context.StudentClassGroups.Any(g => g.Name == updatedGroup.Name && g.Year == updatedGroup.Year && g.StudentClassGroupId != id);
                
                if (nameConflict)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Turma com este nome no ano escolhido já existente."));
                }

                existing.Name = updatedGroup.Name;
                existing.Year = updatedGroup.Year;
                existing.NightShift = updatedGroup.NightShift;

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
                    existing.StudentClassGroupId,
                    existing.Name,
                    existing.Year,
                    existing.NightShift
                });
            }
        }

        // DELETE api/studentclassgroups/5
        // Deletes a student class group.
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            using (var context = new DataClasses1DataContext())
            {
                var existing = context.StudentClassGroups.FirstOrDefault(g => g.StudentClassGroupId == id);
                
                if (existing == null)
                {
                    return NotFound();
                }

                context.StudentClassGroups.DeleteOnSubmit(existing);

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
    }
}
