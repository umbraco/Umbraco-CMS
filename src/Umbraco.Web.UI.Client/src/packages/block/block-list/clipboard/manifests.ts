import { manifests as blockCopyManifests } from './block/copy/manifests.js';
import { manifests as blockPasteManifests } from './block/paste/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...blockCopyManifests, ...blockPasteManifests];
