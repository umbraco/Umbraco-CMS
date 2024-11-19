import { manifests as clipboardRootManifests } from './clipboard-root/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as contextManifests } from './context/manifests.js';
import { manifests as detailManifests } from './detail/manifests.js';

export const manifests = [...contextManifests, ...clipboardRootManifests, ...collectionManifests, ...detailManifests];
