import { UMB_EXTENSION_ENTITY_TYPE } from '../../entity.js';
import { UMB_EXTENSION_ITEM_STORE_CONTEXT } from './item.store.context-token.js';
import type { UmbExtensionItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbItemStore } from '@umbraco-cms/backoffice/store';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { of } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbExtensionItemRepository extends UmbRepositoryBase implements UmbItemRepository<UmbExtensionItemModel> {
	#init: Promise<unknown>;
	#itemStore?: UmbItemStore<UmbExtensionItemModel>;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#init = this.consumeContext(UMB_EXTENSION_ITEM_STORE_CONTEXT, (instance) => {
			this.#itemStore = instance;
		})
			.asPromise({ preventTimeout: true })
			.catch(() => undefined);
	}

	async requestItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		try {
			await this.#init;
		} catch {
			return {
				asObservable: () => undefined,
			};
		}

		const extensions = umbExtensionsRegistry.getAllExtensions();

		const items: Array<UmbExtensionItemModel> = extensions
			.filter((manifest) => uniques.includes(manifest.alias))
			.map((manifest) => ({
				entityType: UMB_EXTENSION_ENTITY_TYPE,
				unique: manifest.alias,
				name: manifest.name,
				description: manifest.alias,
				icon: 'icon-plugin',
				manifest: {
					type: manifest.type,
					alias: manifest.alias,
					name: manifest.name,
					weight: manifest.weight,
					kind: manifest.kind,
				},
			}));

		if (items.length > 0) {
			this.#itemStore?.appendItems(items);
		}

		return { data: items, asObservable: () => this.#itemStore?.items(uniques) };
	}

	async items(uniques: Array<string>) {
		try {
			await this.#init;
		} catch {
			return undefined;
		}

		if (!this.#itemStore) {
			return of([]);
		}

		return this.#itemStore.items(uniques);
	}
}

export { UmbExtensionItemRepository as api };
