import { UmbCollectionViewManager } from '../collection-view.manager.js';
import type { UmbCollectionViewManagerConfig } from '../collection-view.manager.js';
import type {
	UmbCollectionColumnConfiguration,
	UmbCollectionConfiguration,
	UmbCollectionContext,
	UmbCollectionLayoutConfiguration,
} from '../types.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbArrayState, UmbNumberState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbSelectionManager, UmbPaginationManager } from '@umbraco-cms/backoffice/utils';
import type { ManifestCollection, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbCollectionFilterModel, UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const LOCAL_STORAGE_KEY = 'umb-collection-view';

export class UmbDefaultCollectionContext<
		CollectionItemType = any,
		FilterModelType extends UmbCollectionFilterModel = any,
	>
	extends UmbContextBase<UmbDefaultCollectionContext>
	implements UmbCollectionContext, UmbApi
{
	#config?: UmbCollectionConfiguration = { pageSize: 50 };
	#manifest?: ManifestCollection;
	#repository?: UmbCollectionRepository;

	#items = new UmbArrayState<CollectionItemType>([], (x) => x);
	public readonly items = this.#items.asObservable();

	#totalItems = new UmbNumberState(0);
	public readonly totalItems = this.#totalItems.asObservable();

	#filter = new UmbObjectState<FilterModelType | object>({});
	public readonly filter = this.#filter.asObservable();

	#userDefinedProperties = new UmbArrayState<UmbCollectionColumnConfiguration>([], (x) => x.alias);
	public readonly userDefinedProperties = this.#userDefinedProperties.asObservable();

	#viewLayouts = new UmbArrayState<UmbCollectionLayoutConfiguration>([], (x) => x.collectionView);
	public readonly viewLayouts = this.#viewLayouts.asObservable();

	public readonly pagination = new UmbPaginationManager();
	public readonly selection = new UmbSelectionManager(this);
	public readonly view = new UmbCollectionViewManager(this);

	#defaultViewAlias: string;
	#defaultFilter: Partial<FilterModelType>;

	#initResolver?: () => void;
	#initialized = false;

	#init = new Promise<void>((resolve) => {
		this.#initialized ? resolve() : (this.#initResolver = resolve);
	});

	constructor(host: UmbControllerHost, defaultViewAlias: string, defaultFilter: Partial<FilterModelType> = {}) {
		super(host, UMB_COLLECTION_CONTEXT);

		this.#defaultViewAlias = defaultViewAlias;
		this.#defaultFilter = defaultFilter;

		this.pagination.addEventListener(UmbChangeEvent.TYPE, this.#onPageChange);
	}

	#configured = false;

	#configure() {
		if (!this.#config) return;

		this.selection.setMultiple(true);

		if (this.#config.pageSize) {
			this.pagination.setPageSize(this.#config.pageSize);
		}

		this.#filter.setValue({
			...this.#defaultFilter,
			...this.#config,
			...this.#filter.getValue(),
			skip: 0,
			take: this.#config.pageSize,
		});

		this.#userDefinedProperties.setValue(this.#config?.userDefinedProperties ?? []);

		const viewManagerConfig: UmbCollectionViewManagerConfig = { defaultViewAlias: this.#defaultViewAlias };

		if (this.#config.layouts && this.#config.layouts.length > 0) {
			this.#viewLayouts.setValue(this.#config.layouts);
			const aliases = this.#config.layouts.map((layout) => layout.collectionView);
			viewManagerConfig.manifestFilter = (manifest) => aliases.includes(manifest.alias);
		}

		this.view.setConfig(viewManagerConfig);

		this.#configured = true;
	}

	#checkIfInitialized() {
		if (this.#repository) {
			this.#initialized = true;
			this.#initResolver?.();
		}
	}

	#observeRepository(repositoryAlias: string) {
		new UmbExtensionApiInitializer<ManifestRepository<UmbCollectionRepository>>(
			this,
			umbExtensionsRegistry,
			repositoryAlias,
			[this._host],
			(permitted, ctrl) => {
				this.#repository = permitted ? ctrl.api : undefined;
				this.#checkIfInitialized();
			},
		);
	}

	#onPageChange = (event: UmbChangeEvent) => {
		const target = event.target as UmbPaginationManager;
		const skipFilter = { skip: target.getSkip() } as Partial<FilterModelType>;
		this.setFilter(skipFilter);
	};

	/**
	 * Sets the configuration for the collection.
	 * @param {UmbCollectionConfiguration} config
	 * @memberof UmbCollectionContext
	 */
	public setConfig(config: UmbCollectionConfiguration) {
		this.#config = config;
	}

	public getConfig() {
		return this.#config;
	}

	/**
	 * Sets the manifest for the collection.
	 * @param {ManifestCollection} manifest
	 * @memberof UmbCollectionContext
	 */
	public setManifest(manifest: ManifestCollection | undefined) {
		if (this.#manifest === manifest) return;
		this.#manifest = manifest;

		if (!this.#manifest) return;
		this.#observeRepository(this.#manifest.meta.repositoryAlias);
	}

	/**
	 * Returns the manifest for the collection.
	 * @return {ManifestCollection}
	 * @memberof UmbCollectionContext
	 */
	public getManifest() {
		return this.#manifest;
	}

	/**
	 * Requests the collection from the repository.
	 * @return {*}
	 * @memberof UmbCollectionContext
	 */
	public async requestCollection() {
		await this.#init;

		if (!this.#configured) this.#configure();

		if (!this.#repository) throw new Error(`Missing repository for ${this.#manifest}`);

		const filter = this.#filter.getValue();
		const { data } = await this.#repository.requestCollection(filter);

		if (data) {
			this.#items.setValue(data.items);
			this.#totalItems.setValue(data.total);
			this.pagination.setTotalItems(data.total);
		}
	}

	/**
	 * Sets the filter for the collection and refreshes the collection.
	 * @param {Partial<FilterModelType>} filter
	 * @memberof UmbCollectionContext
	 */
	public setFilter(filter: Partial<FilterModelType>) {
		this.#filter.setValue({ ...this.#filter.getValue(), ...filter });
		this.requestCollection();
	}

	public getLastSelectedView(unique: string | undefined): string | undefined {
		if (!unique) return;

		const layouts = JSON.parse(localStorage.getItem(LOCAL_STORAGE_KEY) ?? '{}') ?? {};
		if (!layouts) return;

		return layouts[unique];
	}

	public setLastSelectedView(unique: string | undefined, viewAlias: string) {
		if (!unique) return;

		const layouts = JSON.parse(localStorage.getItem(LOCAL_STORAGE_KEY) ?? '{}') ?? {};
		if (!layouts) return;

		layouts[unique] = viewAlias;

		localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(layouts));
	}
}

export const UMB_COLLECTION_CONTEXT = new UmbContextToken<UmbDefaultCollectionContext>('UmbCollectionContext');

/**
 * @deprecated Use UMB_COLLECTION_CONTEXT instead.
 */
export { UMB_COLLECTION_CONTEXT as UMB_DEFAULT_COLLECTION_CONTEXT };
