import type { UmbMemberGroupDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @export
 * @class UmbMemberGroupDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for MemberGroup Detail
 */
export class UmbMemberGroupDetailStore extends UmbStoreBase {
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT.toString(),
			new UmbArrayState<UmbMemberGroupDetailModel>([], (x) => x.id),
		);
	}
}

export const UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbMemberGroupDetailStore>(
	'UmbMemberGroupDetailStore',
);
