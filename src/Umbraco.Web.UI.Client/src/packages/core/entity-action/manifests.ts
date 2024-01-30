import { manifests as renameEntityActionManifests } from './common/rename/manifests.js';
import { manifest as deleteEntityActionManifest } from './common/delete/delete.action.kind.js';

export const manifests = [...renameEntityActionManifests, deleteEntityActionManifest];
