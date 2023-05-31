import { manifest as startNode } from './config/start-node/manifests.js';
import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUi.TreePicker',
	name: 'Tree Picker Property Editor UI',
	loader: () => import('./property-editor-ui-tree-picker.element.js'),
	meta: {
		label: 'Tree Picker',
		icon: 'umb:page-add',
		group: 'pickers',
		propertyEditorModel: 'Umbraco.MultiNodeTreePicker',
		settings: {
			properties: [
				{
					alias: 'startNode',
					label: 'Start node',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUi.TreePicker.StartNode',
				},
				{
					alias: 'filter',
					label: 'Allow items of type',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUi.TreePicker',
				},
				{
					alias: 'showOpenButton',
					label: 'Show open button',
					description: 'Opens the node in a dialog',
					propertyEditorUI: 'Umb.PropertyEditorUi.Toggle',
				},
			],
		},
	},
};

const config: Array<ManifestPropertyEditorUI> = [startNode];

export const manifests = [manifest, ...config];
