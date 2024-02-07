import type { UmbMemberGroupDetailModel } from '../../types.js';
import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Member Group that fetches data from the server
 * @export
 * @class UmbMemberGroupServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMemberGroupServerDataSource implements UmbDetailDataSource<UmbMemberGroupDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMemberGroupServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMemberGroupServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Member Group scaffold
	 * @param {(string | null)} parentUnique
	 * @return { CreateMemberGroupRequestModel }
	 * @memberof UmbMemberGroupServerDataSource
	 */
	async createScaffold() {
		const data: UmbMemberGroupDetailModel = {
			entityType: UMB_MEMBER_GROUP_ENTITY_TYPE,
			unique: UmbId.new(),
			name: '',
		};

		return { data };
	}

	/**
	 * Fetches a Member Group with the given id from the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbMemberGroupServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		/*
		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			MemberGroupResource.getMemberGroupById({ id: unique }),
		);
		*/

		// TODO => use backend cli when available.
		const { data, error } = (await tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/member-group/${unique}`),
		)) as any;

		const json = await data.json(); // remove this line when backend cli is available

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const memberGroup: UmbMemberGroupDetailModel = {
			entityType: UMB_MEMBER_GROUP_ENTITY_TYPE,
			unique: json.id,
			name: json.name,
		};

		return { data: memberGroup };
	}

	/**
	 * Inserts a new Member Group on the server
	 * @param {UmbMemberGroupDetailModel} model
	 * @return {*}
	 * @memberof UmbMemberGroupServerDataSource
	 */
	async create(model: UmbMemberGroupDetailModel) {
		if (!model) throw new Error('Member Group is missing');

		// TODO: make data mapper to prevent errors
		// TODO: add type CreateMemberGroupRequestModel
		const requestBody: any = {
			id: model.unique,
			name: model.name,
		};

		/*
		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			MemberGroupResource.postMemberGroup({
				requestBody,
			}),
		);
		*/

		// TODO => use backend cli when available.
		const { data, error } = (await tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/member-group`, { method: 'POST', body: JSON.stringify(requestBody) }),
		)) as any;

		const newUnqiue = data.headers.get('Umb-Generated-Resource'); // TODO: remove when backend cli is available

		if (data) {
			return this.read(newUnqiue);
		}

		return { error };
	}

	/**
	 * Updates a MemberGroup on the server
	 * @param {UmbMemberGroupDetailModel} MemberGroup
	 * @return {*}
	 * @memberof UmbMemberGroupServerDataSource
	 */
	async update(model: UmbMemberGroupDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		// TODO:  add type UpdateMemberGroupRequestModel
		const requestBody: any = {
			name: model.name,
		};

		/*
		const { error } = await tryExecuteAndNotify(
			this.#host,
			MemberGroupResource.putMemberGroupById({
				id: model.unique,
				requestBody,
			}),
		);
		*/

		const { error } = (await tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/member-group/${model.unique}`, {
				method: 'PUT',
				body: JSON.stringify(requestBody),
			}),
		)) as any;

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a Member Group on the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbMemberGroupServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		/*
		return tryExecuteAndNotify(
			this.#host,
			MemberGroupResource.deleteMemberGroupById({
				id: unique,
			}),
		);
		*/

		const { error } = (await tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/member-group/${unique}`, {
				method: 'DELETE',
			}),
		)) as any;

		return { error };
	}
}
