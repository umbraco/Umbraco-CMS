import type { ManifestActionGroupBase } from '@umbraco-cms/backoffice/action';

/**
 * A group of entity actions. Entity actions reference it through `group`.
 */
export interface ManifestEntityActionGroup extends ManifestActionGroupBase {
	type: 'entityActionGroup';
}

declare global {
	interface UmbExtensionManifestMap {
		umbEntityActionGroup: ManifestEntityActionGroup;
	}
}
