import type { UmbMenuItemExtensionElement } from '../interfaces/menu-item-extension-element.interface.js';
import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestMenuItem extends ManifestElement<UmbMenuItemExtensionElement> {
	type: 'menuItem';
	meta: MetaMenuItem;
}

export interface MetaMenuItem {
	label: string;
	icon: string;
	entityType?: string;
	menus: Array<string>;
}

export interface ManifestMenuItemTreeKind extends ManifestMenuItem {
	type: 'menuItem';
	kind: 'tree';
	meta: MetaMenuItemTreeKind;
}

export interface MetaMenuItemTreeKind extends MetaMenuItem {
	treeAlias: string;
}
