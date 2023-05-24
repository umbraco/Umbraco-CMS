import type { ManifestSection } from '../models/index.js';

export interface UmbSectionExtensionElement extends HTMLElement {
	manifest?: ManifestSection;
}
