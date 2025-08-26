import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbMissingPropertyModalData {
	value: string | null;
}

export type UmbMissingPropertyModalResult = undefined;

export const UMB_MISSING_PROPERTY_EDITOR_MODAL = new UmbModalToken<
	UmbMissingPropertyModalData,
	UmbMissingPropertyModalResult
>('Umb.Modal.MissingPropertyEditor', {
	modal: {
		type: 'dialog',
		size: 'small',
	},
});
