import type { ManifestModal } from '@umbraco-cms/backoffice/extensions-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		kind: 'treePicker',
		alias: 'Umb.Modal.DataTypePicker',
		name: 'Data Type Picker Modal',
		meta: {
			treeAlias: 'Umb.Tree.DataTypes',
		},
	},
];

export const manifests = [...modals];
