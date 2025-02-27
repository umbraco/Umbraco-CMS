import type { ManifestMenuItem, MetaMenuItem } from '../../../menu-item.extension.js';

export interface ManifestMenuItemLinkKind extends ManifestMenuItem {
	type: 'menuItem';
	kind: 'link';
	meta: MetaMenuItemLinkKind;
}

export interface MetaMenuItemLinkKind extends MetaMenuItem {
	href: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbLinkMenuItemKind: ManifestMenuItemLinkKind;
	}
}
