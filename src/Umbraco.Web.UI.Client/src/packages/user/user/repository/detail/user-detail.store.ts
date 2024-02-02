import type { UmbUserDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbUserDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for User Details
 */
export class UmbUserDetailStore extends UmbDetailStoreBase<UmbUserDetailModel> {
	/**
	 * Creates an instance of UmbUserDetailStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserDetailStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_USER_DETAIL_STORE_CONTEXT.toString());
	}

	withPropertyEditorUiAlias(propertyEditorUiAlias: string) {
		// TODO: Use a model for the data-type tree items: ^^Most likely it should be parsed to the UmbEntityTreeStore as a generic type.
		return this._data.asObservablePart((items) => items.filter((item) => item.editorUiAlias === propertyEditorUiAlias));
	}
}

export const UMB_USER_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbUserDetailStore>('UmbUserDetailStore');
