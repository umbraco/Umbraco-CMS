import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestUserGranularPermission extends ManifestElement {
	type: 'userGranularPermission';
	meta: MetaUserGranularPermission;
}

export interface MetaUserGranularPermission {
	entityType: string;
}
