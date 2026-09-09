import type { UmbEntityReferenceListSource } from '../../global-components/entity-reference-list.element.js';

export interface UmbEntityReferencesModalData {
	unique: string;
	referenceRepositoryAlias: string;
	itemRepositoryAlias?: string;
	headline?: string;
	/**
	 * When set, only the matching kind of reference is shown. When omitted, both are shown, as before.
	 */
	source?: UmbEntityReferenceListSource;
}

export type UmbEntityReferencesModalValue = undefined;
