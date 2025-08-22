import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface ExampleModalData {
	value: string | null;
}

export type ExampleModalResult = undefined;

export const UMB_MISSING_PROPERTY_EDITOR_MODAL = new UmbModalToken<ExampleModalData, ExampleModalResult>(
	'Umb.Modal.MissingPropertyEditor',
	{
		modal: {
			type: 'dialog',
			size: 'small',
		},
	},
);
