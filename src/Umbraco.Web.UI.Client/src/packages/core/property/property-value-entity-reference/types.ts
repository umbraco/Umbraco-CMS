import type { UmbPropertyValueDataPotentiallyWithEditorAlias } from '../types.js';
import type { ManifestPropertyValueEntityReference } from './property-value-entity-reference.extension.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export type * from './property-value-entity-reference.extension.js';

/**
 * Resolves the entities a property editor's value directly references, e.g. the elements embedded in a Block
 * List/Grid value, or the elements picked by an Element Picker.
 */
export interface UmbPropertyValueEntityReferenceResolver extends UmbApi {
	/** Assigned by the controller after construction — mirrors `UmbPropertyValueResolver.manifest`. */
	manifest?: ManifestPropertyValueEntityReference;
	resolveEntityReferences(
		value: UmbPropertyValueDataPotentiallyWithEditorAlias,
	): Promise<Array<UmbEntityModel>>;
}
