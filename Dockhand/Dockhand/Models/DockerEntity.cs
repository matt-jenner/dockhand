using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dockhand.Exceptions;

namespace Dockhand.Models
{
    public abstract class DockerEntity
    {
        public string Id { get; protected set; }
        public bool Deleted { get; private set; }

        protected virtual Task<bool> ExistsAction() => throw new NotImplementedException();
        protected virtual void ErrorAction(DockerCommandException e) => throw new NotImplementedException();
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
                //throw new DockerContainerDeletedException(Id);
            }

            try
            {
                return await commandFunc();
            }
            catch (DockerCommandException e)
            {
                // Check if the container exists still
                //Deleted = !(await _client.ContainerExistsAsync(Id));
                Deleted = !(await ExistsAction());

                if (Deleted)
                {
                    ErrorAction(e);
                    //throw new DockerContainerNotFoundException(Id, e);
                }

                // Otherwise throw the DockerCommandException
                throw;
            }
        }

        protected async Task EnsureExistsBefore(Func<Task> commandFunc)
        {
            if (Deleted)
            {
                //throw new DockerContainerDeletedException(Id);
                DeletedAction();
            }

            try
            {
                await commandFunc();
            }
            catch (DockerCommandException e)
            {
                // Check if the image exists still
                // Deleted = !(await _client.ContainerExistsAsync(Id));
                Deleted = !(await ExistsAction());

                if (Deleted)
                {
                    ErrorAction(e);
                    //throw new DockerContainerNotFoundException(Id, e);
                }

                // Otherwise throw the DockerCommandException
                throw;
            }
        }
    }
}
