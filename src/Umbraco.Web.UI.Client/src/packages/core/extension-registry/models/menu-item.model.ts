import type { UmbMenuItemElement } from '../interfaces/menu-item-element.interface.js';
import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestMenuItem extends ManifestElement<UmbMenuItemElement> {
	type: 'menuItem';
	meta: MetaMenuItem;
}

export interface MetaMenuItem {
	label: string;
	menus: Array<string>;
	entityType?: string;
	icon?: string;
}

export interface ManifestMenuItemTreeKind extends ManifestMenuItem {
	type: 'menuItem';
	kind: 'tree';
	meta: MetaMenuItemTreeKind;
}

export interface MetaMenuItemTreeKind extends MetaMenuItem {
	treeAlias: string;
	hideTreeRoot?: boolean;
}
