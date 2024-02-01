import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as cultureManifests } from './culture/manifests.js';
import { manifests as dataTypeManifests } from './data-type/manifests.js';
import { manifests as debugManifests } from './debug/manifests.js';
import { manifests as entityActionManifests } from './entity-action/manifests.js';
import { manifests as extensionManifests } from './extension-registry/manifests.js';
import { manifests as localizationManifests } from './localization/manifests.js';
import { manifests as modalManifests } from './modal/common/manifests.js';
import { manifests as propertyActionManifests } from './property-action/manifests.js';
import { manifests as propertyEditorManifests } from './property-editor/manifests.js';
import { manifests as themeManifests } from './themes/manifests.js';
import { manifests as tinyMcePluginManifests } from './property-editor/uis/tiny-mce/plugins/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

import type { ManifestTypes, UmbBackofficeManifestKind } from './extension-registry/index.js';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	...collectionManifests,
	...cultureManifests,
	...dataTypeManifests,
	...debugManifests,
	...entityActionManifests,
	...extensionManifests,
	...localizationManifests,
	...modalManifests,
	...propertyActionManifests,
	...propertyEditorManifests,
	...themeManifests,
	...tinyMcePluginManifests,
	...treeManifests,
	...workspaceManifests,
];
