using System;
using System.Linq;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public static class RepositoryConfigExtensions
    {
        //Our package repo
        private static readonly Guid RepoGuid = new Guid("65194810-1f85-11dd-bd0b-0800200c9a66");

        public static IRepository GetDefault(this IRepositoriesSection repos)
        {
            var found = repos.Repositories.FirstOrDefault(x => x.Id == RepoGuid);
            if (found == null)
                throw new InvalidOperationException("No default package repository found with id " + RepoGuid);
            return found;
        }
    }
}