import { UmbRollbackServerDataSource } from './rollback.server.data-source.js';
import type {
	UmbContentRollbackRepository,
	UmbContentRollbackVersionDetailModel,
	UmbContentRollbackVersionItemModel,
} from '@umbraco-cms/backoffice/content';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { PagedDocumentVersionItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

export class UmbDocumentRollbackRepository extends UmbControllerBase implements UmbContentRollbackRepository {
	#dataSource: UmbRollbackServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbRollbackServerDataSource(this);
	}

	async requestVersionsByEntityId(
		id: string,
		culture?: string,
	): Promise<{ data?: { items: Array<UmbContentRollbackVersionItemModel>; total: number }; error?: unknown }> {
		const { data, error } = await this.#dataSource.getVersionsByDocumentId(id, culture);

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

	async setPreventCleanup(versionId: string, preventCleanup: boolean) {
		return await this.#dataSource.setPreventCleanup(versionId, preventCleanup);
	}

	async rollback(versionId: string, culture?: string) {
		return await this.#dataSource.rollback(versionId, culture);
	}

	/**
	 * @param {string} id - The document unique identifier.
	 * @param {string} [culture] - The culture to get versions for.
	 * @returns {UmbRepositoryResponse<PagedDocumentVersionItemResponseModel>} The versions for the document.
	 * @deprecated Use {@link requestVersionsByEntityId} instead. Scheduled for removal in Umbraco 19.
	 */
	async requestVersionsByDocumentId(
		id: string,
		culture?: string,
	): Promise<UmbRepositoryResponse<PagedDocumentVersionItemResponseModel>> {
		return await this.#dataSource.getVersionsByDocumentId(id, culture);
	}
}

/**
 * @deprecated Use {@link UmbDocumentRollbackRepository} instead. Scheduled for removal in Umbraco 19.
 */
export { UmbDocumentRollbackRepository as UmbRollbackRepository };

export default UmbDocumentRollbackRepository;
