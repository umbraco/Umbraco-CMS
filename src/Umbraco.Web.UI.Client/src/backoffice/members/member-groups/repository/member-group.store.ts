import type { MemberGroupDetails } from '@umbraco-cms/backoffice/models';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { ArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbMemberGroupStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Member Groups
 */
export class UmbMemberGroupStore extends UmbStoreBase {
	#data = new ArrayState<MemberGroupDetails>([], (x) => x.key);

	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_GROUP_STORE_CONTEXT_TOKEN.toString());
	}

	append(memberGroup: MemberGroupDetails) {
		this.#data.append([memberGroup]);
	}

	remove(uniques: string[]) {
		this.#data.remove(uniques);
	}
}

export const UMB_MEMBER_GROUP_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberGroupStore>('UmbMemberGroupStore');
