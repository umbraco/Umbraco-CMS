import type { UmbMenuItemExtensionElement } from '../interfaces/menu-item-extension-element.interface.js';
import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestMenuItem extends ManifestElement<UmbMenuItemExtensionElement> {
	type: 'menuItem';
	meta: MetaMenuItem;
	conditions: ConditionsMenuItem;
}

export interface MetaMenuItem {
	label: string;
	icon: string;
	entityType?: string;
}

export interface ConditionsMenuItem {
	menus: Array<string>;
}

export interface ManifestMenuItemTreeKind extends ManifestMenuItem {
	type: 'menuItem';
	kind: 'tree';
	meta: MetaMenuItemTreeKind;
}

export interface MetaMenuItemTreeKind {
	treeAlias: string;
	label: string;
	icon: string;
	entityType?: string;
}
