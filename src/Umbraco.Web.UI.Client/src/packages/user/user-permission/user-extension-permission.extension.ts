import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';
import type { MetaEntityUserPermission } from './entity-user-permission.extension.js';

export interface ManifestExtensionPermissions extends ManifestElement {
	type: 'extensionPermissions';
	forEntityTypes: Array<string>;
	meta: MetaExtensionPermissions;
}

export interface MetaExtensionPermissions {
	extensionAlias: string;
	schemaType?: string;
	labelKey?: string;
	label?: string;
	descriptionKey?: string;
	description?: string;
}

export interface ManifestExtensionUserPermission extends ManifestElement {
	type: 'extensionUserPermission';
	forEntityTypes: Array<string>;
	forExtension: string;
	meta: MetaEntityUserPermission;
}

declare global {
	interface UmbExtensionManifestMap {
		umbExtensionPermissions: ManifestExtensionPermissions;
		umbExtensionUserPermission: ManifestExtensionUserPermission;
	}
}
