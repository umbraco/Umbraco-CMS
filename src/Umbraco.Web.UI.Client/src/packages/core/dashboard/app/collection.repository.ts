import type { UmbDashboardAppDetailModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbCollectionFilterModel, UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDashboardAppCollectionFilterModel extends UmbCollectionFilterModel {}

export class UmbDashboardAppCollectionRepository extends UmbControllerBase implements UmbCollectionRepository {
	async requestCollection(args: UmbDashboardAppCollectionFilterModel) {
		let manifests: Array<UmbDashboardAppDetailModel> = umbExtensionsRegistry
			.getByType('dashboardApp')
			.map((manifest) => {
				return {
					unique: manifest.alias,
					entityType: manifest.type,
					name: manifest.meta.headline,
				};
			});

		const skip = args.skip || 0;
		const take = args.take || 100;

		if (args.filter) {
			const text = args.filter.toLowerCase();
			manifests = manifests.filter((x) => x.name.toLowerCase().includes(text));
		}

		manifests.sort((a, b) => a.name.localeCompare(b.name));

		const total = manifests.length;
		const items = manifests.slice(skip, skip + take);
		const data = { items, total };
		return { data, error: undefined };
	}
}

export { UmbDashboardAppCollectionRepository as api };
