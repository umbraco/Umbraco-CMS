import type { UmbExtensionTypeCollectionFilterModel, UmbExtensionTypeCollectionItemModel } from '../types.js';
import { UMB_EXTENSION_TYPE_ENTITY_TYPE } from '../../entity.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { fromCamelCase } from '@umbraco-cms/backoffice/utils';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';

export class UmbExtensionTypeCollectionRepository
	extends UmbRepositoryBase
	implements UmbCollectionRepository<UmbExtensionTypeCollectionItemModel, UmbExtensionTypeCollectionFilterModel>
{
	async requestCollection(query: UmbExtensionTypeCollectionFilterModel) {
		const extensions = umbExtensionsRegistry.getAllExtensions();
		const types = [...new Set(extensions.map((m) => m.type))];

		let items: Array<UmbExtensionTypeCollectionItemModel> = types.sort().map((type) => ({
			unique: type,
			name: fromCamelCase(type),
			entityType: UMB_EXTENSION_TYPE_ENTITY_TYPE,
		}));

		if (query.filter) {
			const text = query.filter.toLowerCase();
			items = items.filter((x) => x.name?.toLowerCase().includes(text));
		}

		const skip = query.skip ?? 0;
		const take = query.take ?? items.length;
		const total = items.length;
		const paged = items.slice(skip, skip + take);

		return { data: { items: paged, total } };
	}
}

export { UmbExtensionTypeCollectionRepository as api };
