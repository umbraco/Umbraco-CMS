import type { ManifestPackageView } from './package-view.model.js';
export type * from './package-view.model.js';

declare global {
	interface UmbExtensionManifestMap {
		ManifestPackageView: ManifestPackageView;
	}
}
