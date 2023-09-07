import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPermission extends ManifestBase {
	type: 'permission';
	meta: MetaPermission;
}

export interface MetaPermission {
	label: string;
  description?: string;
  group?: string;
}
