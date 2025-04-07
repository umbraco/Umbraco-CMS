import type { UmbMemberDetailModel } from '../../types.js';
import { UMB_MEMBER_ENTITY_TYPE } from '../../entity.js';
import { UmbMemberKind } from '../../utils/index.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type { CreateMemberRequestModel, UpdateMemberRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MemberService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Member that fetches data from the server
 * @class UmbMemberServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMemberServerDataSource implements UmbDetailDataSource<UmbMemberDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMemberServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Member scaffold
	 * @param {Partial<UmbMemberDetailModel>} [preset]
	 * @returns { CreateMemberRequestModel }
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
				icon: '',
			},
			isApproved: false,
			isLockedOut: false,
			isTwoFactorEnabled: false,
			kind: UmbMemberKind.DEFAULT,
			failedPasswordAttempts: 0,
			lastLoginDate: null,
			lastLockoutDate: null,
			lastPasswordChangeDate: null,
			groups: [],
			values: [],
			variants: [
				{
					name: '',
					culture: null,
					segment: null,
					createDate: new Date().toISOString(),
					updateDate: new Date().toISOString(),
				},
			],
			...preset,
		};

		return { data };
	}

	/**
	 * Fetches a Member with the given id from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbMemberServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(this.#host, MemberService.getMemberById({ id: unique }));

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
				icon: data.memberType.icon,
			},
			isApproved: data.isApproved,
			isLockedOut: data.isLockedOut,
			isTwoFactorEnabled: data.isTwoFactorEnabled,
			kind: data.kind,
			failedPasswordAttempts: data.failedPasswordAttempts,
			lastLoginDate: data.lastLoginDate || null,
			lastLockoutDate: data.lastLockoutDate || null,
			lastPasswordChangeDate: data.lastPasswordChangeDate || null,
			groups: data.groups,
			values: data.values.map((value) => {
				return {
					editorAlias: value.editorAlias,
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
	 * @returns {*}
	 * @memberof UmbMemberServerDataSource
	 */
	async create(model: UmbMemberDetailModel) {
		if (!model) throw new Error('Member is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateMemberRequestModel = {
			id: model.unique,
			email: model.email,
			username: model.username,
			password: model.newPassword || '',
			memberType: { id: model.memberType.unique },
			groups: model.groups,
			isApproved: model.isApproved,
			values: model.values,
			variants: model.variants,
		};

		const { data, error } = await tryExecute(
			this.#host,
			MemberService.postMember({
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
	 * @param model
	 * @returns {*}
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

		const { error } = await tryExecute(
			this.#host,
			MemberService.putMemberById({
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
	 * @returns {*}
	 * @memberof UmbMemberServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(
			this.#host,
			MemberService.deleteMemberById({
				id: unique,
			}),
		);
	}
}
