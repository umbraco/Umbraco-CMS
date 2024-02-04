import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbImportDictionaryModalData {
	unique: string | null;
}

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
