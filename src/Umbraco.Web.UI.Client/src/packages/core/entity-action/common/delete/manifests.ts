import { manifest as deleteKindManifest } from './delete.action.kind.js';
import { manifests as modalManifest } from './modal/manifests.js';

export const manifests = [deleteKindManifest, ...modalManifest];
