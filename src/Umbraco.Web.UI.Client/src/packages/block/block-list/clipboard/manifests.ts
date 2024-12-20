import { manifests as copyToClipboardManifests } from './copy/manifests.js';
import { manifests as pasteFromClipboardManifests } from './paste/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...copyToClipboardManifests, ...pasteFromClipboardManifests];
