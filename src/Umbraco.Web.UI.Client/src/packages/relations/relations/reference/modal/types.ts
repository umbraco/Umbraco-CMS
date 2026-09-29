import type { UmbEntityReferenceListSource } from '../../global-components/entity-reference-list.element.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

/**
 * The kind of reference the entity references modal can show: any `umb-entity-reference-list` source, or the
 * entities the entity directly references that need attention (supplied by the caller as `entitiesNeedingAttention`).
 */
export type UmbEntityReferencesModalSource = UmbEntityReferenceListSource | 'needingAttention';

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
	 * When set, only that kind of reference is shown; otherwise all are.
	 */
	source?: UmbEntityReferencesModalSource;
	/** When set, also shows a read-only section listing the entities this entity directly references that need attention before publishing. */
	entitiesNeedingAttention?: Array<UmbEntityModel>;
}

export type UmbEntityReferencesModalValue = undefined;
