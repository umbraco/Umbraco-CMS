import { manifest as createKindManifest } from './create.action.kind.js';
import { manifests as modalManifests } from './modal/manifests.js';

export const manifests = [createKindManifest, ...modalManifests];
