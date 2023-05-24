import { manifest as startNode } from './config/start-node/manifests.js';
import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.TreePicker',
	name: 'Tree Picker Property Editor UI',
	loader: () => import('./property-editor-ui-tree-picker.element'),
	meta: {
		label: 'Tree Picker',
		icon: 'umb:page-add',
		group: 'pickers',
		propertyEditorModel: 'Umbraco.MultiNodeTreePicker',
		config: {
			properties: [
				{
					alias: 'startNode',
					label: 'Start node',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUI.TreePicker.StartNode',
				},
				{
					alias: 'filter',
					label: 'Allow items of type',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUI.TreePicker',
				},
				{
					alias: 'showOpenButton',
					label: 'Show open button',
					description: 'Opens the node in a dialog',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
			],
		},
	},
};

const config: Array<ManifestPropertyEditorUI> = [startNode];

export const manifests = [manifest, ...config];
