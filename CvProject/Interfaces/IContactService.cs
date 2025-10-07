using CvProject.DTOs;

namespace CvProject.Interfaces
{
    public interface IContactService
    {
        Task<bool> SaveAndSendAsync(ContactDto contactDto);
    }
}
