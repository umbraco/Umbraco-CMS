import type { UmbExtensionCollectionFilterModel, UmbExtensionCollectionItemModel } from '../types.js';
import { UMB_EXTENSION_ENTITY_TYPE } from '../../entity.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';

export class UmbExtensionCollectionRepository
	extends UmbRepositoryBase
	implements UmbCollectionRepository<UmbExtensionCollectionItemModel, UmbExtensionCollectionFilterModel>
{
	constructor(host: UmbControllerHost) {
		super(host);
	}

	async requestCollection(query: UmbExtensionCollectionFilterModel) {
		let extensions: Array<UmbExtensionCollectionItemModel> = umbExtensionsRegistry
			.getAllExtensions()
			.map((manifest) => {
				return {
					type: manifest.type,
					alias: manifest.alias,
					name: manifest.name,
					weight: manifest.weight,
					unique: manifest.alias,
					entityType: UMB_EXTENSION_ENTITY_TYPE,
				};
			});

		const skip = query.skip || 0;
		const take = query.take || 100;

		if (query.filter) {
			const text = query.filter.toLowerCase();
			extensions = extensions.filter(
				(x) => x.name.toLowerCase().includes(text) || x.alias.toLowerCase().includes(text),
			);
		}

		if (query.type) {
			extensions = extensions.filter((x) => x.type === query.type);
		}

		extensions.sort((a, b) => a.type.localeCompare(b.type) || a.alias.localeCompare(b.alias));

		const total = extensions.length;
		const items = extensions.slice(skip, skip + take);
		const data = { items, total };
		return { data };
	}
}

export { UmbExtensionCollectionRepository as api };
