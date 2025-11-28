using DataStorage.Entities;
using BusinessModels;

namespace DataStorage.Converters;

internal static class PatronConverter
{
    public static BusinessModels.Patron ToModel(Entities.Patron entity)
    {
        return new BusinessModels.Patron
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber,
            MembershipDate = entity.MembershipDate,
            IsActive = entity.IsActive
        };
    }

    public static Entities.Patron ToEntity(BusinessModels.Patron model)
    {
        return new Entities.Patron
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            MembershipDate = model.MembershipDate,
            IsActive = model.IsActive
        };
    }
}
