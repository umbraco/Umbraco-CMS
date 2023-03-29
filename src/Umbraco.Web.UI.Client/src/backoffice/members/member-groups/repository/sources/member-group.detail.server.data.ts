import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import type { MemberGroupDetails } from '@umbraco-cms/backoffice/models';
import { UmbDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * @description - A data source for the MemberGroup detail that fetches data from the server
 * @export
 * @class UmbMemberGroupDetailServerDataSource
 * @implements {MemberGroupDetailDataSource}
 */
// TODO => Provide type when it is available
export class UmbMemberGroupDetailServerDataSource implements UmbDataSource<any> {
	#host: UmbControllerHostElement;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * @description - Creates a new MemberGroup scaffold
	 * @return {*}
	 * @memberof UmbMemberGroupDetailServerDataSource
	 */
	async createScaffold() {
		const data: MemberGroupDetails = {
			name: '',
		} as MemberGroupDetails;

		return { data };
	}

	/**
	 * @description - Fetches a MemberGroup with the given key from the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbMemberGroupDetailServerDataSource
	 */
	get(key: string) {
		//return tryExecuteAndNotify(this.#host, MemberGroupResource.getMemberGroup({ key })) as any;
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(this.#host, fetch(`/umbraco/management/api/v1/member-group/${key}`)) as any;
	}

	/**
	 * @description - Updates a MemberGroup on the server
	 * @param {MemberGroupDetails} memberGroup
	 * @return {*}
	 * @memberof UmbMemberGroupDetailServerDataSource
	 */
	async update(memberGroup: MemberGroupDetails) {
		if (!memberGroup.key) {
			const error: ProblemDetailsModel = { title: 'Member Group key is missing' };
			return { error };
		}

		const payload = { key: memberGroup.key, requestBody: memberGroup };
		//return tryExecuteAndNotify(this.#host, MemberGroupResource.putMemberGroupByKey(payload));
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/member-group/${memberGroup.key}`, {
				method: 'PUT',
				body: JSON.stringify(payload),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		) as any;
	}

	/**
	 * @description - Inserts a new MemberGroup on the server
	 * @param {MemberGroupDetails} data
	 * @return {*}
	 * @memberof UmbMemberGroupDetailServerDataSource
	 */
	async insert(data: MemberGroupDetails) {
		const requestBody = {
			name: data.name,
		};

		//return tryExecuteAndNotify(this.#host, MemberGroupResource.postMemberGroup({ requestBody }));
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/member-group/`, {
				method: 'POST',
				body: JSON.stringify(requestBody),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		) as any;
	}

	/**
	 * @description - Deletes a MemberGroup on the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbMemberGroupDetailServerDataSource
	 */
	async trash(key: string) {
		return this.delete(key);
	}

	/**
	 * @description - Deletes a MemberGroup on the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbMemberGroupDetailServerDataSource
	 */
	async delete(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		//return await tryExecuteAndNotify(this.#host, MemberGroupResource.deleteMemberGroupByKey({ key }));
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/member-group/${key}`, {
				method: 'DELETE',
			})
		) as any;
	}
}
