import { manifests as dataSource } from './data-source/manifests.js';
import { manifests as elementPicker } from './element-picker/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...dataSource, ...elementPicker];
