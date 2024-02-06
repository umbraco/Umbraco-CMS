import { umbExtensionsRegistry } from '../../registry.js';
import { UmbRepositoryBase, type UmbCollectionRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbExtensionCollectionRepository extends UmbRepositoryBase implements UmbCollectionRepository {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	async requestCollection(filter: any) {
		const extensions = umbExtensionsRegistry.getAllExtensions();
		const total = extensions.length;
		const items = extensions.slice(filter.skip, filter.skip + filter.take);
		const data = { items, total };
		return { data };
	}

	destroy(): void {}
}

export default UmbExtensionCollectionRepository;
