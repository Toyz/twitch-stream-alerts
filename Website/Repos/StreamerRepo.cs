using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Models;

namespace Website.Repos
{
    public interface IStreamerRepo
    {
        Task<Streamer> GetById(string id);

        Task<int> Update(Streamer streamer);

        Task<int> Insert(Streamer streamer);
    }

    public class StreamerRepo : IStreamerRepo
    {
        private readonly DBContext _dbContext;

        public StreamerRepo(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Streamer> GetById(string id)
        {
            return await _dbContext.Streamers.FirstOrDefaultAsync(x => x.StreamId == id);
        }

        public async Task<int> Update(Streamer streamer)
        {
            _dbContext.Update(streamer);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> Insert(Streamer streamer)
        {
            _dbContext.Add(streamer);
            return await _dbContext.SaveChangesAsync();
        }
    }
}
