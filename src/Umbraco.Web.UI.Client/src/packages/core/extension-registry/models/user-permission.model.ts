import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestUserPermission extends ManifestBase {
	type: 'userPermission';
	meta: MetaUserPermission;
}

export interface MetaUserPermission {
	label: string;
  description?: string;
  group?: string;
}
