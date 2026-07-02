using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SGE.Data;

namespace SGE.Api.Controllers
{
    [RoutePrefix("api/students")]
    public class StudentsController : ApiController
    {
        DataClasses1DataContext dc = new DataClasses1DataContext();

        // GET api/students
        // Lists all students with view vw_students to return corresponding StudentClassGroup too.
        [Route("")]
        public IHttpActionResult GetAll()
        {
            using (var context = dc)
            {
                var students = context.vw_Students
                    .OrderBy(s => s.Surname)
                    .ThenBy(s => s.Name)
                    .ToList();

                return Ok(students);
            }
        }

        // GET api/students/5
        // Returns a specific student through Id.
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            using (var context = dc)
            {
                var student = context.vw_Students.FirstOrDefault(s => s.StudentId == id);

                if (student == null)
                    return NotFound();

                return Ok(student);
            }
        }

        // POST api/students
        // Creates a new student.
        [Route("")]
        public IHttpActionResult Create([FromBody] Student newStudent)
        {
            if (newStudent == null)
                return BadRequest("O corpo do pedido não pode estar vazio.");

            if (string.IsNullOrWhiteSpace(newStudent.Name) || string.IsNullOrWhiteSpace(newStudent.Surname))
                return BadRequest("Nome e apelido são obrigatórios.");

            if (string.IsNullOrWhiteSpace(newStudent.Email) || !newStudent.Email.Contains("@"))
                return BadRequest("Email inválido.");

            if (string.IsNullOrWhiteSpace(newStudent.Contact))
                return BadRequest("Contacto é obrigatório.");

            if (string.IsNullOrWhiteSpace(newStudent.Address))
                return BadRequest("Morada é obrigatória.");

            using (var context = dc)
            {
                bool emailExists = context.Students.Any(s => s.Email == newStudent.Email);
                if (emailExists)
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Já existe um aluno com este email."));

                if (newStudent.GroupId.HasValue)
                {
                    bool groupExists = context.StudentClassGroups.Any(g => g.StudentClassGroupId == newStudent.GroupId.Value);
                    if (!groupExists)
                        return BadRequest($"A turma com Id {newStudent.GroupId} não existe.");
                }

                context.Students.InsertOnSubmit(newStudent);

                try
                {
                    context.SubmitChanges();
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }

                return Created($"api/students/{newStudent.StudentId}", newStudent);
            }
        }

        // PUT api/students/5
        // Updates existing student.
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] Student updatedStudent)
        {
            if (updatedStudent == null)
                return BadRequest("O corpo do pedido não pode estar vazio.");

            if (string.IsNullOrWhiteSpace(updatedStudent.Name) || string.IsNullOrWhiteSpace(updatedStudent.Surname))
                return BadRequest("Nome e apelido são obrigatórios.");

            if (string.IsNullOrWhiteSpace(updatedStudent.Email) || !updatedStudent.Email.Contains("@"))
                return BadRequest("Email inválido.");

            using (var context = dc)
            {
                var existing = context.Students.FirstOrDefault(s => s.StudentId == id);
                if (existing == null)
                    return NotFound();

                bool emailConflict = context.Students.Any(s => s.Email == updatedStudent.Email && s.StudentId != id);
                if (emailConflict)
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Já existe outro aluno com este email."));

                if (updatedStudent.GroupId.HasValue)
                {
                    bool groupExists = context.StudentClassGroups.Any(g => g.StudentClassGroupId == updatedStudent.GroupId.Value);
                    if (!groupExists)
                        return BadRequest($"A turma com Id {updatedStudent.GroupId} não existe.");
                }

                existing.Name = updatedStudent.Name;
                existing.Surname = updatedStudent.Surname;
                existing.DOB = updatedStudent.DOB;
                existing.Contact = updatedStudent.Contact;
                existing.Address = updatedStudent.Address;
                existing.Email = updatedStudent.Email;
                existing.GroupId = updatedStudent.GroupId;

                try
                {
                    context.SubmitChanges();
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }

                return Ok(existing);
            }
        }

        // DELETE api/students/5
        // Deletes a student.
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            using (var context = dc)
            {
                var existing = context.Students.FirstOrDefault(s => s.StudentId == id);
                if (existing == null)
                    return NotFound();

                context.Students.DeleteOnSubmit(existing);

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

        // GET api/students/5/grades
        // Lists a student's average in all enrolled courses.
        [Route("{id:int}/grades")]
        public IHttpActionResult GetGrades(int id)
        {
            using (var context = dc)
            {
                bool studentExists = context.Students.Any(s => s.StudentId == id);
                if (!studentExists)
                    return NotFound();

                var averages = context.vw_EnrollmentAverages
                    .Where(a => a.StudentId == id)
                    .ToList();

                return Ok(averages);
            }
        }
    }
}