import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as defaultTreeItemManifests } from './tree-item/tree-item-default/manifests.js';
import { manifests as defaultTreeManifests } from './default/manifests.js';
import { manifests as treePickerManifests } from './tree-picker-modal/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';
import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [
	...defaultTreeItemManifests,
	...defaultTreeManifests,
	...entityActionManifests,
	...folderManifests,
	...treePickerManifests,
];
