import { manifest as duplicateToKindManifest } from './duplicate-to/duplicate-to.action.kind.js';
import { manifests as modalManifests } from './modal/manifests.js';

export const manifests = [duplicateToKindManifest, ...modalManifests];
