import type { ManifestActionGroupBase } from '@umbraco-cms/backoffice/action';

/**
 * A group of property actions. Property actions reference it through `group`.
 */
export interface ManifestPropertyActionGroup extends ManifestActionGroupBase {
	type: 'propertyActionGroup';
}

declare global {
	interface UmbExtensionManifestMap {
		umbPropertyActionGroup: ManifestPropertyActionGroup;
	}
}
