import { UmbElementRollbackServerDataSource } from './rollback.server.data-source.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type {
	UmbContentRollbackRepository,
	UmbContentRollbackVersionDetailModel,
	UmbContentRollbackVersionItemModel,
} from '@umbraco-cms/backoffice/content';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Repository for the element rollback feature.
 * @class UmbElementRollbackRepository
 * @augments {UmbControllerBase}
 * @implements {UmbContentRollbackRepository}
 */
export class UmbElementRollbackRepository extends UmbControllerBase implements UmbContentRollbackRepository {
	#dataSource: UmbElementRollbackServerDataSource;

	/**
	 * Creates an instance of UmbElementRollbackRepository.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbElementRollbackRepository
	 */
	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbElementRollbackServerDataSource(this);
	}

	/**
	 * Request the available versions for an element entity.
	 * @param {string} id - The unique ID of the element
	 * @param {string} [culture] - Optional culture to filter versions by
	 * @returns {Promise<{ data?: { items: Array<UmbContentRollbackVersionItemModel>; total: number }; error?: unknown }>} The list of versions, or an error
	 * @memberof UmbElementRollbackRepository
	 */
	async requestVersionsByEntityId(
		id: string,
		culture?: string,
	): Promise<{ data?: { items: Array<UmbContentRollbackVersionItemModel>; total: number }; error?: unknown }> {
		const { data, error } = await this.#dataSource.getVersionsByElementId(id, culture);

		if (data) {
			return {
				data: {
					items: data.items.map((item) => ({
						id: item.id,
						versionDate: item.versionDate,
						user: { id: item.user.id },
						isCurrentDraftVersion: item.isCurrentDraftVersion,
						isCurrentPublishedVersion: item.isCurrentPublishedVersion,
						preventCleanup: item.preventCleanup,
					})),
					total: data.total,
				},
			};
		}

		return { error };
	}

	/**
	 * Request the details of a specific version.
	 * @param {string} id - The unique ID of the version
	 * @returns {Promise<{ data?: UmbContentRollbackVersionDetailModel; error?: unknown }>} The version details, or an error
	 * @memberof UmbElementRollbackRepository
	 */
	async requestVersionById(id: string): Promise<{ data?: UmbContentRollbackVersionDetailModel; error?: unknown }> {
		const { data, error } = await this.#dataSource.getVersionById(id);

		if (data) {
			return {
				data: {
					id: data.id,
					variants: data.variants.map((v) => ({
						culture: v.culture ?? null,
						name: v.name,
					})),
					values: data.values.map((v) => ({
						culture: v.culture ?? null,
						alias: v.alias,
						value: v.value,
					})),
				},
			};
		}

		return { error };
	}

	/**
	 * Toggle whether a specific version is excluded from automatic content version cleanup.
	 * @param {string} versionId - The unique ID of the version
	 * @param {boolean} preventCleanup - `true` to prevent cleanup, `false` to allow it
	 * @returns {*} The result of the operation
	 * @memberof UmbElementRollbackRepository
	 */
	async setPreventCleanup(versionId: string, preventCleanup: boolean) {
		return await this.#dataSource.setPreventCleanup(versionId, preventCleanup);
	}

	/**
	 * Roll the element back to a specific version.
	 * @param {string} versionId - The unique ID of the version to roll back to
	 * @param {string} [culture] - Optional culture to roll back
	 * @returns {*} The result of the operation
	 * @memberof UmbElementRollbackRepository
	 */
	async rollback(versionId: string, culture?: string) {
		return await this.#dataSource.rollback(versionId, culture);
	}
}

export default UmbElementRollbackRepository;
