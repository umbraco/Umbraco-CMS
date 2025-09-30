import { manifests as languageManifests } from './language/manifests.js';
import { manifests as languageRootManifests } from './language-root/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...languageManifests, ...languageRootManifests];
