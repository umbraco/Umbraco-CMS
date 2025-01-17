import type { ManifestMenuItem } from './menu-item.extension.js';

export interface UmbMenuItemElement extends HTMLElement {
	manifest?: ManifestMenuItem;
}
