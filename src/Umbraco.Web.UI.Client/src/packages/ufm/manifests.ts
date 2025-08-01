import { manifest as ufmContext } from './contexts/manifest.js';
import { manifests as markedExtensions } from './extensions/manifests.js';
import { manifests as ufmComponents } from './components/manifests.js';
import { manifests as ufmFilters } from './filters/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	ufmContext,
	...markedExtensions,
	...ufmComponents,
	...ufmFilters,
];
