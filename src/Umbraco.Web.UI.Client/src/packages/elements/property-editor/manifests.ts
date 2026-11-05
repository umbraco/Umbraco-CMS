import { manifests as elementPicker } from './element-picker/manifests.js';
import { manifests as elementstartNodeAccess } from './element-start-node-access/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...elementPicker, ...elementstartNodeAccess];
