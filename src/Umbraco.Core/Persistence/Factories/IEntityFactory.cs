namespace Umbraco.Core.Persistence.Factories
{
    //TODO: Not sure why we need this interface as it's never referenced in code.

    internal interface IEntityFactory<TEntity, TDto> 
        where TEntity : class
        where TDto : class
    {
        TEntity BuildEntity(TDto dto);
        TDto BuildDto(TEntity entity);
    }
}