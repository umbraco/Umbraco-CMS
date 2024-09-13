import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';
import { manifests as memberGroupPickerModalManifests } from './components/member-group-picker-modal/manifests.js';
import { manifests as menuItemManifests } from './menu-item/manifests.js';
import { manifests as propertyEditorManifests } from './property-editor/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as sectionViewManifests } from './section-view/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	...collectionManifests,
	...entityActionManifests,
	...memberGroupPickerModalManifests,
	...menuItemManifests,
	...propertyEditorManifests,
	...repositoryManifests,
	...sectionViewManifests,
	...workspaceManifests,
];
