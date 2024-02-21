import { umbExtensionsRegistry } from '../../registry.js';
import type { ManifestTypes } from '../../models/index.js';
import { UmbRepositoryBase, type UmbCollectionRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbExtensionCollectionFilter {
	query?: string;
	skip: number;
	take: number;
	type?: ManifestTypes['type'];
}

export class UmbExtensionCollectionRepository extends UmbRepositoryBase implements UmbCollectionRepository {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	async requestCollection(filter: UmbExtensionCollectionFilter) {
		let extensions = umbExtensionsRegistry.getAllExtensions();

		if (filter.query) {
			const query = filter.query.toLowerCase();
			extensions = extensions.filter(
				(x) => x.name.toLowerCase().includes(query) || x.alias.toLowerCase().includes(query),
			);
		}

		if (filter.type) {
			extensions = extensions.filter((x) => x.type === filter.type);
		}

		extensions.sort((a, b) => a.type.localeCompare(b.type) || a.alias.localeCompare(b.alias));

		const total = extensions.length;
		const items = extensions.slice(filter.skip, filter.skip + filter.take);
		const data = { items, total };
		return { data };
	}

	destroy(): void {}
}

export default UmbExtensionCollectionRepository;
