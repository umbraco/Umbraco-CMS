import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbImportDictionaryModalData {
	unique: string | null;
}

export interface UmbImportDictionaryModalValue {
	entityItems: Array<EntityTreeItemResponseModel>;
	parentId?: string;
}

export const UMB_IMPORT_DICTIONARY_MODAL = new UmbModalToken<
	UmbImportDictionaryModalData,
	UmbImportDictionaryModalValue
>('Umb.Modal.ImportDictionary', {
	config: {
		type: 'sidebar',
		size: 'small',
	},
});
