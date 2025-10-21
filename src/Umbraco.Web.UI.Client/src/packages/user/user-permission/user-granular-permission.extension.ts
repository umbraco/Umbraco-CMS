import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestGranularUserPermission extends ManifestElement {
	type: 'userGranularPermission';
	forEntityTypes: Array<string>;
	meta: MetaGranularUserPermission;
}

export interface MetaGranularUserPermission {
	schemaType: string;
	label?: string;
	labelKey?: string;
	description?: string;
	descriptionKey?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbUserGranularPermission: ManifestGranularUserPermission;
	}
}
