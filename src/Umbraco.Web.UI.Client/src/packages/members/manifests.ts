import { manifests as memberGroupManifests } from './member-group/manifests.js';
import { manifests as memberManifests } from './member/manifests.js';
import { manifests as memberPublicAccessManifests } from './member-public-access/manifests.js';
import { manifests as memberTypeManifests } from './member-type/manifests.js';
import { manifests as sectionManifests } from './section/manifests.js';

import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

import './member/components/index.js';
import './member-group/components/index.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...memberGroupManifests,
	...memberManifests,
	...memberPublicAccessManifests,
	...memberTypeManifests,
	...sectionManifests,
];
