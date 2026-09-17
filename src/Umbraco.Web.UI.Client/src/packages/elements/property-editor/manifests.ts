import { manifests as elementPicker } from './element-picker/manifests.js';
import { manifests as elementStartNodeAccess } from './element-start-node-access/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...elementPicker, ...elementStartNodeAccess];
