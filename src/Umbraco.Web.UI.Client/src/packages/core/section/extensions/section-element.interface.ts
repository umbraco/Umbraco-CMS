import type { ManifestSection } from './section.extension.js';

export interface UmbSectionElement extends HTMLElement {
	manifest?: ManifestSection;
}
