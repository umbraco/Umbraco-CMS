import type { ManifestSection } from '../../section/extensions/index.js';

export interface ManifestSectionDefaultKind extends ManifestSection {
	kind: 'default';
}

declare global {
	interface UmbExtensionManifestMap {
		umbDefaultSectionKind: ManifestSectionDefaultKind;
	}
}
