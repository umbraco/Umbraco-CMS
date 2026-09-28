export type * from './confirm-action-modal-entity-references.element.js';
export type * from './confirm-bulk-action-modal-entity-references.element.js';
export type * from './entity-reference-list.element.js';
export type * from './entity-references-summary.element.js';

/**
 * Configuration for looking up the entities that reference a single entity, and the descendants of that entity
 * that are referenced elsewhere.
 */
export interface UmbEntityReferencesConfig {
	/**
	 * Alias of the item repository used to resolve descendant uniques into presentable items.
	 */
	itemRepositoryAlias: string;
	/**
	 * Alias of the reference repository used to look up references.
	 */
	referenceRepositoryAlias: string;
	/**
	 * The unique identifier of the entity to look up references for.
	 */
	unique: string;
}

/**
 * Configuration for looking up which of a set of entities are referenced elsewhere.
 */
export interface UmbEntityReferencesBulkConfig {
	/**
	 * The unique identifiers of the entities to check.
	 */
	uniques: Array<string>;
	/**
	 * Alias of the item repository used to resolve uniques into presentable items.
	 */
	itemRepositoryAlias: string;
	/**
	 * Alias of the reference repository used to look up references.
	 */
	referenceRepositoryAlias: string;
}
