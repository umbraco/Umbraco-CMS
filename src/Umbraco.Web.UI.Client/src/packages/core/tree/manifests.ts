import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as defaultTreeItemManifests } from './tree-item/tree-item-default/manifests.js';
import { manifests as defaultTreeManifests } from './default/manifests.js';
import { manifests as treePickerManifests } from './tree-picker/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	...defaultTreeItemManifests,
	...defaultTreeManifests,
	...entityActionManifests,
	...folderManifests,
	...treePickerManifests,
];
