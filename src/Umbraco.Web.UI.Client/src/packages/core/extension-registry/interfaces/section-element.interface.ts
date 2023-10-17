import type { ManifestSection } from '../models/index.js';

export interface UmbSectionElement extends HTMLElement {
	manifest?: ManifestSection;
}
