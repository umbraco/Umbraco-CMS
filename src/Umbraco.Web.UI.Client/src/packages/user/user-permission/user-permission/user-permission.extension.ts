import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestUserPermission<MetaType extends MetaUserPermission = MetaUserPermission> extends ManifestBase {
	type: 'userPermission';
	meta: MetaType;
}

export interface MetaUserPermission {
	label: string;
	description?: string;
	permission: {
		context: string;
		key?: string;
		permission: string;
	};
}

declare global {
	interface UmbExtensionManifestMap {
		umbUserPermission: ManifestUserPermission;
	}
}
