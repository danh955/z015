// <copyright file="UserEntity.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository
{
    using System.Collections.Generic;

    /// <summary>
    /// User entity class.
    /// </summary>
    //// TODO: Work out logging in.
    public class UserEntity
    {
        /// <summary>
        /// Gets or sets the user entity ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the login name.
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// Gets or sets the portfolios.
        /// </summary>
        public List<PortfolioEntity> Portfolios { get; set; }
    }
}