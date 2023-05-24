import { manifest as configuration } from './config/configuration/manifests.js';
import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.TinyMCE',
	name: 'Rich Text Editor Property Editor UI',
	loader: () => import('./property-editor-ui-tiny-mce.element'),
	meta: {
		label: 'Rich Text Editor',
		propertyEditorModel: 'Umbraco.TinyMCE',
		icon: 'umb:browser-window',
		group: 'richText',
		config: {
			properties: [
				{
					alias: 'editor',
					label: 'Editor',
					propertyEditorUI: 'Umb.PropertyEditorUI.TinyMCE.Configuration',
				},
				{
					alias: 'overlaySize',
					label: 'Overlay Size',
					propertyEditorUI: 'Umb.PropertyEditorUI.OverlaySize',
				},
				{
					alias: 'hideLabel',
					label: 'Hide Label',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
			],
		},
	},
};

const config = [configuration];

export const manifests = [manifest, ...config];
