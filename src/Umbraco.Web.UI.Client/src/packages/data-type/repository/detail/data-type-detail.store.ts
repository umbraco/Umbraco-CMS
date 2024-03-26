import type { UmbDataTypeDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbDataTypeDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Data Type Details
 */
export class UmbDataTypeDetailStore extends UmbDetailStoreBase<UmbDataTypeDetailModel> {
	/**
	 * Creates an instance of UmbDataTypeDetailStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDataTypeDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DATA_TYPE_DETAIL_STORE_CONTEXT.toString());
	}

	withPropertyEditorUiAlias(propertyEditorUiAlias: string) {
		return this._data.asObservablePart((items) => items.filter((item) => item.editorUiAlias === propertyEditorUiAlias));
	}
}

export const UMB_DATA_TYPE_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbDataTypeDetailStore>('UmbDataTypeDetailStore');
