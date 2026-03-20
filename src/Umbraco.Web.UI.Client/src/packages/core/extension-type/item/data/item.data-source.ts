import { UMB_EXTENSION_TYPE_ENTITY_TYPE } from '../../entity.js';
import type { UmbExtensionTypeItemModel } from './types.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { fromCamelCase } from '@umbraco-cms/backoffice/utils';
import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';

export class UmbExtensionTypeItemDataSource implements UmbItemDataSource<UmbExtensionTypeItemModel> {
	async getItems(uniques: Array<string>) {
		const extensions = umbExtensionsRegistry.getAllExtensions();
		const allTypes = new Set(extensions.map((m) => m.type));

		const data: Array<UmbExtensionTypeItemModel> = uniques
			.filter((unique) => allTypes.has(unique))
			.map((type) => ({
				entityType: UMB_EXTENSION_TYPE_ENTITY_TYPE,
				unique: type,
				name: fromCamelCase(type),
				icon: 'icon-plugin',
			}));

		return { data };
	}
}
