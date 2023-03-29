import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { ArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { MemberDetails } from '@umbraco-cms/backoffice/models';

/**
 * @export
 * @class UmbMemberStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Members
 */
export class UmbMemberStore extends UmbStoreBase {
	#data = new ArrayState<MemberDetails>([], (x) => x.key);

	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_STORE_CONTEXT_TOKEN.toString());
	}

	append(member: MemberDetails) {
		this.#data.append([member]);
	}

	remove(uniques: string[]) {
		this.#data.remove(uniques);
	}
}

export const UMB_MEMBER_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberStore>('UmbMemberStore');
