import { Observable } from 'rxjs';
import type { MemberGroupDetails } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState, createObservablePart } from '@umbraco-cms/observable-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbStoreBase } from '@umbraco-cms/store';
import { DictionaryItem, MemberGroupResource } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { umbMemberGroupData } from 'src/core/mocks/data/member-group.data';

export const UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberGroupDetailStore>('UmbMemberGroupDetailStore');

/**
 * @export
 * @class UmbMemberGroupDetailStore
 * @extends {UmbStoreBase}
 * @description - Detail Data Store for Member Groups
 */
export class UmbMemberGroupDetailStore extends UmbStoreBase {


	#groups = new ArrayState<MemberGroupDetails>([], x => x.key);
	public groups = this.#groups.asObservable();


	constructor(private host: UmbControllerHostInterface) {
		super(host, UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT_TOKEN.toString());
	}

	/**
	 * @description - Request a Member Group by key. The Member Group is added to the store and is returned as an Observable.
	 * @param {string} key
	 * @return {*}  {(Observable<MemberGroupDetails>)}
	 * @memberof UmbMemberGroupDetailStore
	 */
	getByKey(key: string): Observable<MemberGroupDetails> {
		// tryExecuteAndNotify(this.host, MemberGroupResource.getMemberGroupByKey({ key })).then(({ data }) => {
		// 	if (data) {
		// 		this.#groups.appendOne(data);
		// 	}
		// });

		// temp until Resource is updated
		const group = umbMemberGroupData.getByKey(key);
		if (group) {
			this.#groups.appendOne(group);
		}

		return createObservablePart(
			this.#groups,
			(groups) => groups.find((group) => group.key === key) as MemberGroupDetails
		);
	}

	async save(mediaTypes: Array<MemberGroupDetails>): Promise<void> {
		return null as any;
	}
}
