import type { UmbCollectionAction } from '../action/collection-action-base.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

/**
 * An action to perform on an entity
 * For example for content you may wish to create a new document etc
 */
// TODO: create interface for API
export interface ManifestCollectionAction
	extends ManifestElementAndApi<UmbControllerHostElement, UmbCollectionAction>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'collectionAction';
	meta: MetaCollectionAction;
}

export interface MetaCollectionAction {
	label: string;
	href?: string;
	additionalOptions?: boolean;
}

declare global {
	interface UmbExtensionManifestMap {
		umbCollectionAction: ManifestCollectionAction;
	}
}
