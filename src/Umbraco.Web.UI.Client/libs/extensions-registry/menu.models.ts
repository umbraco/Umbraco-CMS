import type { ManifestElement } from './models';

export interface ManifestMenu extends ManifestElement {
	type: 'menu';
	meta: MetaMenu;
}

export interface MetaMenu {
	label: string;
}
