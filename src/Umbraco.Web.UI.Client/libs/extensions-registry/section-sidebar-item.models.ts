import type { ManifestElement } from './models';

export interface ManifestSectionSidebarItem extends ManifestElement {
	type: 'sectionSidebarItem';
	meta: MetaSectionSidebarItem;
}

export interface MetaSectionSidebarItem {
	sections: Array<string>;
}
