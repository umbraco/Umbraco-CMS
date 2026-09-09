import type { UmbEntityReferenceListSource } from '../../global-components/entity-reference-list.element.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbEntityReferencesModalData {
	unique: string;
	referenceRepositoryAlias: string;
	itemRepositoryAlias?: string;
	headline?: string;
	/**
	 * When set, only the matching kind of reference is shown. When omitted, both are shown, as before.
	 */
	source?: UmbEntityReferenceListSource;
	/** When set, also shows a read-only section listing the entities this entity directly references that need attention before publishing. */
	entitiesNeedingAttention?: Array<UmbEntityModel>;
}

export type UmbEntityReferencesModalValue = undefined;
