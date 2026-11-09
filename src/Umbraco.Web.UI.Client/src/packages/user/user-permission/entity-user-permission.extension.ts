import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestEntityUserPermission extends ManifestBase {
	type: 'entityUserPermission';
	forEntityTypes: Array<string>;
	meta: MetaEntityUserPermission;
}

export interface MetaEntityUserPermission {
	verbs: Array<string>;
	label?: string;
	description?: string;
	group?: string;

	/**
	 * When true, the permission is only offered when editing granular permissions and is left out of group-level default permissions.
	 */
	granularOnly?: boolean;
}

declare global {
	interface UmbExtensionManifestMap {
		umbEntityUserPermission: ManifestEntityUserPermission;
	}
}
