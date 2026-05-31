import type { UmbMemberTypeDetailModel } from '../../types.js';
import { UmbMemberTypeDetailServerDataSource } from './member-type-detail.server.data-source.js';
import { UMB_MEMBER_TYPE_DETAIL_STORE_CONTEXT } from './member-type-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbDetailRepositoryBase,
	type UmbRepositoryResponseWithAsObservable,
} from '@umbraco-cms/backoffice/repository';
import type { UmbDetailStore } from '@umbraco-cms/backoffice/store';

/**
 * A repository for the Member Type detail
 * @class UmbMemberTypeDetailRepository
 * @augments {UmbDetailRepositoryBase<UmbMemberTypeDetailModel>}
 */
export class UmbMemberTypeDetailRepository extends UmbDetailRepositoryBase<
	UmbMemberTypeDetailModel,
	UmbMemberTypeDetailServerDataSource
> {
	#init: Promise<unknown>;
	#detailStore?: UmbDetailStore<UmbMemberTypeDetailModel>;

	/**
	 * Creates an instance of UmbMemberTypeDetailRepository.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberTypeDetailRepository
	 */
	constructor(host: UmbControllerHost) {
		super(host, UmbMemberTypeDetailServerDataSource, UMB_MEMBER_TYPE_DETAIL_STORE_CONTEXT);

		this.#init = this.consumeContext(UMB_MEMBER_TYPE_DETAIL_STORE_CONTEXT, (instance) => {
			this.#detailStore = instance;
		})
			.asPromise({ preventTimeout: true })
			.catch(() => undefined);
	}

	/**
	 * Requests multiple member type details by their unique IDs
	 * @param {Array<string>} uniques - The unique IDs of the member types to fetch
	 * @returns {Promise<UmbRepositoryResponseWithAsObservable<Array<UmbMemberTypeDetailModel>>>}
	 * @memberof UmbMemberTypeDetailRepository
	 */
	async requestByUniques(
		uniques: Array<string>,
	): Promise<UmbRepositoryResponseWithAsObservable<Array<UmbMemberTypeDetailModel> | undefined>> {
		if (!uniques || uniques.length === 0) {
			return { data: [] };
		}

		await this.#init;

		const { data, error } = await this.detailDataSource.readMany(uniques);

		if (data) {
			data.forEach((item) => this.#detailStore?.append(item));
		}

		return {
			data,
			error,
			asObservable: () => this.#detailStore?.byUniques(uniques),
		};
	}
}

export default UmbMemberTypeDetailRepository;
