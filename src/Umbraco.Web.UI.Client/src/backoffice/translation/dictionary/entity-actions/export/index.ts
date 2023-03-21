import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbExportDictionaryModalData {
	unique: string | null;
}

export interface UmbExportDictionaryModalResult {
	includeChildren?: boolean;
}

export const UMB_EXPORT_DICTIONARY_MODAL_TOKEN = new UmbModalToken<
	UmbExportDictionaryModalData,
	UmbExportDictionaryModalResult
>('Umb.Modal.ExportDictionary', {
	type: 'sidebar',
	size: 'small',
});
