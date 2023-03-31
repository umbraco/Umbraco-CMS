import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import type { MemberTypeDetails } from '@umbraco-cms/backoffice/models';
import { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';

/**
 * @description - A data source for the MemberType detail that fetches data from the server
 * @export
 * @class UmbMemberTypeDetailServerDataSource
 * @implements {MemberTypeDetailDataSource}
 */
export class UmbMemberTypeDetailServerDataSource implements UmbDetailRepository<MemberTypeDetails> {
	#host: UmbControllerHostElement;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * @description - Creates a new MemberType scaffold
	 * @return {*}
	 * @memberof UmbMemberTypeDetailServerDataSource
	 */
	async createScaffold() {
		const data = {} as MemberTypeDetails;
		return { data };
	}

	/**
	 * @description - Fetches a MemberType with the given key from the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbMemberTypeDetailServerDataSource
	 */
	requestByKey(key: string) {
		//return tryExecuteAndNotify(this.#host, MemberTypeResource.getMemberTypeByKey({ key }));
		// TODO => use backend cli when available.
		return tryExecuteAndNotify(this.#host, fetch(`/umbraco/management/api/v1/member-group/${key}`)) as any;
	}

	/**
	 * @description - Updates a MemberType on the server
	 * @param {MemberTypeDetails} memberType
	 * @return {*}
	 * @memberof UmbMemberTypeDetailServerDataSource
	 */
	async save(memberType: MemberTypeDetails) {
		if (!memberType.key) {
			const error: ProblemDetailsModel = { title: 'MemberType key is missing' };
			return { error };
		}

		const payload = { key: memberType.key, requestBody: memberType };
		//return tryExecuteAndNotify(this.#host, MemberTypeResource.putMemberTypeByKey(payload));

		// TODO => use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/member-type/${memberType.key}`, {
				method: 'PUT',
				body: JSON.stringify(payload),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		) as any;
	}

	/**
	 * @description - Inserts a new MemberType on the server
	 * @param {MemberTypeDetails} data
	 * @return {*}
	 * @memberof UmbMemberTypeDetailServerDataSource
	 */
	async create(data: MemberTypeDetails) {
		const requestBody = {
			name: data.name,
		};

		//return tryExecuteAndNotify(this.#host, MemberTypeResource.postMemberType({ requestBody }));
		// TODO => use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/member-type`, {
				method: 'POST',
				body: JSON.stringify(requestBody),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		) as any;
	}

	/**
	 * @description - Deletes a MemberType on the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbMemberTypeDetailServerDataSource
	 */
	async delete(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		//return await tryExecuteAndNotify(this.#host, MemberTypeResource.deleteMemberTypeByKey({ key }));
		// TODO => use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/member-type/${key}`, {
				method: 'DELETE',
			})
		) as any;
	}
}
