import { manifests as publishManifests } from './publish/manifests.js';
import { manifests as unpublishManifests } from './unpublish/manifests.js';

export const manifests = [...publishManifests, ...unpublishManifests];
