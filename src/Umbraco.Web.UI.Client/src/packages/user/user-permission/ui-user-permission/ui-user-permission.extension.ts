import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestUiUserPermission<MetaType extends MetaUiUserPermission = MetaUiUserPermission>
	extends ManifestElement {
	type: 'uiUserPermission';
	meta: MetaType;
}

export interface MetaUiUserPermission {
	label: string;
	description?: string;
	group: string;
	permission: {
		context: string;
		verbs: Array<string>;
	};
}

declare global {
	interface UmbExtensionManifestMap {
		umbUiUserPermission: ManifestUiUserPermission;
	}
}
