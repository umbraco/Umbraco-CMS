import type { ManifestMenuItem } from '../models/index.js';

export interface UmbMenuItemExtensionElement extends HTMLElement {
	manifest?: ManifestMenuItem;
}
