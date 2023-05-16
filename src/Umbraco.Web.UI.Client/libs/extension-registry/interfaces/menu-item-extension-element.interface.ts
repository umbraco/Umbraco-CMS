import type { ManifestMenuItem } from '../models';

export interface UmbMenuItemExtensionElement extends HTMLElement {
	manifest?: ManifestMenuItem;
}
