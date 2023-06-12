import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCreateDictionaryModalData {
	parentId: string | null;
	parentName?: Observable<string | undefined>;
}

export interface UmbCreateDictionaryModalResult {
	name: string;
	parentId: string | null;
}

export const UMB_CREATE_DICTIONARY_MODAL = new UmbModalToken<
	UmbCreateDictionaryModalData,
	UmbCreateDictionaryModalResult
>('Umb.Modal.CreateDictionary', {
	type: 'sidebar',
	size: 'small',
});
