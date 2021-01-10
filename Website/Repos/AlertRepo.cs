using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Models;

namespace Website.Repos
{
    public interface IAlertRepo
    {
        Task<Alert> GetById(int id, string owner);

        Task<Alert> GetByStreamer(string streamer, string owner);

        Task<int> Update(Alert alert);

        Task<int> Insert(Alert alert);
    }

    public class AlertRepo : IAlertRepo
    {
        private readonly DBContext _dbContext;

        public AlertRepo(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Alert> GetById(int id, string owner)
        {
            return await _dbContext.Alerts.FirstOrDefaultAsync(x => x.Id == id && x.Owner == owner);
        }

        public async Task<Alert> GetByStreamer(string streamer, string owner)
        {
            return await _dbContext.Alerts.FirstOrDefaultAsync(x => x.Streamer == streamer && x.Owner == owner);
        }

        public async Task<int> Insert(Alert alert)
        {
            _dbContext.Add(alert);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Alert alert)
        {
            _dbContext.Add(alert);
            return await _dbContext.SaveChangesAsync();
        }
    }
}
