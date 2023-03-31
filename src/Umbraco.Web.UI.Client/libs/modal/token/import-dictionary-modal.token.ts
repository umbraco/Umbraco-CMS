import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// TODO: add interface for data
// PropertyTypeViewModelBaseModel
export interface UmbImportDictionaryModalData {
	unique: string | null;
}

export interface UmbImportDictionaryModalResult {
	fileName?: string;
	parentKey?: string;
}

export const UMB_IMPORT_DICTIONARY_MODAL = new UmbModalToken<
	UmbImportDictionaryModalData,
	UmbImportDictionaryModalResult
>('Umb.Modal.ImportDictionary', {
	type: 'sidebar',
	size: 'small',
});
