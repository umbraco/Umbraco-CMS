import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { ArrayState } from '@umbraco-cms/observable-api';
import type { MemberTypeDetails } from '@umbraco-cms/models';

/**
 * @export
 * @class UmbMemberTypeDetailStore
 * @extends {UmbStoreBase}
 * @description - Details Data Store for Member Types
 */
export class UmbMemberTypeDetailStore
	extends UmbStoreBase
{
	#data = new ArrayState<MemberTypeDetails>([], (x) => x.key);

	constructor(host: UmbControllerHostInterface) {
		super(host, UmbMemberTypeDetailStore.name);
	}

	append(MemberType: MemberTypeDetails) {
		this.#data.append([MemberType]);
	}

	remove(uniques: string[]) {
		this.#data.remove(uniques);
	}	
}

export const UMB_MEMBER_TYPE_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberTypeDetailStore>(
	UmbMemberTypeDetailStore.name
);