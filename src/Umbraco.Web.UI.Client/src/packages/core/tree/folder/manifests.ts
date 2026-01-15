import { manifests as entityActionManifests } from './entity-action/manifests.js';
import { manifests as entityCreateOptionActionManifests } from './entity-create-option-action/manifests.js';
import { manifests as modalManifests } from './modal/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...entityActionManifests,
	...entityCreateOptionActionManifests,
	...modalManifests,
];
