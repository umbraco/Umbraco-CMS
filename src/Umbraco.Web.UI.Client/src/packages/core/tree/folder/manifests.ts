import { manifests as modalManifests } from './modal/manifests.js';
import { manifests as entityActionManifests } from './entity-action/manifests.js';
import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [...modalManifests, ...entityActionManifests];
