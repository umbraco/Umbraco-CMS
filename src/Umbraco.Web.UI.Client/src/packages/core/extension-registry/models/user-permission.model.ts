import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestEntityUserPermission extends ManifestBase {
	type: 'entityUserPermission';
	meta: MetaEntityUserPermission;
}

export interface MetaEntityUserPermission {
	entityType: string;
	label?: string;
	labelKey?: string;
	description?: string;
	descriptionKey?: string;
	group?: string;
}
