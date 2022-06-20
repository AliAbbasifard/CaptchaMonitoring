using Entities;
using Microsoft.EntityFrameworkCore;
using Monitor.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Monitor.Repository
{
    public interface IRoomRepository
    {
        Task<string> CreateRoom(string connectionId);
        Task<string> GetRoom(string connectionId);
    }

    public class RoomRepository : IRoomRepository
    {
        private readonly ApplicationDbContext _context;
        public RoomRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Get Or Create a room for a given ConnectionId and return roomId
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public async Task<string> CreateRoom(string connectionId)
        {
            Room room = null;

            try
            {
                // check if there is room in database
                room = await _context
                    .Rooms
                    .AsNoTracking()
                    .SingleOrDefaultAsync(room => room.ConnectionId.Equals(connectionId));

                if (room is not null)
                    return room.RoomName;

                // create room
                var newRoom = new Room
                {
                    ConnectionId = connectionId,
                    RoomName = Guid.NewGuid().ToString()
                };
                await _context.Rooms.AddAsync(newRoom);
                await _context.SaveChangesAsync();

                return newRoom.RoomName;
            }
            catch (Exception)
            {
                // if there is more than one room for a given connectionId
                return null;
            }
        }

        /// <summary>
        /// Get room by ConnectionId
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public async Task<string> GetRoom(string connectionId)
        {
            Room room = null;
            try
            {
                // check if there is room in database
                room = await _context
                    .Rooms
                    .AsNoTracking()
                    .SingleOrDefaultAsync(room => room.ConnectionId.Equals(connectionId));

                if (room is not null)
                    return room.RoomName;

                return null;
            }
            catch (Exception)
            {
                // if there is more than one room for a given connectionId
                return null;
            }
        }
    }
}
