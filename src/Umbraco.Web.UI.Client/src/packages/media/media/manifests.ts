import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as dropzoneManifests } from './dropzone/manifests.js';
import { manifests as entityActionsManifests } from './entity-actions/manifests.js';
import { manifests as entityBulkActionsManifests } from './entity-bulk-actions/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as propertyEditorsManifests } from './property-editors/manifests.js';
import { manifests as recycleBinManifests } from './recycle-bin/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as searchManifests } from './search/manifests.js';
import { manifests as sectionViewManifests } from './section-view/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as fileUploadPreviewManifests } from './components/input-upload-field/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	...collectionManifests,
	...dropzoneManifests,
	...entityActionsManifests,
	...entityBulkActionsManifests,
	...menuManifests,
	...modalManifests,
	...propertyEditorsManifests,
	...recycleBinManifests,
	...repositoryManifests,
	...searchManifests,
	...sectionViewManifests,
	...treeManifests,
	...workspaceManifests,
	...fileUploadPreviewManifests,
];
