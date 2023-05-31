import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Checkbox List',
	alias: 'Umbraco.CheckboxList',
	meta: {
		defaultUI: 'Umb.PropertyEditorUI.CheckboxList',
	},
};
