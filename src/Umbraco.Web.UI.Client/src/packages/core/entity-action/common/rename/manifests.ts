import { manifests as renameModalManifests } from './modal/manifests.js';
import { manifest as renameKindManifest } from './rename.action.kind.js';

export const manifests = [...renameModalManifests, renameKindManifest];
