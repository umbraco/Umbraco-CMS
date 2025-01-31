import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface UmbWorkspaceInfoAppElement extends HTMLElement {
	manifest?: ManifestWorkspaceInfoApp;
}

export interface ManifestWorkspaceInfoApp
	extends ManifestElement<UmbWorkspaceInfoAppElement>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'workspaceInfoApp';
	meta: MetaWorkspaceInfoApp;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaWorkspaceInfoApp {}

declare global {
	interface UmbExtensionManifestMap {
		umbWorkspaceInfoApp: ManifestWorkspaceInfoApp;
	}
}
