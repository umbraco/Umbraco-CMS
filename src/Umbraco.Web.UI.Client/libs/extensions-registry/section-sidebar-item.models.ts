import type { ManifestElement } from './models';

export interface ManifestSectionSidebarApp extends ManifestElement {
	type: 'sectionSidebarApp';
	meta: MetaSectionSidebarApp;
}

export interface MetaSectionSidebarApp {
	sections: Array<string>;
}
