import { manifest as startNode } from './config/start-node/manifests.js';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
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
					propertyEditorUi: 'Umb.PropertyEditorUi.TreePicker.StartNode',
				},
				{
					alias: 'filter',
					label: 'Allow items of type',
					description: '',
					propertyEditorUi: 'Umb.PropertyEditorUi.TreePicker',
				},
				{
					alias: 'showOpenButton',
					label: 'Show open button',
					description: 'Opens the node in a dialog',
					propertyEditorUi: 'Umb.PropertyEditorUi.Toggle',
				},
			],
		},
	},
};

const config: Array<ManifestPropertyEditorUi> = [startNode];

export const manifests = [manifest, ...config];
