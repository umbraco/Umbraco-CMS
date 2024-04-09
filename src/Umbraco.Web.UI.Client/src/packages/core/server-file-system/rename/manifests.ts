import { manifests as renameModalManifests } from './modal/manifests.js';
import { manifest as renameKindManifest } from './rename-server-file.action.kind.js';

export const manifests = [...renameModalManifests, renameKindManifest];
