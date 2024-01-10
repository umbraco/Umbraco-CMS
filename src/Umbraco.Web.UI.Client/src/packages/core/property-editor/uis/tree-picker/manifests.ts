import { manifest as filter } from './config/filter/manifests.js';
import { manifest as sourcePicker } from './config/source-picker/manifests.js';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.TreePicker',
	name: 'Tree Picker Property Editor UI',
	js: () => import('./property-editor-ui-tree-picker.element.js'),
	meta: {
		label: 'Tree Picker',
		icon: 'icon-page-add',
		group: 'pickers',
		propertyEditorSchemaAlias: 'Umbraco.MultiNodeTreePicker',
		settings: {
			properties: [
				{
					alias: 'startNode',
					label: 'Node type',
					description: '',
					propertyEditorUiAlias: sourcePicker.alias,
				},
				{
					alias: 'filter',
					label: 'Allow items of type',
					description: 'Select the applicable types',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.TreePicker.Filter',
				},
				{
					alias: 'showOpenButton',
					label: 'Show open button',
					description: 'Opens the node in a dialog',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
			],
		},
	},
};

const config: Array<ManifestPropertyEditorUi> = [sourcePicker, filter];

export const manifests = [manifest, ...config];
