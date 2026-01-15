import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifests: Array<ManifestPropertyEditorUi> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.BlockRteTypeConfiguration',
		name: 'Block RTE Type Configuration Property Editor UI',
		element: () => import('./property-editor-ui-block-rte-type-configuration.element.js'),
		meta: {
			label: 'Block RTE Type Configuration',
			icon: 'icon-autofill',
			group: 'common',
		},
	},
];
