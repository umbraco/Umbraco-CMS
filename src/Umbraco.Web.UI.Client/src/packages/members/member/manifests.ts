import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as memberPickerModalManifests } from './components/member-picker-modal/manifests.js';
import { manifests as menuItemManifests } from './menu-item/manifests.js';
import { manifests as pickerManifests } from './picker/manifests.js';
import { manifests as propertyEditorManifests } from './property-editor/manifests.js';
import { manifests as referenceManifests } from './reference/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as searchManifests } from './search/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...collectionManifests,
	...entityActionManifests,
	...itemManifests,
	...memberPickerModalManifests,
	...menuItemManifests,
	...pickerManifests,
	...propertyEditorManifests,
	...referenceManifests,
	...repositoryManifests,
	...searchManifests,
	...workspaceManifests,
];
