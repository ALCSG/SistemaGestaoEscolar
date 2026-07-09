using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SGE.Data;

namespace SGE.Api.Controllers
{
    [RoutePrefix("api/courses")]
    public class CoursesController : ApiController
    {
        // GET api/courses
        // Lists all courses. Includes deleted courses (IsDeleted = true) so the client can decide what to show.
        [Route("")]
        public IHttpActionResult GetAll()
        {
            using (var context = new DataClasses1DataContext())
            {
                var courses = context.vw_Courses
                    .OrderBy(c => c.Name)
                    .ToList();

                return Ok(courses);
            }
        }

        // GET api/courses/active
        // Lists only active courses (IsDeleted = false).
        [Route("active")]
        public IHttpActionResult GetActive()
        {
            using (var context = new DataClasses1DataContext())
            {
                var courses = context.vw_Courses
                    .Where(c => c.IsDeleted == false)
                    .OrderBy(c => c.Name)
                    .ToList();

                return Ok(courses);
            }
        }

        // GET api/courses/5
        // Returns a specific course by Id.
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            using (var context = new DataClasses1DataContext())
            {
                var course = context.vw_Courses.FirstOrDefault(c => c.CourseId == id);

                if (course == null)
                {
                    return NotFound();
                }

                return Ok(course);
            }
        }

        // POST api/courses
        // Creates a new course.
        [Route("")]
        public IHttpActionResult Create([FromBody] Course newCourse)
        {
            if (newCourse == null)
            {
                return BadRequest("O corpo do pedido não pode estar vazio.");
            }

            if (string.IsNullOrWhiteSpace(newCourse.Name))
            {
                return BadRequest("Nome da disciplina obrigatório.");
            }

            if (newCourse.WeeklyHours <= 0)
            {
                return BadRequest("Carga horária semanal tem de ser superior a 0.");

            }

            using (var context = new DataClasses1DataContext())
            {
                bool nameExists = context.Courses.Any(c => c.Name == newCourse.Name);
                
                if (nameExists)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Disciplina com este nome já existente."));
                }

                newCourse.IsDeleted = false;

                context.Courses.InsertOnSubmit(newCourse);

                try
                {
                    context.SubmitChanges();
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }

                return Created($"api/courses/{newCourse.Id}", new
                {
                    CourseId = newCourse.Id,
                    newCourse.Name,
                    newCourse.WeeklyHours,
                    newCourse.IsDeleted
                });
            }
        }

        // PUT api/courses/5
        // Updates an existing course.
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] Course updatedCourse)
        {
            if (updatedCourse == null)
            {
                return BadRequest("O corpo do pedido não pode estar vazio.");
            }

            if (string.IsNullOrWhiteSpace(updatedCourse.Name))
            {
                return BadRequest("Nome da disciplina obrigatório.");
            }

            if (updatedCourse.WeeklyHours <= 0)
            {
                return BadRequest("Carga horária semanal tem de ser superior a 0.");
            }

            using (var context = new DataClasses1DataContext())
            {
                var existing = context.Courses.FirstOrDefault(c => c.Id == id);
                
                if (existing == null)
                {
                    return NotFound();
                }

                bool nameConflict = context.Courses.Any(c => c.Name == updatedCourse.Name && c.Id != id);
                
                if (nameConflict)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Disciplina com este nome já existente."));
                }

                existing.Name = updatedCourse.Name;
                existing.WeeklyHours = updatedCourse.WeeklyHours;

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
                    CourseId = existing.Id,
                    existing.Name,
                    existing.WeeklyHours,
                    existing.IsDeleted
                });
            }
        }

        // DELETE api/courses/5
        // Soft delete - marks the course as deleted (IsDeleted = true) instead of removing it.
        // This preserves historical data (enrollments, grades) that reference this course.
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            using (var context = new DataClasses1DataContext())
            {
                var existing = context.Courses.FirstOrDefault(c => c.Id == id);
                
                if (existing == null)
                {
                    return NotFound();
                }

                if (existing.IsDeleted)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Disciplina já eliminada."));

                }

                existing.IsDeleted = true;

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

        // PUT api/courses/5/restore
        // Restores a soft-deleted course (IsDeleted = false).
        [Route("{id:int}/restore")]
        public IHttpActionResult Restore(int id)
        {
            using (var context = new DataClasses1DataContext())
            {
                var existing = context.Courses.FirstOrDefault(c => c.Id == id);
                
                if (existing == null)
                {
                    return NotFound();
                }

                if (!existing.IsDeleted)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Disciplina não eliminada."));
                }

                existing.IsDeleted = false;

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
                    CourseId = existing.Id,
                    existing.Name,
                    existing.WeeklyHours,
                    existing.IsDeleted
                });
            }
        }
    }
}