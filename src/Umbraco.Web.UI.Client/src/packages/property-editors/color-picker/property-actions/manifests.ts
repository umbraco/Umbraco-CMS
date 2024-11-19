import { manifests as copyToClipboardManifests } from './copy-to-clipboard/manifests.js';
import { manifests as pasteFromClipboardManifests } from './paste-from-clipboard/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...copyToClipboardManifests, ...pasteFromClipboardManifests];
