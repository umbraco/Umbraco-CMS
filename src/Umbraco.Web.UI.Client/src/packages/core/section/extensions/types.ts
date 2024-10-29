export type * from './section-element.interface.js';
export type * from './section-sidebar-app-element.interface.js';
export type * from './section-view-element.interface.js';

import type { ManifestSectionRoute } from './section-route.extension.js';
import type { ManifestSectionSidebarApp } from './section-sidebar-app.extension.js';
import type { ManifestSectionView } from './section-view.extension.js';
import type { ManifestSection } from './section.extension.js';

type UmbSectionExtensions = ManifestSection | ManifestSectionRoute | ManifestSectionSidebarApp | ManifestSectionView;

declare global {
	interface UmbExtensionManifestMap {
		UmbSectionExtensions: UmbSectionExtensions;
	}
}
