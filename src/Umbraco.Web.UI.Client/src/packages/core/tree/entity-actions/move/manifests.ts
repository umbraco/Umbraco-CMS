import { manifests as modalManifests } from './modal/manifests.js';
import { manifest as moveToKindManifest } from './move-to.action.kind.js';

export const manifests = [moveToKindManifest, ...modalManifests];
