import type { UmbUserDetailModel, UmbUserStartNodesModel } from '../../types.js';
import type { UmbUserDetailDataSource } from './types.js';
import { UmbUserServerDataSource } from './user-detail.server.data-source.js';
import type { UmbUserDetailStore } from './user-detail.store.js';
import { UMB_USER_DETAIL_STORE_CONTEXT } from './user-detail.store.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbDetailRepositoryBase,
	type UmbDataSourceResponse,
	type UmbRepositoryResponse,
	type UmbRepositoryResponseWithAsObservable,
} from '@umbraco-cms/backoffice/repository';

export class UmbUserDetailRepository extends UmbDetailRepositoryBase<UmbUserDetailModel, UmbUserDetailDataSource> {
	// TODO: Consider promoting `requestByUniques` into `UmbDetailRepositoryBase` so every detail
	// repository shares it; this class only re-consumes the store context because the base's
	// `#init`/`#detailStore` are private.
	#init: Promise<unknown>;
	#detailStore?: UmbUserDetailStore;

	constructor(host: UmbControllerHost) {
		super(host, UmbUserServerDataSource, UMB_USER_DETAIL_STORE_CONTEXT);

		this.#init = this.consumeContext(UMB_USER_DETAIL_STORE_CONTEXT, (instance) => {
			this.#detailStore = instance;
		})
			.asPromise({ preventTimeout: true })
			.catch(() => {
				this.#detailStore = undefined;
			});
	}

	/**
	 * Requests multiple user details by their unique IDs
	 * @param {Array<string>} uniques - The unique IDs of the users to fetch
	 * @returns {Promise<UmbRepositoryResponseWithAsObservable<Array<UmbUserDetailModel> | undefined>>} The requested user details
	 * @memberof UmbUserDetailRepository
	 */
	async requestByUniques(
		uniques: Array<string>,
	): Promise<UmbRepositoryResponseWithAsObservable<Array<UmbUserDetailModel> | undefined>> {
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

	/**
	 * Creates a new User detail
	 * @param {UmbUserDetailModel} model - The user model to create
	 * @returns {Promise<UmbRepositoryResponse<UmbUserDetailModel>>} The created user details
	 * @memberof UmbUserDetailRepository
	 */
	override async create(model: UmbUserDetailModel): Promise<UmbRepositoryResponse<UmbUserDetailModel>> {
		return super.create(model, null);
	}

	/**
	 * Requests the detail for the given unique
	 * @param {string} unique - The unique id of the user
	 * @returns {Promise<UmbDataSourceResponse<UmbUserStartNodesModel>>} The calculated start nodes for the user
	 * @memberof UmbUserDetailRepository
	 */
	requestCalculateStartNodes(unique: string): Promise<UmbDataSourceResponse<UmbUserStartNodesModel>> {
		return this.detailDataSource.calculateStartNodes(unique);
	}
}

export default UmbUserDetailRepository;
