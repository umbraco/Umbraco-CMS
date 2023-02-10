import { Observable } from 'rxjs';
import type { MemberDetails, MemberGroupDetails } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState, createObservablePart } from '@umbraco-cms/observable-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbEntityDetailStore, UmbStoreBase } from '@umbraco-cms/store';
import { umbMemberData } from 'src/core/mocks/data/member.data';

export const UMB_MEMBER_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberDetailStore>('UmbMemberDetailStore');

/**
 * @export
 * @class UmbMemberDetailStore
 * @extends {UmbStoreBase}
 * @description - Detail Data Store for Members
 */
export class UmbMemberDetailStore extends UmbStoreBase implements UmbEntityDetailStore<MemberDetails> {

	#data = new ArrayState<MemberDetails>([], x => x.key);
	public groups = this.#data.asObservable();

	constructor(private host: UmbControllerHostInterface) {
		super(host, UMB_MEMBER_DETAIL_STORE_CONTEXT_TOKEN.toString());
	}

	getScaffold(entityType: string, parentKey: string | null) {
		return {
		} as MemberDetails;
	}

	/**
	 * @description - Request a Member by key. The Member is added to the store and is returned as an Observable.
	 * @param {string} key
	 * @return {*}  {(Observable<MemberDetails>)}
	 * @memberof UmbMemberDetailStore
	 */
	getByKey(key: string): Observable<MemberGroupDetails> {
		// tryExecuteAndNotify(this.host, MemberResource.getMemberByKey({ key })).then(({ data }) => {
		// 	if (data) {}
		// 		this.#data.appendOne(data);
		// 	}
		// });

		// temp until Resource is updated
		const member = umbMemberData.getByKey(key);
		if (member) {
			this.#data.appendOne(member);
		}

		return createObservablePart(
			this.#data,
			(members) => members.find((member) => member.key === key) as MemberDetails
		);
	}

	async save(member: Array<MemberDetails>): Promise<void> {
		return null as any;
	}
}
