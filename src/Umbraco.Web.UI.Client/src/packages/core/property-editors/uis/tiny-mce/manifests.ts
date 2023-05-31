import { manifest as configuration } from './config/configuration/manifests.js';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.TinyMCE',
	name: 'Rich Text Editor Property Editor UI',
	loader: () => import('./property-editor-ui-tiny-mce.element.js'),
	meta: {
		label: 'Rich Text Editor',
		propertyEditorModel: 'Umbraco.TinyMCE',
		icon: 'umb:browser-window',
		group: 'richText',
		settings: {
			properties: [
				{
					alias: 'editor',
					label: 'Editor',
					propertyEditorUi: 'Umb.PropertyEditorUi.TinyMCE.Configuration',
				},
				{
					alias: 'overlaySize',
					label: 'Overlay Size',
					propertyEditorUi: 'Umb.PropertyEditorUi.OverlaySize',
				},
				{
					alias: 'hideLabel',
					label: 'Hide Label',
					propertyEditorUi: 'Umb.PropertyEditorUi.Toggle',
				},
			],
		},
	},
};

const config = [configuration];

export const manifests = [manifest, ...config];
