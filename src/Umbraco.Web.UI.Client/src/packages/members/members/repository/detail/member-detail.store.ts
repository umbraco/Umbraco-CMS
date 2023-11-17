import type { UmbMemberDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @export
 * @class UmbMemberDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Member Detail
 */
export class UmbMemberDetailStore extends UmbStoreBase {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_DETAIL_STORE_CONTEXT.toString(), new UmbArrayState<UmbMemberDetailModel>([], (x) => x.id));
	}
}

export const UMB_MEMBER_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbMemberDetailStore>('UmbMemberDetailStore');
