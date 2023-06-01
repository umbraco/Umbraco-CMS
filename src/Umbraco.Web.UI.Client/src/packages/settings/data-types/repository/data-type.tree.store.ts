import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityTreeStore } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbDataTypeTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Data-Types
 */
// TODO: consider if tree store could be turned into a general EntityTreeStore class?
export class UmbDataTypeTreeStore extends UmbEntityTreeStore {
	/**
	 * Creates an instance of UmbDataTypeTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDataTypeTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DATA_TYPE_TREE_STORE_CONTEXT_TOKEN.toString());
	}

	withPropertyEditorUiAlias(propertyEditorUiAlias: string) {
		// TODO: Use a model for the data-type tree items: ^^Most likely it should be parsed to the UmbEntityTreeStore as a generic type.
		return this._data.getObservablePart((item) => {
			if ((item as any).propertyEditorUiAlias === propertyEditorUiAlias) {
				return item;
			}
		});
	}
}

export const UMB_DATA_TYPE_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDataTypeTreeStore>('UmbDataTypeTreeStore');
