using Microsoft.EntityFrameworkCore;
using YourAppName.Data.Entities.Identity;
using YourAppName.Infrastructure.Abstracts;
using YourAppName.Infrastructure.Data;
using YourAppName.Infrastructure.InfrastructureBases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Infrastructure.Repositories
{
    public class RefreshTokenRepository : GenericRepositoryAsync<UserRefreshToken>, IRefreshTokenRepository
    {
        #region Fields / Properties
        private readonly DbSet<UserRefreshToken> _userRefreshTokens;
        #endregion

        #region Constructor(s)
        public RefreshTokenRepository(AppDbContext context) : base(context)
        {
            _userRefreshTokens = context.Set<UserRefreshToken>();
        }
        #endregion

        #region Methods
        #endregion
    }
}
