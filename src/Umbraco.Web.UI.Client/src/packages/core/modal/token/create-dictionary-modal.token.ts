import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCreateDictionaryModalData {
	parentId: string | null;
	parentName?: Observable<string | undefined>;
}

export interface UmbCreateDictionaryModalValue {
	name: string;
	parentId: string | null;
}

export const UMB_CREATE_DICTIONARY_MODAL = new UmbModalToken<
	UmbCreateDictionaryModalData,
	UmbCreateDictionaryModalValue
>('Umb.Modal.CreateDictionary', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
