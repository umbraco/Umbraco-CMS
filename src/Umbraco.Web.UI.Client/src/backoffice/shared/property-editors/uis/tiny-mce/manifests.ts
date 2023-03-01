import { manifest as toolbar } from './config/configuration/manifests';
import type { ManifestPropertyEditorUI } from '@umbraco-cms/models';

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
					description: 'Select the width of the overlay (link picker)',
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

const config = [toolbar];

export const manifests = [manifest, ...config];
