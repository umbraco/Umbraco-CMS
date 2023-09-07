export interface UmbRepository<EntityType = unknown> {

	/**
	 * Get the type of the entity
	 *
	 * @public
	 * @type      {EntityType}
	 * @returns   undefined
	 */
	readonly ENTITY_TYPE: EntityType;

}
