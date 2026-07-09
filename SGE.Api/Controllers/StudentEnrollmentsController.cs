using SGE.Data;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SGE.Api.Controllers
{
    [RoutePrefix("api/studentenrollments")]
    public class StudentEnrollmentsController : ApiController
    {
        // GET api/studentenrollments
        // Lists all enrollments with resolved names.
        [Route("")]
        public IHttpActionResult GetAll()
        {
            using (var context = new DataClasses1DataContext())
            {
                var enrollments = context.vw_StudentEnrollments
                    .OrderBy(se => se.StudentName)
                    .ThenBy(se => se.CourseName)
                    .ToList();

                return Ok(enrollments);
            }
        }

        // GET api/studentenrollments/5
        // Returns a specific enrollment by Id.
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            using (var context = new DataClasses1DataContext())
            {
                var enrollment = context.vw_StudentEnrollments.FirstOrDefault(se => se.StudentEnrollmentId == id);

                if (enrollment == null)
                {
                    return NotFound();
                }

                return Ok(enrollment);
            }
        }

        // GET api/studentenrollments/student/5
        // Lists all enrollments for a specific student.
        [Route("student/{studentId:int}")]
        public IHttpActionResult GetByStudent(int studentId)
        {
            using (var context = new DataClasses1DataContext())
            {
                bool studentExists = context.Students.Any(s => s.StudentId == studentId);
                
                if (!studentExists)
                {
                    return NotFound();
                }

                var enrollments = context.vw_StudentEnrollments
                    .Where(se => se.StudentId == studentId)
                    .OrderBy(se => se.CourseName)
                    .ToList();

                return Ok(enrollments);
            }
        }

        // GET api/studentenrollments/group/5
        // Lists all enrollments for a specific group.
        [Route("group/{groupId:int}")]
        public IHttpActionResult GetByGroup(int groupId)
        {
            using (var context = new DataClasses1DataContext())
            {
                bool groupExists = context.StudentClassGroups.Any(g => g.StudentClassGroupId == groupId);
                
                if (!groupExists)
                {
                    return NotFound();
                }

                var enrollments = context.vw_StudentEnrollments
                    .Where(se => se.GroupId == groupId)
                    .OrderBy(se => se.StudentName)
                    .ThenBy(se => se.CourseName)
                    .ToList();

                return Ok(enrollments);
            }
        }

        // POST api/studentenrollments
        // Creates a new enrollment.
        [Route("")]
        public IHttpActionResult Create([FromBody] StudentEnrollment newEnrollment)
        {
            if (newEnrollment == null)
            {
                return BadRequest("O corpo do pedido não pode estar vazio.");
            }

            using (var context = new DataClasses1DataContext())
            {
                bool studentExists = context.Students.Any(s => s.StudentId == newEnrollment.StudentId);
                
                if (!studentExists)
                {
                    return BadRequest($"Aluno com Id {newEnrollment.StudentId} não existe.");
                }

                bool groupExists = context.StudentClassGroups.Any(g => g.StudentClassGroupId == newEnrollment.GroupId);
                
                if (!groupExists)
                {
                    return BadRequest($"Turma com Id {newEnrollment.GroupId} não existe.");
                }

                bool courseExists = context.Courses.Any(c => c.Id == newEnrollment.CourseId && c.IsDeleted == false);
                
                if (!courseExists)
                {
                    return BadRequest($"Disciplina com Id {newEnrollment.CourseId} não existe ou está eliminada.");
                }

                bool alreadyEnrolled = context.StudentEnrollments.Any(se =>
                    se.StudentId == newEnrollment.StudentId &&
                    se.CourseId == newEnrollment.CourseId &&
                    se.GroupId == newEnrollment.GroupId);
                
                if (alreadyEnrolled)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Aluno já inscrito nesta disciplina e turma."));
                }

                context.StudentEnrollments.InsertOnSubmit(newEnrollment);

                try
                {
                    context.SubmitChanges();
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }

                return Created($"api/studentenrollments/{newEnrollment.StudentEnrollmentId}", new
                {
                    newEnrollment.StudentEnrollmentId,
                    newEnrollment.StudentId,
                    newEnrollment.CourseId,
                    newEnrollment.GroupId
                });
            }
        }

        // PUT api/studentenrollments/5
        // Updates an existing enrollment.
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] StudentEnrollment updatedEnrollment)
        {
            if (updatedEnrollment == null)
            {
                return BadRequest("O corpo do pedido não pode estar vazio.");
            }

            using (var context = new DataClasses1DataContext())
            {
                var existing = context.StudentEnrollments.FirstOrDefault(se => se.StudentEnrollmentId == id);
                
                if (existing == null)
                {
                    return NotFound();
                }

                bool studentExists = context.Students.Any(s => s.StudentId == updatedEnrollment.StudentId);
                
                if (!studentExists)
                {
                    return BadRequest($"Aluno com Id {updatedEnrollment.StudentId} não existe.");
                }

                bool groupExists = context.StudentClassGroups.Any(g => g.StudentClassGroupId == updatedEnrollment.GroupId);
                
                if (!groupExists)
                {
                    return BadRequest($"Turma com Id {updatedEnrollment.GroupId} não existe.");
                }

                bool courseExists = context.Courses.Any(c => c.Id == updatedEnrollment.CourseId && c.IsDeleted == false);
                
                if (!courseExists)
                {
                    return BadRequest($"Disciplina com Id {updatedEnrollment.CourseId} não existe ou está eliminada.");
                }

                bool conflict = context.StudentEnrollments.Any(se =>
                    se.StudentId == updatedEnrollment.StudentId &&
                    se.CourseId == updatedEnrollment.CourseId &&
                    se.GroupId == updatedEnrollment.GroupId &&
                    se.StudentEnrollmentId != id);
                
                if (conflict)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Aluno já inscrito nesta disciplina e turma."));
                }

                existing.StudentId = updatedEnrollment.StudentId;
                existing.CourseId = updatedEnrollment.CourseId;
                existing.GroupId = updatedEnrollment.GroupId;

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
                    existing.StudentEnrollmentId,
                    existing.StudentId,
                    existing.CourseId,
                    existing.GroupId
                });
            }
        }

        // DELETE api/studentenrollments/5
        // Deletes an enrollment.
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            using (var context = new DataClasses1DataContext())
            {
                var existing = context.StudentEnrollments.FirstOrDefault(se => se.StudentEnrollmentId == id);
                
                if (existing == null)
                {
                    return NotFound();
                }

                context.StudentEnrollments.DeleteOnSubmit(existing);

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