import type { ManifestCollectionMenu } from '../extension/types.js';
import type { UmbCollectionRepository } from '../../repository/index.js';
import type { UmbCollectionItemModel } from '../../item/types.js';
import { UMB_COLLECTION_MENU_CONTEXT } from './default-collection-menu.context.token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbPaginationManager, UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry, type ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export class UmbDefaultCollectionMenuContext extends UmbContextBase {
	public selectableFilter?: (item: UmbCollectionItemModel) => boolean = () => true;
	public filter?: (item: UmbCollectionItemModel) => boolean = () => true;
	public filterArgs?: Record<string, unknown>;

	public readonly selection = new UmbSelectionManager(this);
	public readonly pagination = new UmbPaginationManager();

	#items = new UmbArrayState<UmbCollectionItemModel>([], (x) => x.unique);
	items = this.#items.asObservable();

	#manifest?: ManifestCollectionMenu;
	#repository?: UmbCollectionRepository;

	#paging = {
		skip: 0,
		take: 50,
	};

	#initResolver?: () => void;
	#initialized = false;

	#init = new Promise<void>((resolve) => {
		if (this.#initialized) {
			resolve();
		} else {
			this.#initResolver = resolve;
		}
	});

	constructor(host: UmbControllerHost) {
		super(host, UMB_COLLECTION_MENU_CONTEXT);

		this.pagination.setPageSize(this.#paging.take);
		//this.#consumeContexts();

		// listen for page changes on the pagination manager
		this.pagination.addEventListener(UmbChangeEvent.TYPE, this.#onPageChange);

		// always load the tree root because we need the root entity to reload the entire tree
		this.#loadItems();
	}

	/**
	 * Sets the manifest
	 * @param {ManifestTree} manifest
	 * @memberof UmbDefaultTreeContext
	 */
	public set manifest(manifest: ManifestCollectionMenu | undefined) {
		if (this.#manifest === manifest) return;
		this.#manifest = manifest;
		this.#observeRepository(this.#manifest?.meta.collectionRepositoryAlias);
	}
	public get manifest() {
		return this.#manifest;
	}

	#checkIfInitialized() {
		if (this.#repository) {
			this.#initialized = true;
			this.#initResolver?.();
		}
	}

	#onPageChange = (event: UmbChangeEvent) => {
		const target = event.target as UmbPaginationManager;
		this.#paging.skip = target.getSkip();
		this.#loadItems(true);
	};

	async #loadItems(loadMore = false) {
		await this.#init;

		const skip = loadMore ? this.#paging.skip : 0;
		const take = loadMore ? this.#paging.take : this.pagination.getCurrentPageNumber() * this.#paging.take;

		const { data } = await this.#repository!.requestCollection({
			...this.filterArgs,
			skip,
			take,
		});

		if (data) {
			if (loadMore) {
				const currentItems = this.#items.getValue();
				this.#items.setValue([...currentItems, ...data.items]);
			} else {
				this.#items.setValue(data.items);
			}

			this.pagination.setTotalItems(data.total);
		}
	}

	#observeRepository(repositoryAlias?: string) {
		if (!repositoryAlias) throw new Error('Collection Menu must have a repository alias.');

		new UmbExtensionApiInitializer<ManifestRepository<UmbCollectionRepository>>(
			this,
			umbExtensionsRegistry,
			repositoryAlias,
			[this],
			(permitted, ctrl) => {
				this.#repository = permitted ? ctrl.api : undefined;
				this.#checkIfInitialized();
			},
		);
	}
}

export { UmbDefaultCollectionMenuContext as api };
