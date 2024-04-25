import { manifest as blockListTypeConfiguration } from './block-list-type-configuration/manifests.js';
import { manifests as blockGridEditorManifests } from './block-list-editor/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [blockListTypeConfiguration, ...blockGridEditorManifests];
