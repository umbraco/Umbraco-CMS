import { manifests as conditionManifests } from './conditions/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...modalManifests, ...workspaceManifests, ...conditionManifests];
