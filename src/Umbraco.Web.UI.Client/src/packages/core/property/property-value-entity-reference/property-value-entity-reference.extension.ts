import type { UmbPropertyValueEntityReferenceResolver } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * Registers a resolver for the entities a property editor's value directly references.
 */
export interface ManifestPropertyValueEntityReference extends ManifestApi<UmbPropertyValueEntityReferenceResolver> {
	type: 'propertyValueEntityReference';
	/** The alias of the property editor schema whose values this resolver handles. */
	forEditorAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestPropertyValueEntityReference: ManifestPropertyValueEntityReference;
	}
}
