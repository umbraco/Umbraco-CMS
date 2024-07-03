import { manifest as blockGridAreaTypePermission } from './block-grid-area-type-permission/manifests.js';
import { manifest as blockGridAreasConfigEditor } from './block-grid-areas-config/manifests.js';
import { manifest as blockGridColumnSpan } from './block-grid-column-span/manifests.js';
import { manifests as blockGridEditorManifests } from './block-grid-editor/manifests.js';
import { manifest as blockGridGroupConfiguration } from './block-grid-group-configuration/manifests.js';
import { manifest as blockGridLayoutStylesheet } from './block-grid-layout-stylesheet/manifests.js';
import { manifest as blockGridTypeConfiguration } from './block-grid-type-configuration/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	blockGridAreaTypePermission,
	blockGridAreasConfigEditor,
	blockGridColumnSpan,
	...blockGridEditorManifests,
	blockGridGroupConfiguration,
	blockGridLayoutStylesheet,
	blockGridTypeConfiguration,
];
