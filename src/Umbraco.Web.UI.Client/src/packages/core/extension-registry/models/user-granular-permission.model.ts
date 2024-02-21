import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestGranularUserPermission extends ManifestElement {
	type: 'userGranularPermission';
	meta: MetaGranularUserPermission;
}

export interface MetaGranularUserPermission {
	schemaType: string;
}
