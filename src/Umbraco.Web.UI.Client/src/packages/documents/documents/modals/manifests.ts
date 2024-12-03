import { manifest as publishModalManifest } from './publish-modal/manifest.js';
import { manifest as publishWithDescendantsModalManifest } from './publish-with-descendants-modal/manifest.js';
import { manifest as saveModalManifest } from './save-modal/manifest.js';
import { manifest as scheduleModalManifest } from './schedule-modal/manifest.js';
import { manifest as unpublishModalManifest } from './unpublish-modal/manifest.js';

export const manifests: Array<UmbExtensionManifest> = [
	publishModalManifest,
	publishWithDescendantsModalManifest,
	saveModalManifest,
	scheduleModalManifest,
	unpublishModalManifest,
];
