import type { UmbMemberGroupDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbMemberGroupDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Member Group Details
 */
export class UmbMemberGroupDetailStore extends UmbDetailStoreBase<UmbMemberGroupDetailModel> {
	/**
	 * Creates an instance of UmbMemberGroupDetailStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMemberGroupDetailStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT.toString());
	}

	withPropertyEditorUiAlias(propertyEditorUiAlias: string) {
		// TODO: Use a model for the member-group tree items: ^^Most likely it should be parsed to the UmbEntityTreeStore as a generic type.
		return this._data.asObservablePart((items) => items.filter((item) => item.editorUiAlias === propertyEditorUiAlias));
	}
}

export const UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbMemberGroupDetailStore>(
	'UmbMemberGroupDetailStore',
);
