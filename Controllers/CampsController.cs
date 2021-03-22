using System;
using System.Linq;
using System.Threading.Tasks;
using AspRestApiWorkshop.Models;
using AutoMapper;
using CoreCodeCamp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AspRestApiWorkshop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiConventionType(typeof(CustomConventions))]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _campRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public CampsController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _campRepository = campRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        /// <summary>
        /// List all conferences.
        /// </summary>
        /// <param name="includeTalks">Talks should also be displayed.</param>
        /// <returns>All available conferences from our database.</returns>
        [HttpGet]
        public async Task<ActionResult<CampModel[]>> GetCamps(bool includeTalks = false)
        {
            try
            {
                var results = await _campRepository.GetAllCampsAsync(includeTalks);
                CampModel[] campModels = _mapper.Map<CampModel[]>(results);

                return campModels.Select(camp => CreateLinksForCamp(camp)).ToArray();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        /// <summary>
        /// Show a specific conference.
        /// </summary>
        /// <param name="moniker">Conference abbreviation.</param>
        /// <returns>Available conference from our database.</returns>
        /// <response code="200">Returns the requested conference.</response>
        /// <remarks>
        /// Sample request (this request list an specific conference.) \
        /// GET /api/camps/DWX2020
        /// </remarks>
        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> GetCamp(string moniker)
        {
            try
            {
                var result = await _campRepository.GetCampAsync(moniker);

                if (result == null)
                {
                    return NotFound();
                }

                return CreateLinksForCamp(_mapper.Map<CampModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var result = await _campRepository.GetAllCampsByEventDate(theDate, includeTalks);

                if (!result.Any())
                {
                    return NotFound();
                }

                return _mapper.Map<CampModel[]>(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPost]
        //[ApiConventionMethod(typeof(CustomConventions), nameof(CustomConventions.Insert))]
        public async Task<ActionResult<CampModel>> Insert(CampModel campModel)
        {
            try
            {
                var existingCamp = await _campRepository.GetCampAsync(campModel.Moniker);
                if (existingCamp != null)
                {
                    return BadRequest("Moniker is in Use");
                }

                var location = _linkGenerator.GetPathByAction("GetCamp", "Camps", new { moniker = campModel.Moniker });

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker");
                }

                var camp = _mapper.Map<Camp>(campModel);
                _campRepository.Add(camp);

                if (await _campRepository.SaveChangesAsync())
                {
                    return Created(location, _mapper.Map<CampModel>(camp));
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }

            return BadRequest();
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel campModel)
        {
            try
            {
                var oldCamp = await _campRepository.GetCampAsync(moniker);
                if (oldCamp == null)
                {
                    return NotFound($"Could not find camp with moniker of {moniker}");
                }

                _mapper.Map(campModel, oldCamp);

                if (await _campRepository.SaveChangesAsync())
                {
                    return _mapper.Map<CampModel>(oldCamp);
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }

            return BadRequest();
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var oldCamp = await _campRepository.GetCampAsync(moniker);
                if (oldCamp == null)
                {
                    return NotFound();
                }

                _campRepository.Delete(oldCamp);

                if (await _campRepository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }

            return BadRequest();
        }

        private CampModel CreateLinksForCamp(CampModel campModel)
        {
            campModel.Links.Add(new LinkDto(
                _linkGenerator.GetUriByAction(HttpContext, "GetCamp", "camps", new { moniker = campModel.Moniker }),
                "self",
                "GET"
                ));

            campModel.Links.Add(new LinkDto(
                _linkGenerator.GetUriByAction(HttpContext, "Get", "talks", new { moniker = campModel.Moniker }),
                "get_talks_from_camp",
                "GET"
                ));

            campModel.Links.Add(new LinkDto(
                _linkGenerator.GetUriByAction(HttpContext, "Delete", "camps", new { moniker = campModel.Moniker }),
                "delete_camp",
                "DELETE"
                ));

            campModel.Links.Add(new LinkDto(
                _linkGenerator.GetUriByAction(HttpContext, "Put", "camps", new { moniker = campModel.Moniker }),
                "edit_camp",
                "PUT"
                ));

            campModel.Links.Add(new LinkDto(
                _linkGenerator.GetUriByAction(HttpContext, "Post", "camps"),
                "create_camp",
                "POST"
                ));

            return campModel;
        }

        [HttpOptions]
        public IActionResult GetCampsOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,PUT,DELETE,POST");
            return Ok();
        }
    }
}





