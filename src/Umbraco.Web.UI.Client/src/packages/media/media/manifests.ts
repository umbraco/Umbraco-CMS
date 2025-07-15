import { manifests as auditLogManifests } from './audit-log/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as entityActionsManifests } from './entity-actions/manifests.js';
import { manifests as entityBulkActionsManifests } from './entity-bulk-actions/manifests.js';
import { manifests as fileUploadPreviewManifests } from './components/input-upload-field/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as propertyEditorsManifests } from './property-editors/manifests.js';
import { manifests as recycleBinManifests } from './recycle-bin/manifests.js';
import { manifests as referenceManifests } from './reference/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as searchManifests } from './search/manifests.js';
import { manifests as sectionViewManifests } from './dashboard/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as urlManifests } from './url/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...auditLogManifests,
	...collectionManifests,
	...entityActionsManifests,
	...entityBulkActionsManifests,
	...fileUploadPreviewManifests,
	...itemManifests,
	...menuManifests,
	...modalManifests,
	...propertyEditorsManifests,
	...recycleBinManifests,
	...referenceManifests,
	...repositoryManifests,
	...searchManifests,
	...sectionViewManifests,
	...treeManifests,
	...urlManifests,
	...workspaceManifests,
];
