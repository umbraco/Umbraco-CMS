import type { MemberGroupDetails } from '../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbMemberGroupStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Member Groups
 */
export class UmbMemberGroupStore extends UmbStoreBase {
	#data = new UmbArrayState<MemberGroupDetails>([], (x) => x.id);

	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_MEMBER_GROUP_STORE_CONTEXT_TOKEN.toString(),
			new UmbArrayState<MemberGroupDetails>([], (x) => x.id)
		);
	}

	append(memberGroup: MemberGroupDetails) {
		this._data.append([memberGroup]);
	}

	/**
	 * Retrieve a member from the store
	 * @param {string} id
	 * @memberof UmbMemberGroupStore
	 */
	byId(id: MemberGroupDetails['id']) {
		return this._data.asObservablePart((x) => x.find((y) => y.id === id));
	}

	remove(uniques: string[]) {
		this._data.remove(uniques);
	}
}

export const UMB_MEMBER_GROUP_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberGroupStore>('UmbMemberGroupStore');
