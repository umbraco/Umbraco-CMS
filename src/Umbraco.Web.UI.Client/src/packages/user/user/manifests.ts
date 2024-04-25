import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as conditionsManifests } from './conditions/manifests.js';
import { manifests as entityActionsManifests } from './entity-actions/manifests.js';
import { manifests as entityBulkActionManifests } from './entity-bulk-actions/manifests.js';
import { manifests as inviteManifests } from './invite/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as sectionViewManifests } from './section-view/manifests.js';
import { manifests as propertyEditorManifests } from './property-editor/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	...collectionManifests,
	...conditionsManifests,
	...entityActionsManifests,
	...entityBulkActionManifests,
	...inviteManifests,
	...modalManifests,
	...repositoryManifests,
	...sectionViewManifests,
	...propertyEditorManifests,
	...workspaceManifests,
];
