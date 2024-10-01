import type { UmbMenuItemElement } from './menu-item-element.interface.js';
import type { ManifestWithDynamicConditions, ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestMenuItem
	extends ManifestElement<UmbMenuItemElement>,
		ManifestWithDynamicConditions<UmbExtensionCondition> {
	type: 'menuItem';
	meta: MetaMenuItem;
}

export interface MetaMenuItem {
	label: string;
	menus: Array<string>;
	entityType?: string;
	icon?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbMenuItem: ManifestMenuItem;
	}
}
