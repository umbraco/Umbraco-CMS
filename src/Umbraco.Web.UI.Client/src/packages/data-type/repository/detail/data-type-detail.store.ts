import type { UmbDataTypeDetailModel } from '../../types.js';
import { UMB_DATA_TYPE_DETAIL_STORE_CONTEXT } from './data-type-detail.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbDataTypeDetailStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Data Type Details
 */
export class UmbDataTypeDetailStore extends UmbDetailStoreBase<UmbDataTypeDetailModel> {
	/**
	 * Creates an instance of UmbDataTypeDetailStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDataTypeDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DATA_TYPE_DETAIL_STORE_CONTEXT.toString());
	}

	withPropertyEditorUiAlias(propertyEditorUiAlias: string) {
		return this._data.asObservablePart((items) => items.filter((item) => item.editorUiAlias === propertyEditorUiAlias));
	}
}
export { UmbDataTypeDetailStore as api };
