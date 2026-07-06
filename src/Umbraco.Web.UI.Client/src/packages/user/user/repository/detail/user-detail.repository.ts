import type { UmbUserDetailModel } from '../../types.js';
import type { UmbUserDetailDataSource } from './types.js';
import { UmbUserServerDataSource } from './user-detail.server.data-source.js';
import type { UmbUserDetailStore } from './user-detail.store.js';
import { UMB_USER_DETAIL_STORE_CONTEXT } from './user-detail.store.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbDetailRepositoryBase,
	type UmbRepositoryResponseWithAsObservable,
} from '@umbraco-cms/backoffice/repository';

export class UmbUserDetailRepository extends UmbDetailRepositoryBase<UmbUserDetailModel, UmbUserDetailDataSource> {
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
	 * @returns {Promise<UmbRepositoryResponseWithAsObservable<Array<UmbUserDetailModel> | undefined>>}
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
	 * @param {UmbUserDetailModel} model
	 * @returns {*}
	 * @memberof UmbUserDetailRepository
	 */
	override async create(model: UmbUserDetailModel) {
		return super.create(model, null);
	}

	/**
	 * Requests the detail for the given unique
	 * @param unique
	 * @returns {*}
	 * @memberof UmbUserDetailRepository
	 */
	requestCalculateStartNodes(unique: string) {
		return this.detailDataSource.calculateStartNodes(unique);
	}
}

export default UmbUserDetailRepository;
