using SGE.Data;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SGE.Api.Controllers
{
    [RoutePrefix("api/events")]
    public class EventsController : ApiController
    {
        // GET api/events
        // Lists all events.
        [Route("")]
        public IHttpActionResult GetAll()
        {
            using (var context = new DataClasses1DataContext())
            {
                var events = context.vw_Events
                    .OrderBy(e => e.Date)
                    .ToList();

                return Ok(events);
            }
        }

        // GET api/events/5
        // Returns a specific event by Id.
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            using (var context = new DataClasses1DataContext())
            {
                var @event = context.vw_Events.FirstOrDefault(e => e.EventId == id);

                if (@event == null)
                {
                    return NotFound();
                }

                return Ok(@event);
            }
        }

        // GET api/events/5/participants
        // Lists all participants (students and professors) for a specific event.
        [Route("{id:int}/participants")]
        public IHttpActionResult GetParticipants(int id)
        {
            using (var context = new DataClasses1DataContext())
            {
                bool eventExists = context.Events.Any(e => e.EventId == id);
                
                if (!eventExists)
                {
                    return NotFound();
                }

                var participants = context.vw_EventParticipants
                    .Where(p => p.EventId == id)
                    .OrderBy(p => p.ParticipantType)
                    .ThenBy(p => p.ParticipantName)
                    .ToList();

                return Ok(participants);
            }
        }

        // POST api/events
        // Creates a new event.
        [Route("")]
        public IHttpActionResult Create([FromBody] Event newEvent)
        {
            if (newEvent == null)
            {
                return BadRequest("O corpo do pedido não pode estar vazio.");
            }

            if (string.IsNullOrWhiteSpace(newEvent.Name))
            {
                return BadRequest("Nome do evento é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(newEvent.Type))
            {
                return BadRequest("Tipo do evento é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(newEvent.Location))
            {
                return BadRequest("Local do evento é obrigatório.");
            }

            using (var context = new DataClasses1DataContext())
            {
                bool eventExists = context.Events.Any(e => e.Name == newEvent.Name && e.Date == newEvent.Date);

                if (eventExists)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Já existe um evento com este nome e data."));
                }

                context.Events.InsertOnSubmit(newEvent);

                try
                {
                    context.SubmitChanges();
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }

                return Created($"api/events/{newEvent.EventId}", new
                {
                    newEvent.EventId,
                    newEvent.Name,
                    newEvent.Type,
                    newEvent.Date,
                    newEvent.Location,
                    newEvent.Description
                });
            }
        }

        // PUT api/events/5
        // Updates an existing event.
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] Event updatedEvent)
        {
            if (updatedEvent == null)
            {
                return BadRequest("O corpo do pedido não pode estar vazio.");
            }

            if (string.IsNullOrWhiteSpace(updatedEvent.Name))
            {
                return BadRequest("Nome do evento é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(updatedEvent.Type))
            {
                return BadRequest("Tipo do evento é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(updatedEvent.Location))
            {
                return BadRequest("Local do evento é obrigatório.");
            }


            using (var context = new DataClasses1DataContext())
            {
                var existing = context.Events.FirstOrDefault(e => e.EventId == id);

                if (existing == null)
                {
                    return NotFound();
                }
                    

                bool conflict = context.Events.Any(e => e.Name == updatedEvent.Name && e.Date == updatedEvent.Date && e.EventId != id);

                if (conflict)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Já existe outro evento com este nome e data."));
                }

                existing.Name = updatedEvent.Name;
                existing.Type = updatedEvent.Type;
                existing.Date = updatedEvent.Date;
                existing.Location = updatedEvent.Location;
                existing.Description = updatedEvent.Description;

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
                    existing.EventId,
                    existing.Name,
                    existing.Type,
                    existing.Date,
                    existing.Location,
                    existing.Description
                });
            }
        }

        // DELETE api/events/5
        // Deletes an event.
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            using (var context = new DataClasses1DataContext())
            {
                var existing = context.Events.FirstOrDefault(e => e.EventId == id);

                if (existing == null)
                {  
                    return NotFound();
                }

                context.Events.DeleteOnSubmit(existing);

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

        // POST api/events/5/students/3
        // Adds a student as a participant in an event.
        [Route("{eventId:int}/students/{studentId:int}")]
        public IHttpActionResult AddStudent(int eventId, int studentId)
        {
            using (var context = new DataClasses1DataContext())
            {
                bool eventExists = context.Events.Any(e => e.EventId == eventId);

                if (!eventExists)
                {
                    return NotFound();
                }

                bool studentExists = context.Students.Any(s => s.StudentId == studentId);

                if (!studentExists)
                {
                    return BadRequest($"Aluno com Id {studentId} não existe.");
                }

                bool alreadyParticipating = context.EventStudents.Any(es => es.EventId == eventId && es.StudentId == studentId);

                if (alreadyParticipating)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Aluno já é participante neste evento."));
                }

                var eventStudent = new EventStudent
                {
                    EventId = eventId,
                    StudentId = studentId
                };

                context.EventStudents.InsertOnSubmit(eventStudent);

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

        // DELETE api/events/5/students/3
        // Removes a student from an event.
        [Route("{eventId:int}/students/{studentId:int}")]
        public IHttpActionResult RemoveStudent(int eventId, int studentId)
        {
            using (var context = new DataClasses1DataContext())
            {
                var existing = context.EventStudents.FirstOrDefault(es => es.EventId == eventId && es.StudentId == studentId);

                if (existing == null)
                {
                    return NotFound();
                }

                context.EventStudents.DeleteOnSubmit(existing);

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

        // POST api/events/5/professors/3
        // Adds a professor as a participant in an event.
        [Route("{eventId:int}/professors/{professorId:int}")]
        public IHttpActionResult AddProfessor(int eventId, int professorId)
        {
            using (var context = new DataClasses1DataContext())
            {
                bool eventExists = context.Events.Any(e => e.EventId == eventId);

                if (!eventExists)
                {
                    return NotFound();
                }

                bool professorExists = context.Professors.Any(p => p.ProfessorId == professorId);

                if (!professorExists)
                {
                    return BadRequest($"Professor com Id {professorId} não existe.");
                }

                bool alreadyParticipating = context.EventProfessors.Any(ep => ep.EventId == eventId && ep.ProfessorId == professorId);
                
                if (alreadyParticipating)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Professor já é participante neste evento."));
                }

                var eventProfessor = new EventProfessor
                {
                    EventId = eventId,
                    ProfessorId = professorId
                };

                context.EventProfessors.InsertOnSubmit(eventProfessor);

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

        // DELETE api/events/5/professors/3
        // Removes a professor from an event.
        [Route("{eventId:int}/professors/{professorId:int}")]
        public IHttpActionResult RemoveProfessor(int eventId, int professorId)
        {
            using (var context = new DataClasses1DataContext())
            {
                var existing = context.EventProfessors.FirstOrDefault(ep => ep.EventId == eventId && ep.ProfessorId == professorId);
                
                if (existing == null)
                {
                    return NotFound();
                }

                context.EventProfessors.DeleteOnSubmit(existing);

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