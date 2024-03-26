import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestEntityUserPermission extends ManifestBase {
	type: 'entityUserPermission';
	forEntityTypes: Array<string>;
	meta: MetaEntityUserPermission;
}

export interface MetaEntityUserPermission {
	verbs: Array<string>;
	label?: string;
	labelKey?: string;
	description?: string;
	descriptionKey?: string;
	group?: string;
}
