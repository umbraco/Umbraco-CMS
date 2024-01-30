import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbEntityTreeItemModel } from '@umbraco-cms/backoffice/tree';

export interface UmbImportDictionaryModalData {
	unique: string | null;
}

export interface UmbImportDictionaryModalValue {
	entityItems: Array<UmbEntityTreeItemModel>;
	parentId?: string;
}

export const UMB_IMPORT_DICTIONARY_MODAL = new UmbModalToken<
	UmbImportDictionaryModalData,
	UmbImportDictionaryModalValue
>('Umb.Modal.Dictionary.Import', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
