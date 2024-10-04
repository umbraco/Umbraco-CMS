import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbImportDictionaryModalData {
	unique: string | null;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbImportDictionaryModalValue {}

export const UMB_IMPORT_DICTIONARY_MODAL = new UmbModalToken<
	UmbImportDictionaryModalData,
	UmbImportDictionaryModalValue
>('Umb.Modal.Dictionary.Import', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
