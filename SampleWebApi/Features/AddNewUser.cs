﻿using MakeItSimple.WebApi.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleWebApi.Common;
using SampleWebApi.Data;
using SampleWebApi.Domain;

namespace SampleWebApi.Features
{
    [Route("api/User"), ApiController]
    public class AddNewUser : ControllerBase
    {
        private readonly IMediator _mediator;

        public AddNewUser(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("AddNewUser")]

        public async Task<IActionResult> Add([FromBody]AddNewUserCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);

                if (result.IsFailure)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        public class AddnewUserResult
        {
            public int Id { get; set; }
            public string Fullname { get; set; }
            public string Username { get; set; }

            public string Password { get; set; }
        }

        public class AddNewUserCommand : IRequest<Result>
        {
            public string Fullname { get; set; }
            public string Username { get; set; }

            public string Password { get; set; }
        }

        public class Handler : IRequestHandler<AddNewUserCommand, Result>
        {
            private readonly SampleWebApiDbContext _context;

            public Handler(SampleWebApiDbContext context)
            {
                _context = context;
            }

            public async Task<Result> Handle(AddNewUserCommand command, CancellationToken cancellationToken)
            {
                var existingClient = await _context.Users
                    .FirstOrDefaultAsync(user =>
                    user.Fullname == command.Fullname && 
                    user.Username == command.Username);

                if (existingClient != null)
                {
                    return Result.Failure(UserErrors.AlreadyExist(command.Fullname));
                }
                var user = new User
                {
                    Fullname = command.Fullname,
                    Username = command.Username,
                    Password = command.Password
                };

                await _context.Users.AddAsync(user, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                var result = new AddnewUserResult
                {
                    Id = user.Id,
                    Fullname = user.Fullname,
                    Username = user.Username,
                    Password = user.Password
                };

                return Result.Success();
            }
        }
    }
}
