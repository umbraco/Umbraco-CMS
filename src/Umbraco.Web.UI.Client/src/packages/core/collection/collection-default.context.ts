import { UmbCollectionConfiguration } from './types.js';
import { UmbCollectionRepository } from '@umbraco-cms/backoffice/repository';
import { UmbBaseController, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState, UmbNumberState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import {
	UmbApi,
	UmbExtensionApiInitializer,
	UmbExtensionsManifestInitializer,
} from '@umbraco-cms/backoffice/extension-api';
import {
	ManifestCollectionView,
	ManifestRepository,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import { UmbSelectionManager, UmbPaginationManager } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

export class UmbCollectionDefaultContext<ItemType, FilterModelType extends UmbCollectionFilterModel>
	extends UmbBaseController
	implements UmbApi
{
	#alias?: string;

	#items = new UmbArrayState<ItemType>([]);
	public readonly items = this.#items.asObservable();

	#totalItems = new UmbNumberState(0);
	public readonly totalItems = this.#totalItems.asObservable();

	#filter = new UmbObjectState<FilterModelType | object>({});
	public readonly filter = this.#filter.asObservable();

	#views = new UmbArrayState<ManifestCollectionView>([]);
	public readonly views = this.#views.asObservable();

	#currentView = new UmbObjectState<ManifestCollectionView | undefined>(undefined);
	public readonly currentView = this.#currentView.asObservable();

	repository?: UmbCollectionRepository;
	collectionRootPathname: string;

	#initResolver?: () => void;
	#initialized = false;

	#init = new Promise<void>((resolve) => {
		this.#initialized ? resolve() : (this.#initResolver = resolve);
	});

	public readonly pagination = new UmbPaginationManager();
	public readonly selection = new UmbSelectionManager();

	constructor(host: UmbControllerHostElement, config: UmbCollectionConfiguration = { pageSize: 50 }) {
		super(host);

		// listen for page changes on the pagination manager
		this.pagination.addEventListener(UmbChangeEvent.TYPE, this.#onPageChange);

		const currentUrl = new URL(window.location.href);
		this.collectionRootPathname = currentUrl.pathname.substring(0, currentUrl.pathname.lastIndexOf('/'));

		this.#configure(config);

		this.provideContext(UMB_COLLECTION_CONTEXT, this);
	}

	// TODO: find a generic way to do this
	#checkIfInitialized() {
		if (this.repository) {
			this.#initialized = true;
			this.#initResolver?.();
		}
	}

	public setAlias(alias?: string) {
		if (this.#alias === alias) return;

		this.#alias = alias;
		this.#observeCollectionManifest();
	}

	public getAlias() {
		return this.#alias;
	}

	/**
	 * Requests the collection from the repository.
	 * @return {*}
	 * @memberof UmbCollectionContext
	 */
	public async requestCollection() {
		await this.#init;
		if (!this.repository) throw new Error(`Missing repository for ${this.#alias}`);

		const filter = this.#filter.getValue();
		const { data } = await this.repository.requestCollection(filter);

		if (data) {
			this.#items.next(data.items);
			this.#totalItems.next(data.total);
			this.pagination.setTotalItems(data.total);
		}
	}

	/**
	 * Sets the filter for the collection and refreshes the collection.
	 * @param {Partial<FilterModelType>} filter
	 * @memberof UmbCollectionContext
	 */
	public setFilter(filter: Partial<FilterModelType>) {
		this.#filter.next({ ...this.#filter.getValue(), ...filter });
		this.requestCollection();
	}

	// Views
	/**
	 * Sets the current view.
	 * @param {ManifestCollectionView} view
	 * @memberof UmbCollectionContext
	 */
	public setCurrentView(view: ManifestCollectionView) {
		this.#currentView.next(view);
	}

	/**
	 * Returns the current view.
	 * @return {ManifestCollectionView}
	 * @memberof UmbCollectionContext
	 */
	public getCurrentView() {
		return this.#currentView.getValue();
	}

	#configure(configuration: UmbCollectionConfiguration) {
		this.selection.setMultiple(true);
		this.pagination.setPageSize(configuration.pageSize);
		this.#filter.next({ ...this.#filter.getValue(), skip: 0, take: configuration.pageSize });
	}

	#onPageChange = (event: UmbChangeEvent) => {
		const target = event.target as UmbPaginationManager;
		const skipFilter = { skip: target.getSkip() } as Partial<FilterModelType>;
		this.setFilter(skipFilter);
	};

	#setCurrentView() {
		const currentUrl = new URL(window.location.href);
		const lastPathSegment = currentUrl.pathname.split('/').pop();
		const views = this.#views.getValue();
		const viewMatch = views.find((view) => view.meta.pathName === lastPathSegment);

		/* TODO: Find a way to figure out which layout it starts with and set _currentLayout to that instead of [0]. eg. '/table'
			For document, media and members this will come as part of a data type configuration, but in other cases "users" we should find another way.
			This should only happen if the current layout is not set in the URL.
		*/
		const currentView = viewMatch || views[0];
		this.setCurrentView(currentView);
	}

	#observeCollectionManifest() {
		if (!this.#alias) return;

		this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('collection', this.#alias),
			async (manifest) => {
				if (!manifest) return;
				const repositoryAlias = manifest.meta.repositoryAlias;
				if (!repositoryAlias) throw new Error('A collection must have a repository alias.');
				this.#observeRepository(repositoryAlias);
				this.#observeViews();
			},
			'umbObserveCollectionManifest',
		);
	}

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

	#observeViews() {
		return new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'collectionView', null, (views) => {
			this.#views.next(views.map((view) => view.manifest));
			this.#setCurrentView();
		});
	}
}

export const UMB_COLLECTION_CONTEXT = new UmbContextToken<UmbCollectionDefaultContext<any, any>>(
	'UmbCollectionContext',
);
