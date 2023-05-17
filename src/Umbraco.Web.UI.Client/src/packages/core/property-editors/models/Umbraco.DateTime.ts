import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

// TODO: We won't include momentjs anymore so we need to find a way to handle date formats
export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Date/Time',
	alias: 'Umbraco.DateTime',
	meta: {
		config: {
			properties: [
				{
					alias: 'offsetTime',
					label: 'Offset time',
					description:
						'When enabled the time displayed will be offset with the servers timezone, this is useful for scenarios like scheduled publishing when an editor is in a different timezone than the hosted server',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
			],
		},
	},
};
