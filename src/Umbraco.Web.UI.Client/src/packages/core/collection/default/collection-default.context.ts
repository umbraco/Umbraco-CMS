import { UmbCollectionConfiguration, UmbCollectionContext } from '../types.js';
import { UmbCollectionViewManager } from '../collection-view.manager.js';
import { UmbCollectionRepository } from '@umbraco-cms/backoffice/repository';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState, UmbNumberState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbApi, UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import {
	ManifestCollection,
	ManifestRepository,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import { UmbSelectionManager, UmbPaginationManager } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

export class UmbDefaultCollectionContext<
		CollectionItemType = any,
		FilterModelType extends UmbCollectionFilterModel = any,
	>
	extends UmbContextBase<UmbDefaultCollectionContext>
	implements UmbCollectionContext, UmbApi
{
	#manifest?: ManifestCollection;

	#items = new UmbArrayState<CollectionItemType>([], (x) => x);
	public readonly items = this.#items.asObservable();

	#totalItems = new UmbNumberState(0);
	public readonly totalItems = this.#totalItems.asObservable();

	#filter = new UmbObjectState<FilterModelType | object>({});
	public readonly filter = this.#filter.asObservable();

	repository?: UmbCollectionRepository;

	#initResolver?: () => void;
	#initialized = false;

	#init = new Promise<void>((resolve) => {
		this.#initialized ? resolve() : (this.#initResolver = resolve);
	});

	public readonly pagination = new UmbPaginationManager();
	public readonly selection = new UmbSelectionManager(this);
	public readonly view;

	constructor(host: UmbControllerHostElement, config: UmbCollectionConfiguration = { pageSize: 50 }) {
		super(host, UMB_DEFAULT_COLLECTION_CONTEXT);

		// listen for page changes on the pagination manager
		this.pagination.addEventListener(UmbChangeEvent.TYPE, this.#onPageChange);

		this.view = new UmbCollectionViewManager(this, { defaultViewAlias: config.defaultViewAlias });
		this.#configure(config);
	}

	// TODO: find a generic way to do this
	#checkIfInitialized() {
		if (this.repository) {
			this.#initialized = true;
			this.#initResolver?.();
		}
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
		if (!this.repository) throw new Error(`Missing repository for ${this.#manifest}`);

		const filter = this.#filter.getValue();
		const { data } = await this.repository.requestCollection(filter);

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

	#configure(configuration: UmbCollectionConfiguration) {
		this.selection.setMultiple(true);
		this.pagination.setPageSize(configuration.pageSize!);
		this.#filter.setValue({ ...this.#filter.getValue(), skip: 0, take: configuration.pageSize });
	}

	#onPageChange = (event: UmbChangeEvent) => {
		const target = event.target as UmbPaginationManager;
		const skipFilter = { skip: target.getSkip() } as Partial<FilterModelType>;
		this.setFilter(skipFilter);
	};

	#observeRepository(repositoryAlias: string) {
		new UmbExtensionApiInitializer<ManifestRepository<UmbCollectionRepository>>(
			this,
			umbExtensionsRegistry,
			repositoryAlias,
			[this._host],
			(permitted, ctrl) => {
				this.repository = permitted ? ctrl.api : undefined;
				this.#checkIfInitialized();
			},
		);
	}
}

export const UMB_DEFAULT_COLLECTION_CONTEXT = new UmbContextToken<UmbDefaultCollectionContext<any, any>>(
	'UmbCollectionContext',
);
