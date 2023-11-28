import type { ManifestMenuItem } from '../models/index.js';

export interface UmbMenuItemElement extends HTMLElement {
	manifest?: ManifestMenuItem;
}
