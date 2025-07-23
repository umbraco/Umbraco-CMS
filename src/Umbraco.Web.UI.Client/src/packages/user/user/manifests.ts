import { manifests as clientCredentialManifests } from './client-credential/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as conditionsManifests } from './conditions/manifests.js';
import { manifests as entityActionsManifests } from './entity-actions/manifests.js';
import { manifests as entityBulkActionManifests } from './entity-bulk-actions/manifests.js';
import { manifests as inviteManifests } from './invite/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as menuItemManifests } from './menu-item/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as propertyEditorManifests } from './property-editor/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...clientCredentialManifests,
	...collectionManifests,
	...conditionsManifests,
	...entityActionsManifests,
	...entityBulkActionManifests,
	...inviteManifests,
	...itemManifests,
	...menuItemManifests,
	...modalManifests,
	...propertyEditorManifests,
	...repositoryManifests,
	...workspaceManifests,
];
