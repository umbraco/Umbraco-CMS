import type { UmbMemberDetailModel } from '../../types.js';
import { UMB_MEMBER_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type { CreateMemberRequestModel, UpdateMemberRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MemberResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Member that fetches data from the server
 * @export
 * @class UmbMemberServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMemberServerDataSource implements UmbDetailDataSource<UmbMemberDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMemberServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMemberServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Member scaffold
	 * @param {Partial<UmbMemberDetailModel>} [preset]
	 * @return { CreateMemberRequestModel }
	 * @memberof UmbMemberServerDataSource
	 */
	async createScaffold(preset: Partial<UmbMemberDetailModel> = {}) {
		const data: UmbMemberDetailModel = {
			entityType: UMB_MEMBER_ENTITY_TYPE,
			unique: UmbId.new(),
			email: '',
			username: '',
			memberType: {
				unique: '',
			},
			isApproved: false,
			isLockedOut: false,
			isTwoFactorEnabled: false,
			failedPasswordAttempts: 0,
			lastLoginDate: null,
			lastLockoutDate: null,
			lastPasswordChangeDate: null,
			groups: [],
			values: [],
			variants: [],
			...preset,
		};

		return { data };
	}

	/**
	 * Fetches a Member with the given id from the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbMemberServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecuteAndNotify(this.#host, MemberResource.getMemberById({ id: unique }));

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const Member: UmbMemberDetailModel = {
			entityType: UMB_MEMBER_ENTITY_TYPE,
			unique: data.id,
			email: data.email,
			username: data.username,
			memberType: {
				unique: data.memberType.id,
			},
			isApproved: data.isApproved,
			isLockedOut: data.isLockedOut,
			isTwoFactorEnabled: data.isTwoFactorEnabled,
			failedPasswordAttempts: data.failedPasswordAttempts,
			lastLoginDate: data.lastLoginDate || null,
			lastLockoutDate: data.lastLockoutDate || null,
			lastPasswordChangeDate: data.lastPasswordChangeDate || null,
			groups: data.groups,
			values: data.values.map((value) => {
				return {
					culture: value.culture || null,
					segment: value.segment || null,
					alias: value.alias,
					value: value.value,
				};
			}),
			variants: data.variants.map((variant) => {
				return {
					culture: variant.culture || null,
					segment: variant.segment || null,
					name: variant.name,
					createDate: variant.createDate,
					updateDate: variant.updateDate,
				};
			}),
		};

		return { data: Member };
	}

	/**
	 * Inserts a new Member on the server
	 * @param {UmbMemberDetailModel} model
	 * @return {*}
	 * @memberof UmbMemberServerDataSource
	 */
	async create(model: UmbMemberDetailModel) {
		if (!model) throw new Error('Member is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateMemberRequestModel = {
			id: model.unique,
			email: model.email,
			username: model.username,
			password: '', // TODO: figure out what to get password from
			memberType: { id: model.memberType.unique },
			groups: model.groups,
			isApproved: model.isApproved,
			values: model.values,
			variants: model.variants,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			MemberResource.postMember({
				requestBody,
			}),
		);

		if (data) {
			return this.read(data);
		}

		return { error };
	}

	/**
	 * Updates a Member on the server
	 * @param {UmbMemberDetailModel} Member
	 * @return {*}
	 * @memberof UmbMemberServerDataSource
	 */
	async update(model: UmbMemberDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: UpdateMemberRequestModel = {
			email: model.email,
			groups: model.groups,
			isApproved: model.isApproved,
			isLockedOut: model.isLockedOut,
			isTwoFactorEnabled: model.isTwoFactorEnabled,
			newPassword: model.newPassword,
			oldPassword: model.oldPassword,
			username: model.username,
			values: model.values,
			variants: model.variants,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			MemberResource.putMemberById({
				id: model.unique,
				requestBody,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a Member on the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbMemberServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			MemberResource.deleteMemberById({
				id: unique,
			}),
		);
	}
}
