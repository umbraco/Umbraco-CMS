import type { UmbExtensionCollectionFilterModel, UmbExtensionCollectionItemModel } from '../types.js';
import { UMB_EXTENSION_ENTITY_TYPE } from '../../entity.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';

export class UmbExtensionCollectionRepository
	extends UmbRepositoryBase
	implements UmbCollectionRepository<UmbExtensionCollectionItemModel, UmbExtensionCollectionFilterModel>
{
	async requestCollection(query: UmbExtensionCollectionFilterModel) {
		let extensions: Array<UmbExtensionCollectionItemModel> = umbExtensionsRegistry
			.getAllExtensions()
			.map((manifest) => {
				return {
					unique: manifest.alias,
					name: manifest.name,
					description: manifest.alias,
					icon: 'icon-plugin',
					entityType: UMB_EXTENSION_ENTITY_TYPE,
					manifest: {
						type: manifest.type,
						alias: manifest.alias,
						name: manifest.name,
						weight: manifest.weight,
						kind: manifest.kind,
					},
				};
			});

		const skip = query.skip || 0;
		const take = query.take || 100;

		if (query.filter) {
			const text = query.filter.toLowerCase();
			extensions = extensions.filter(
				(x) => x.manifest.name.toLowerCase().includes(text) || x.manifest.alias.toLowerCase().includes(text),
			);
		}

		if (query.type) {
			new UmbDeprecation({
				deprecated: 'UmbExtensionCollectionFilterModel .type property.',
				removeInVersion: '19',
				solution: 'Use the .extensionTypes property instead.',
			}).warn();
		}

		const types = query.extensionTypes ?? (query.type ? [query.type] : undefined);
		if (types?.length) {
			extensions = extensions.filter((x) => types.includes(x.manifest.type));
		}

		extensions.sort(
			(a, b) => a.manifest.type.localeCompare(b.manifest.type) || a.manifest.alias.localeCompare(b.manifest.alias),
		);

		const total = extensions.length;
		const items = extensions.slice(skip, skip + take);
		const data = { items, total };
		return { data };
	}
}

export { UmbExtensionCollectionRepository as api };
