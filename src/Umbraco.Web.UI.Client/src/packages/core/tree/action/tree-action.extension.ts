import type { UmbTreeAction } from './tree-action-base.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTreeAction
	extends
		ManifestElementAndApi<UmbControllerHostElement, UmbTreeAction>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'treeAction';
	meta: MetaTreeAction;
}

export interface MetaTreeAction {
	label: string;
	href?: string;
	additionalOptions?: boolean;
}

declare global {
	interface UmbExtensionManifestMap {
		umbTreeAction: ManifestTreeAction;
	}
}
