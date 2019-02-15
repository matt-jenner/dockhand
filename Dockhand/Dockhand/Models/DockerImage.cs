using System;
using System.Threading.Tasks;
using Dockhand.Client;
using Dockhand.Dtos;
using Dockhand.Exceptions;

namespace Dockhand.Models
{
    public class DockerImage
    {
        public string Id => _imageRecord.Id;
        public string Repository => _imageRecord.Repository;
        public string Tag => _imageRecord.Tag;

        public bool Deleted { get; private set; }


        private readonly DockerClient _client;
        private readonly DockerImageResult _imageRecord;
        
        internal DockerImage(DockerClient client, DockerImageResult imageResult)
        {
            _client = client;
            _imageRecord = imageResult;
            Deleted = false;
        }

        public async Task<DockerContainer> StartContainerAsync(DockerPortMapping[] portMappings) => await EnsureExistsBefore<DockerContainer>(() => _client.StartContainerAsync(_imageRecord.Id, portMappings));

        public async Task RemoveAsync() => await EnsureExistsBefore(() => _client.RemoveImageAsync(_imageRecord.Id));

        private async Task<T> EnsureExistsBefore<T>(Func<Task<T>> commandFunc)
        {
            if (Deleted)
            {
                throw new DockerImageDeletedException(_imageRecord.Id);
            }

            try
            {
                return await commandFunc();
            }
            catch (DockerCommandException e)
            {
                // Check if the image exists still
                Deleted = !(await _client.ImageExistsAsync(_imageRecord.Id));

                if (Deleted)
                {
                    throw new DockerImageNotFoundException(_imageRecord.Repository, _imageRecord.Tag, _imageRecord.Id, e);
                }

                // Otherwise throw the DockerCommandException
                throw;
            }
        }

        private async Task EnsureExistsBefore(Func<Task> commandFunc)
        {
            if (Deleted)
            {
                throw new DockerImageDeletedException(_imageRecord.Id);
            }

            try
            {
                await commandFunc();
            }
            catch (DockerCommandException e)
            {
                // Check if the image exists still
                Deleted = !(await _client.ImageExistsAsync(_imageRecord.Id));

                if (Deleted)
                {
                    throw new DockerImageNotFoundException(_imageRecord.Repository, _imageRecord.Tag, _imageRecord.Id, e);
                }

                // Otherwise throw the DockerCommandException
                throw;
            }
        }
    }
}
