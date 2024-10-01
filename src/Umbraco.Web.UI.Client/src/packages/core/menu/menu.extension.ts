import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestMenu extends ManifestElement {
	type: 'menu';
}

declare global {
	interface UmbExtensionManifestMap {
		UmbMenuExtension: ManifestMenu;
	}
}
