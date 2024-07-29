import { manifests as authManifests } from './auth/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as contentManifests } from './content/manifests.js';
import { manifests as contentTypeManifests } from './content-type/manifests.js';
import { manifests as cultureManifests } from './culture/manifests.js';
import { manifests as debugManifests } from './debug/manifests.js';
import { manifests as entityActionManifests } from './entity-action/manifests.js';
import { manifests as entityBulkActionManifests } from './entity-bulk-action/manifests.js';
import { manifests as extensionManifests } from './extension-registry/manifests.js';
import { manifests as iconRegistryManifests } from './icon-registry/manifests.js';
import { manifests as localizationManifests } from './localization/manifests.js';
import { manifests as modalManifests } from './modal/common/manifests.js';
import { manifests as propertyActionManifests } from './property-action/manifests.js';
import { manifests as propertyTypeManifests } from './property-type/manifests.js';
import { manifests as recycleBinManifests } from './recycle-bin/manifests.js';
import { manifests as sectionManifests } from './section/manifests.js';
import { manifests as serverFileSystemManifests } from './server-file-system/manifests.js';
import { manifests as settingsManifests } from './settings/manifests.js';
import { manifests as themeManifests } from './themes/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

import type { ManifestTypes, UmbBackofficeManifestKind } from './extension-registry/index.js';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	...authManifests,
	...extensionManifests,
	...iconRegistryManifests,
	...cultureManifests,
	...localizationManifests,
	...themeManifests,
	...sectionManifests,
	...treeManifests,
	...collectionManifests,
	...workspaceManifests,
	...contentManifests,
	...contentTypeManifests,
	...propertyTypeManifests,
	...settingsManifests,
	...modalManifests,
	...entityActionManifests,
	...entityBulkActionManifests,
	...propertyActionManifests,
	...serverFileSystemManifests,
	...debugManifests,
	...recycleBinManifests,
];
