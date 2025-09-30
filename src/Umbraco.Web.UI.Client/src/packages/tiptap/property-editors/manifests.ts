import { manifests as extensionsConfiguration } from './extensions-configuration/manifests.js';
import { manifests as statusbarConfiguration } from './statusbar-configuration/manifests.js';
import { manifests as tiptapRte } from './tiptap-rte/manifests.js';
import { manifests as toolbarConfiguration } from './toolbar-configuration/manifests.js';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifests: Array<ManifestPropertyEditorUi> = [
	...extensionsConfiguration,
	...statusbarConfiguration,
	...tiptapRte,
	...toolbarConfiguration,
];
