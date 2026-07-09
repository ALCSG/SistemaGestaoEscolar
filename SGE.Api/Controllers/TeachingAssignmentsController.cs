using SGE.Data;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SGE.Api.Controllers
{
    [RoutePrefix("api/teachingassignments")]
    public class TeachingAssignmentsController : ApiController
    {
        // GET api/teachingassignments
        // Lists all teaching assignments.
        [Route("")]
        public IHttpActionResult GetAll()
        {
            using (var context = new DataClasses1DataContext())
            {
                var assignments = context.vw_TeachingAssignments
                    .OrderBy(ta => ta.GroupName)
                    .ThenBy(ta => ta.CourseName)
                    .ToList();

                return Ok(assignments);
            }
        }

        // GET api/teachingassignments/5
        // Returns a specific teaching assignment by Id.
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            using (var context = new DataClasses1DataContext())
            {
                var assignment = context.vw_TeachingAssignments.FirstOrDefault(ta => ta.TeachingAssignmentId == id);

                if (assignment == null)
                {
                    return NotFound();
                }

                return Ok(assignment);
            }
        }

        // GET api/teachingassignments/group/5
        // Lists all teaching assignments for a specific group.
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

                var assignments = context.vw_TeachingAssignments
                    .Where(ta => ta.GroupId == groupId)
                    .OrderBy(ta => ta.CourseName)
                    .ToList();

                return Ok(assignments);
            }
        }

        // GET api/teachingassignments/professor/5
        // Lists all teaching assignments for a specific professor.
        [Route("professor/{professorId:int}")]
        public IHttpActionResult GetByProfessor(int professorId)
        {
            using (var context = new DataClasses1DataContext())
            {
                bool professorExists = context.Professors.Any(p => p.ProfessorId == professorId);
                
                if (!professorExists)
                {
                    return NotFound();

                }

                var assignments = context.vw_TeachingAssignments
                    .Where(ta => ta.ProfessorId == professorId)
                    .OrderBy(ta => ta.GroupName)
                    .ThenBy(ta => ta.CourseName)
                    .ToList();

                return Ok(assignments);
            }
        }

        // POST api/teachingassignments
        // Creates a new teaching assignment.
        [Route("")]
        public IHttpActionResult Create([FromBody] TeachingAssignment newAssignment)
        {
            if (newAssignment == null)
            {
                return BadRequest("O corpo do pedido não pode estar vazio.");
            }

            using (var context = new DataClasses1DataContext())
            {
                bool professorExists = context.Professors.Any(p => p.ProfessorId == newAssignment.ProfessorId);
                
                if (!professorExists)
                {
                    return BadRequest($"Professor com Id {newAssignment.ProfessorId} não existe.");
                }

                bool groupExists = context.StudentClassGroups.Any(g => g.StudentClassGroupId == newAssignment.GroupId);
                
                if (!groupExists)
                {
                    return BadRequest($"Turma com Id {newAssignment.GroupId} não existe.");
                }

                bool courseExists = context.Courses.Any(c => c.Id == newAssignment.CourseId && c.IsDeleted == false);
                
                if (!courseExists)
                {
                    return BadRequest($"Disciplina com Id {newAssignment.CourseId} não existe ou está eliminada.");
                }

                bool assignmentExists = context.TeachingAssignments.Any(ta =>
                    ta.ProfessorId == newAssignment.ProfessorId &&
                    ta.GroupId == newAssignment.GroupId &&
                    ta.CourseId == newAssignment.CourseId);
                
                if (assignmentExists)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Esta atribuição de lecionação já existe."));
                }

                context.TeachingAssignments.InsertOnSubmit(newAssignment);

                try
                {
                    context.SubmitChanges();
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }

                return Created($"api/teachingassignments/{newAssignment.TeachingAssignmentId}", new
                {
                    newAssignment.TeachingAssignmentId,
                    newAssignment.ProfessorId,
                    newAssignment.GroupId,
                    newAssignment.CourseId
                });
            }
        }

        // PUT api/teachingassignments/5
        // Updates an existing teaching assignment.
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] TeachingAssignment updatedAssignment)
        {
            if (updatedAssignment == null)
                return BadRequest("O corpo do pedido não pode estar vazio.");

            using (var context = new DataClasses1DataContext())
            {
                var existing = context.TeachingAssignments.FirstOrDefault(ta => ta.TeachingAssignmentId == id);
                
                if (existing == null)
                {
                    return NotFound();
                }

                bool professorExists = context.Professors.Any(p => p.ProfessorId == updatedAssignment.ProfessorId);
                
                if (!professorExists)
                {
                    return BadRequest($"Professor com Id {updatedAssignment.ProfessorId} não existe.");
                }

                bool groupExists = context.StudentClassGroups.Any(g => g.StudentClassGroupId == updatedAssignment.GroupId);
                
                if (!groupExists)
                {
                    return BadRequest($"Turma com Id {updatedAssignment.GroupId} não existe.");
                }

                bool courseExists = context.Courses.Any(c => c.Id == updatedAssignment.CourseId && c.IsDeleted == false);
                
                if (!courseExists)
                {
                    return BadRequest($"Disciplina com Id {updatedAssignment.CourseId} não existe ou está eliminada.");
                }

                bool conflict = context.TeachingAssignments.Any(ta =>
                    ta.ProfessorId == updatedAssignment.ProfessorId &&
                    ta.GroupId == updatedAssignment.GroupId &&
                    ta.CourseId == updatedAssignment.CourseId &&
                    ta.TeachingAssignmentId != id);
                if (conflict)
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Esta atribuição de lecionação já existe."));

                existing.ProfessorId = updatedAssignment.ProfessorId;
                existing.GroupId = updatedAssignment.GroupId;
                existing.CourseId = updatedAssignment.CourseId;

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
                    existing.TeachingAssignmentId,
                    existing.ProfessorId,
                    existing.GroupId,
                    existing.CourseId
                });
            }
        }

        // DELETE api/teachingassignments/5
        // Deletes a teaching assignment.
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            using (var context = new DataClasses1DataContext())
            {
                var existing = context.TeachingAssignments.FirstOrDefault(ta => ta.TeachingAssignmentId == id);
                
                if (existing == null)
                {
                    return NotFound();
                }

                context.TeachingAssignments.DeleteOnSubmit(existing);

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