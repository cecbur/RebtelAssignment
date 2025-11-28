using DataStorage.Entities;
using BusinessModels;

namespace DataStorage.Converters;

internal static class AuthorConverter
{
    public static BusinessModels.Author ToModel(Entities.Author entity)
    {
        return new BusinessModels.Author
        {
            Id = entity.Id,
            GivenName = entity.GivenName,
            Surname = entity.Surname
        };
    }

    public static Entities.Author ToEntity(BusinessModels.Author model)
    {
        return new Entities.Author
        {
            Id = model.Id,
            GivenName = model.GivenName,
            Surname = model.Surname
        };
    }
}
