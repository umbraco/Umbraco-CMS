import type { MemberDetails } from '../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @export
 * @class UmbMemberStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Members
 */
export class UmbMemberStore extends UmbStoreBase {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_STORE_CONTEXT_TOKEN.toString(), new UmbArrayState<MemberDetails>([], (x) => x.id));
	}

	append(member: MemberDetails) {
		this._data.append([member]);
	}

	remove(uniques: string[]) {
		this._data.remove(uniques);
	}
}

export const UMB_MEMBER_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberStore>('UmbMemberStore');
