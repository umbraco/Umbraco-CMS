import { map, Observable } from 'rxjs';
import { UmbDataStoreBase } from '../../../core/stores/store';
import { DictionaryResource, EntityTreeItem } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbContextToken } from '@umbraco-cms/context-api';

export const STORE_ALIAS = 'UmbDictionaryStore';

/**
 * @export
 * @class UmbDictionaryStore
 * @extends {UmbDataStoreBase<PagedEntityTreeItem>}
 * @description - Data Store for Dictionary Items.
 */
export class UmbDictionaryStore extends UmbDataStoreBase<EntityTreeItem> {
	public readonly storeAlias = STORE_ALIAS;

	/**
	 * @description - Get the root of the tree.
	 * @return {*}  {Observable<Array<PagedEntityTreeItem>>}
	 * @memberof UmbDictionaryStore
	 */
	getTreeRoot(): Observable<Array<EntityTreeItem>> {
		tryExecuteAndNotify(this.host, DictionaryResource.getTreeDictionaryRoot({})).then(({ data }) => {
			if (data) {
				this.updateItems(data.items);
			}
		});

		return this.items.pipe(map((items) => items.filter((item) => item.parentKey === null)));
	}

	/**
	 * @description - Get the children of a tree item.
	 * @param {string} key
	 * @return {*}  {Observable<Array<EntityTreeItem>>}
	 * @memberof UmbDataTypesStore
	 */
	getTreeItemChildren(key: string): Observable<Array<EntityTreeItem>> {
		tryExecuteAndNotify(
			this.host,
			DictionaryResource.getTreeDictionaryChildren({
				parentKey: key,
			})
		).then(({ data }) => {
			if (data) {
				this.updateItems(data.items);
			}
		});

		return this.items.pipe(map((items) => items.filter((item) => item.parentKey === key)));
	}
}

export const UMB_DICTIONARY_STORE_CONTEXT_ALIAS = new UmbContextToken<UmbDictionaryStore>(STORE_ALIAS);
