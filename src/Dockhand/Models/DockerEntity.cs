using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Dockhand.Exceptions;

namespace Dockhand.Models
{
    public abstract class DockerEntity
    {
        public string Id { get; protected set; }
        public bool Deleted { get; internal set; }

        [ExcludeFromCodeCoverage]
        protected virtual Task<bool> ExistsAction() => throw new NotImplementedException();
        [ExcludeFromCodeCoverage]
        protected virtual void ErrorAction(DockerCommandException e) => throw new NotImplementedException();
        [ExcludeFromCodeCoverage]
        protected virtual void DeletedAction() => throw new NotImplementedException();

        protected DockerEntity()
        {
            Deleted = false;
        }

        protected async Task<T> EnsureExistsBefore<T>(Func<Task<T>> commandFunc)
        {
            if (Deleted)
            {
                DeletedAction();
                return default(T);
            }

            try
            {
                return await commandFunc();
            }
            catch (DockerCommandException e)
            {
                Deleted = !(await ExistsAction());

                if (Deleted)
                {
                    ErrorAction(e);
                }

                throw;
            }
        }

        protected async Task EnsureExistsBefore(Func<Task> commandFunc)
        {
            if (Deleted)
            {
                DeletedAction();
                return;
            }

            try
            {
                await commandFunc();
            }
            catch (DockerCommandException e)
            {
                // Check if the image exists still
                Deleted = !(await ExistsAction());

                if (Deleted)
                {
                    ErrorAction(e);
                }

                throw;
            }
        }
    }
}
