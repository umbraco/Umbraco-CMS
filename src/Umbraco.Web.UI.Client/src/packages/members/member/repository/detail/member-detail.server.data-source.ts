import type { UmbMemberDetailModel } from '../../types.js';
import { UMB_MEMBER_ENTITY_TYPE, UMB_MEMBER_PROPERTY_VALUE_ENTITY_TYPE } from '../../entity.js';
import { UmbMemberKind } from '../../utils/index.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type { CreateMemberRequestModel, UpdateMemberRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MemberService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { umbDeepMerge, type UmbDeepPartialObject } from '@umbraco-cms/backoffice/utils';
import { UmbMemberTypeDetailServerDataSource } from '@umbraco-cms/backoffice/member-type';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * A data source for the Member that fetches data from the server
 * @class UmbMemberServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMemberServerDataSource extends UmbControllerBase implements UmbDetailDataSource<UmbMemberDetailModel> {
	/**
	 * Creates a new Member scaffold
	 * @param {Partial<UmbMemberDetailModel>} [preset]
	 * @returns { CreateMemberRequestModel }
	 * @memberof UmbMemberServerDataSource
	 */
	async createScaffold(preset: UmbDeepPartialObject<UmbMemberDetailModel> = {}) {
		let memberTypeIcon = '';

		const memberTypeUnique = preset.memberType?.unique;

		if (!memberTypeUnique) {
			throw new Error('Document type unique is missing');
		}

		const { data } = await new UmbMemberTypeDetailServerDataSource(this).read(memberTypeUnique);
		memberTypeIcon = data?.icon ?? '';

		const defaultData: UmbMemberDetailModel = {
			entityType: UMB_MEMBER_ENTITY_TYPE,
			unique: UmbId.new(),
			email: '',
			username: '',
			memberType: {
				unique: memberTypeUnique,
				icon: memberTypeIcon,
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
			flags: [],
			variants: [
				{
					name: '',
					culture: null,
					segment: null,
					createDate: new Date().toISOString(),
					updateDate: new Date().toISOString(),
					flags: [],
				},
			],
		};

		const scaffold = umbDeepMerge(preset, defaultData);

		return { data: scaffold };
	}

	/**
	 * Fetches a Member with the given id from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbMemberServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(this, MemberService.getMemberById({ path: { id: unique } }));

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
					alias: value.alias,
					culture: value.culture || null,
					editorAlias: value.editorAlias,
					entityType: UMB_MEMBER_PROPERTY_VALUE_ENTITY_TYPE,
					segment: value.segment || null,
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
					// TODO: Transfer member flags when available in the API: [NL]
					flags: [], //variant.flags,
				};
			}),
			flags: data.flags,
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
		const body: CreateMemberRequestModel = {
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
			this,
			MemberService.postMember({
				body,
			}),
		);

		if (data && typeof data === 'string') {
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
		const body: UpdateMemberRequestModel = {
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
			this,
			MemberService.putMemberById({
				path: { id: model.unique },
				body,
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
			this,
			MemberService.deleteMemberById({
				path: { id: unique },
			}),
		);
	}
}
