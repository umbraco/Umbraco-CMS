import { manifests as tinyMcePluginManifests } from './tiny-mce-plugin/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...tinyMcePluginManifests, ...workspaceManifests];
