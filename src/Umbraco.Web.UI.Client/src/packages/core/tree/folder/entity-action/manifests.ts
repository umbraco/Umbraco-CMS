import { manifest as createKindManifest } from './create-folder/create-folder.action.kind.js';
import { manifest as deleteKindManifest } from './delete-folder/delete-folder.action.kind.js';
import { manifest as updateKindManifest } from './update-folder/update-folder.action.kind.js';

export const manifests: Array<ManifestTypes> = [createKindManifest, deleteKindManifest, updateKindManifest];
