export type * from './confirm-action-modal-entity-references.element.js';
export type * from './confirm-bulk-action-modal-entity-references.element.js';
export type * from './entity-reference-list.element.js';
export type * from './entity-references-summary.element.js';

/**
 * Configuration for looking up the entities that reference a single entity, and the descendants of that entity
 * that are referenced elsewhere.
 */
export interface UmbEntityReferencesConfig {
	itemRepositoryAlias: string;
	referenceRepositoryAlias: string;
	unique: string;
}

/**
 * Configuration for looking up which of a set of entities are referenced elsewhere.
 */
export interface UmbEntityReferencesBulkConfig {
	uniques: Array<string>;
	itemRepositoryAlias: string;
	referenceRepositoryAlias: string;
}
