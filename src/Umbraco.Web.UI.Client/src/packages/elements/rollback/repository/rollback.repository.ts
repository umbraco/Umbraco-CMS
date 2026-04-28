import { UmbElementRollbackServerDataSource } from './rollback.server.data-source.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type {
	UmbContentRollbackRepository,
	UmbContentRollbackVersionDetailModel,
	UmbContentRollbackVersionItemModel,
} from '@umbraco-cms/backoffice/content';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbElementRollbackRepository extends UmbControllerBase implements UmbContentRollbackRepository {
	#dataSource: UmbElementRollbackServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbElementRollbackServerDataSource(this);
	}

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
}

export default UmbElementRollbackRepository;
