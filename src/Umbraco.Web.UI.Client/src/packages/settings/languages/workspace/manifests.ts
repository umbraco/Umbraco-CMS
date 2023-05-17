import { manifests as languageManifests } from './language/manifests';
import { manifests as languageRootManifests } from './language-root/manifests';

export const manifests = [...languageManifests, ...languageRootManifests];
