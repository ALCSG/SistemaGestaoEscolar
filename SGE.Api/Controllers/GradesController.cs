using SGE.Data;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SGE.Api.Controllers
{
    [RoutePrefix("api/grades")]
    public class GradesController : ApiController
    {
        // GET api/grades
        // Lists all grades with resolved names.
        [Route("")]
        public IHttpActionResult GetAll()
        {
            using (var context = new DataClasses1DataContext())
            {
                var grades = context.vw_Grades
                    .OrderBy(g => g.StudentName)
                    .ThenBy(g => g.CourseName)
                    .ThenBy(g => g.Date)
                    .ToList();

                return Ok(grades);
            }
        }

        // GET api/grades/5
        // Returns a specific grade by Id.
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            using (var context = new DataClasses1DataContext())
            {
                var grade = context.vw_Grades.FirstOrDefault(g => g.GradeId == id);

                if (grade == null)
                {
                    return NotFound();
                }

                return Ok(grade);
            }
        }

        // GET api/grades/enrollment/5
        // Lists all grades for a specific enrollment.
        [Route("enrollment/{enrollmentId:int}")]
        public IHttpActionResult GetByEnrollment(int enrollmentId)
        {
            using (var context = new DataClasses1DataContext())
            {
                bool enrollmentExists = context.StudentEnrollments.Any(se => se.StudentEnrollmentId == enrollmentId);

                if (!enrollmentExists)
                {
                    return NotFound();
                }

                var grades = context.vw_Grades
                    .Where(g => g.EnrollmentId == enrollmentId)
                    .OrderBy(g => g.Date)
                    .ToList();

                return Ok(grades);
            }
        }

        // GET api/grades/student/5
        // Lists all grades for a specific student across all enrollments.
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

                var grades = context.vw_Grades
                    .Where(g => g.StudentId == studentId)
                    .OrderBy(g => g.CourseName)
                    .ThenBy(g => g.Date)
                    .ToList();

                return Ok(grades);
            }
        }

        // POST api/grades
        // Creates a new grade.
        [Route("")]
        public IHttpActionResult Create([FromBody] Grade newGrade)
        {
            if (newGrade == null)
            {
                return BadRequest("O corpo do pedido não pode estar vazio.");
            }

            if (newGrade.Value < 0 || newGrade.Value > 20)
            {
                return BadRequest("A nota deve estar entre 0 e 20.");
            }

            if (newGrade.Weight <= 0 || newGrade.Weight > 100)
            {
                return BadRequest("O peso deve estar entre 1 e 100.");
            }

            if (string.IsNullOrWhiteSpace(newGrade.Type))
            {
                return BadRequest("O tipo de avaliação é obrigatório.");
            }

            using (var context = new DataClasses1DataContext())
            {
                bool enrollmentExists = context.StudentEnrollments.Any(se => se.StudentEnrollmentId == newGrade.EnrollmentId);
                
                if (!enrollmentExists)
                {
                    return BadRequest($"Inscrição com Id {newGrade.EnrollmentId} não existe.");
                }

                context.Grades.InsertOnSubmit(newGrade);

                try
                {
                    context.SubmitChanges();
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }

                return Created($"api/grades/{newGrade.GradeId}", new
                {
                    newGrade.GradeId,
                    newGrade.EnrollmentId,
                    newGrade.Value,
                    newGrade.Weight,
                    newGrade.Type,
                    newGrade.Date
                });
            }
        }

        // PUT api/grades/5
        // Updates an existing grade.
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] Grade updatedGrade)
        {
            if (updatedGrade == null)
            {
                return BadRequest("O corpo do pedido não pode estar vazio.");
            }

            if (updatedGrade.Value < 0 || updatedGrade.Value > 20)
            {
                return BadRequest("A nota deve estar entre 0 e 20.");
            }

            if (updatedGrade.Weight <= 0 || updatedGrade.Weight > 100)
            {
                return BadRequest("O peso deve estar entre 1 e 100.");
            }

            if (string.IsNullOrWhiteSpace(updatedGrade.Type))
            {
                return BadRequest("O tipo de avaliação é obrigatório.");
            }

            using (var context = new DataClasses1DataContext())
            {
                var existing = context.Grades.FirstOrDefault(g => g.GradeId == id);
                
                if (existing == null)
                {
                    return NotFound();
                }

                bool enrollmentExists = context.StudentEnrollments.Any(se => se.StudentEnrollmentId == updatedGrade.EnrollmentId);
                
                if (!enrollmentExists)
                {
                    return BadRequest($"Inscrição com Id {updatedGrade.EnrollmentId} não existe.");
                }

                existing.EnrollmentId = updatedGrade.EnrollmentId;
                existing.Value = updatedGrade.Value;
                existing.Weight = updatedGrade.Weight;
                existing.Type = updatedGrade.Type;
                existing.Date = updatedGrade.Date;

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
                    existing.GradeId,
                    existing.EnrollmentId,
                    existing.Value,
                    existing.Weight,
                    existing.Type,
                    existing.Date
                });
            }
        }

        // DELETE api/grades/5
        // Deletes a grade.
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            using (var context = new DataClasses1DataContext())
            {
                var existing = context.Grades.FirstOrDefault(g => g.GradeId == id);
                
                if (existing == null)
                {
                    return NotFound();
                }

                context.Grades.DeleteOnSubmit(existing);

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