import type { UmbDrawerAppElement } from './drawer-app-element.interface.js';
import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestDrawerApp
	extends ManifestElement<UmbDrawerAppElement>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'drawerApp';
}

declare global {
	interface UmbExtensionManifestMap {
		umbDrawerApp: ManifestDrawerApp;
	}
}
