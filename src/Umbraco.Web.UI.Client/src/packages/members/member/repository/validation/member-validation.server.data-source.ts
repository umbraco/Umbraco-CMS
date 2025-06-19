import type { UmbMemberDetailModel } from '../../types.js';
import {
	type CreateMemberRequestModel,
	MemberService,
	type UpdateMemberRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * A server data source for Member Validation
 */
export class UmbMemberValidationServerDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Validate a new Member on the server
	 * @param {UmbMemberDetailModel} model - Member Model
	 * @param {UmbEntityUnique} parentUnique - Parent Unique
	 * @returns {*} - The response from the server
	 */
	async validateCreate(
		model: UmbMemberDetailModel,
		parentUnique: UmbEntityUnique = null,
	): Promise<UmbDataSourceResponse<string>> {
		if (!model) throw new Error('Member is missing');
		if (!model.unique) throw new Error('Member unique is missing');
		if (!model.newPassword) throw new Error('Member newPassword is missing');
		if (parentUnique === undefined) throw new Error('Parent unique is missing');

		// TODO: make data mapper to prevent errors
		const body: CreateMemberRequestModel = {
			email: model.email,
			username: model.username,
			password: model.newPassword,
			isApproved: model.isApproved,
			id: model.unique,
			memberType: { id: model.memberType.unique },
			values: model.values,
			variants: model.variants,
		};

		const { data, error } = await tryExecute(
			this.#host,
			MemberService.postMemberValidate({
				body,
			}),
			{ disableNotifications: true },
		);

		if (data && typeof data === 'string') {
			return { data };
		}

		return { error };
	}

	/**
	 * Validate a existing Member
	 * @param {UmbMemberDetailModel} model - Member Model
	 * @param {Array<UmbVariantId>} variantIds - Variant Ids
	 * @returns {Promise<*>} - The response from the server
	 */
	async validateUpdate(
		model: UmbMemberDetailModel,
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		variantIds: Array<UmbVariantId>,
	): Promise<UmbDataSourceResponse<string>> {
		if (!model.unique) throw new Error('Unique is missing');

		//const cultures = variantIds.map((id) => id.culture).filter((culture) => culture !== null) as Array<string>;

		// TODO: make data mapper to prevent errors
		const body: UpdateMemberRequestModel = {
			email: model.email,
			username: model.username,
			isApproved: model.isApproved,
			isLockedOut: model.isLockedOut,
			isTwoFactorEnabled: model.isTwoFactorEnabled,
			values: model.values,
			variants: model.variants,
		};

		// Maybe use: tryExecuteAndNotify
		const { data, error } = await tryExecute(
			this.#host,
			MemberService.putMemberByIdValidate({
				path: { id: model.unique },
				body,
			}),
			{ disableNotifications: true },
		);

		if (data && typeof data === 'string') {
			return { data };
		}

		return { error };
	}
}
