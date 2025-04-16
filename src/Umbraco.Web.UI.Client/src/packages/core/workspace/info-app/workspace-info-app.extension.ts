import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface UmbWorkspaceInfoAppElement extends HTMLElement {
	manifest?: ManifestWorkspaceInfoApp;
}

export interface ManifestWorkspaceInfoApp<MetaType extends MetaWorkspaceInfoApp = MetaWorkspaceInfoApp>
	extends ManifestElement<UmbWorkspaceInfoAppElement>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'workspaceInfoApp';
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaWorkspaceInfoApp {}

declare global {
	interface UmbExtensionManifestMap {
		umbWorkspaceInfoApp: ManifestWorkspaceInfoApp;
	}
}
