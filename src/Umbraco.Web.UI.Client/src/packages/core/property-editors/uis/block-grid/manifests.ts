import { manifest as blockConfiguration } from './config/block-configuration/manifests.js';
import { manifest as groupConfiguration } from './config/group-configuration/manifests.js';
import { manifest as stylesheetPicker } from './config/stylesheet-picker/manifests.js';
import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.BlockGrid',
	name: 'Block Grid Property Editor UI',
	loader: () => import('./property-editor-ui-block-grid.element'),
	meta: {
		label: 'Block Grid',
		propertyEditorModel: 'Umbraco.BlockGrid',
		icon: 'umb:icon-layout',
		group: 'richContent',
		config: {
			properties: [
				{
					alias: 'useLiveEditing',
					label: 'Live editing mode',
					description: 'Live update content when editing in overlay',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
				{
					alias: 'maxPropertyWidth',
					label: 'Editor width',
					description: 'Optional css overwrite. (example: 1200px or 100%)',
					propertyEditorUI: 'Umb.PropertyEditorUI.TextBox',
				},
				{
					alias: 'createLabel',
					label: 'Create Button Label',
					description: 'Override the label text for adding a new block, Example Add Widget',
					propertyEditorUI: 'Umb.PropertyEditorUI.TextBox',
				},
			],
		},
	},
};

const config: Array<ManifestPropertyEditorUI> = [blockConfiguration, groupConfiguration, stylesheetPicker];

export const manifests = [manifest, ...config];
