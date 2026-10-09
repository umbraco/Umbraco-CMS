import type { UmbEntityReferenceListSource } from '../../global-components/entity-reference-list.element.js';

/**
 * Data for the entity references modal, a paged overview of the entities referencing (or referenced by) a single entity.
 */
export interface UmbEntityReferencesModalData {
	/**
	 * The unique identifier of the entity to look up references for.
	 */
	unique: string;
	/**
	 * Alias of the reference repository used to look up references.
	 */
	referenceRepositoryAlias: string;
	/**
	 * Alias of the item repository used to resolve descendant uniques into presentable items. Only needed for
	 * the `descendantsWithReferences` source.
	 */
	itemRepositoryAlias?: string;
	/**
	 * Overrides the modal's default headline.
	 */
	headline?: string;
	/**
	 * When set, only that kind of reference is shown; otherwise both are.
	 */
	source?: UmbEntityReferenceListSource;
}

export type UmbEntityReferencesModalValue = undefined;
