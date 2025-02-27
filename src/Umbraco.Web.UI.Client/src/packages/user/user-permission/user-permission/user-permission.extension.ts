import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestUserPermission<MetaType extends MetaUserPermission = MetaUserPermission>
	extends ManifestElement {
	type: 'userPermission';
	meta: MetaType;
}

export interface MetaUserPermission {
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
		umbUserPermission: ManifestUserPermission;
	}
}
