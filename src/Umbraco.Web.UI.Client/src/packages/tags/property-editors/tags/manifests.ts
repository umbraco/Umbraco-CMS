import { manifest as storageType } from './config/storage-type/manifests';
import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.Tags',
	name: 'Tags Property Editor UI',
	loader: () => import('./property-editor-ui-tags.element'),
	meta: {
		label: 'Tags',
		propertyEditorModel: 'Umbraco.Tags',
		icon: 'umb:tags',
		group: 'common',
	},
};

const config: Array<ManifestPropertyEditorUI> = [storageType];

export const manifests = [manifest, ...config];
