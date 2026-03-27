import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentLinkPickerModalData {
	allowCultureSpecificLinks?: boolean;
}

export type UmbDocumentLinkPickerModalValue = {
	unique: string;
	entityType: string;
	culture?: string;
};

export const UMB_DOCUMENT_LINK_PICKER_MODAL = new UmbModalToken<
	UmbDocumentLinkPickerModalData,
	UmbDocumentLinkPickerModalValue
>('Umb.MultiUrlLinkPicker.Document', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
