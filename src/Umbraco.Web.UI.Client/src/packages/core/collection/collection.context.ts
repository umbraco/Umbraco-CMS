import { UmbCollectionConfiguration } from './types.js';
import { UmbCollectionRepository } from '@umbraco-cms/backoffice/repository';
import { UmbBaseController, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import {
	UmbArrayState,
	UmbNumberState,
	UmbObjectState,
} from '@umbraco-cms/backoffice/observable-api';
import { UmbExtensionsManifestInitializer, createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { ManifestCollectionView, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import { UmbSelectionManager, UmbPaginationManager } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

export class UmbCollectionContext<ItemType, FilterModelType extends UmbCollectionFilterModel> extends UmbBaseController {
	protected entityType: string;
	protected init;

	#items = new UmbArrayState<ItemType>([]);
	public readonly items = this.#items.asObservable();

	#totalItems = new UmbNumberState(0);
	public readonly totalItems = this.#totalItems.asObservable();

	#selectionManager = new UmbSelectionManager();
	public readonly selection = this.#selectionManager.selection;

	#filter = new UmbObjectState<FilterModelType | object>({});
	public readonly filter = this.#filter.asObservable();

	#views = new UmbArrayState<ManifestCollectionView>([]);
	public readonly views = this.#views.asObservable();

	#currentView = new UmbObjectState<ManifestCollectionView | undefined>(undefined);
	public readonly currentView = this.#currentView.asObservable();

	repository?: UmbCollectionRepository;
	collectionRootPathname: string;

	public readonly pagination = new UmbPaginationManager();

	constructor(host: UmbControllerHostElement, entityType: string, repositoryAlias: string, config: UmbCollectionConfiguration = { pageSize: 50 }) {
		super(host);
		this.entityType = entityType;

		// listen for page changes on the pagination manager
		this.pagination.addEventListener(UmbChangeEvent.TYPE, this.#onPageChange);

		const currentUrl = new URL(window.location.href);
		this.collectionRootPathname = currentUrl.pathname.substring(0, currentUrl.pathname.lastIndexOf('/'));

		this.init = Promise.all([
			this.#observeRepository(repositoryAlias).asPromise(),
			this.#observeViews().asPromise(),
		]);

		this.#configure(config);

		this.provideContext(UMB_COLLECTION_CONTEXT, this);
	}

	/**
	 * Returns true if the given id is selected.
	 * @param {string} id
	 * @return {Boolean}
	 * @memberof UmbCollectionContext
	 */
	public isSelected(id: string) {
		return this.#selectionManager.isSelected(id);
	}

	/**
	 * Sets the current selection.
	 * @param {Array<string>} selection
	 * @memberof UmbCollectionContext
	 */
	public setSelection(selection: Array<string>) {
		this.#selectionManager.setSelection(selection);
	}

	/**
	 * Returns the current selection.
	 * @return {Array<string>}
	 * @memberof UmbCollectionContext
	 */
	public getSelection() {
		this.#selectionManager.getSelection();
	}

	/**
	 * Clears the current selection.
	 * @memberof UmbCollectionContext
	 */
	public clearSelection() {
		this.#selectionManager.clearSelection();
	}

	/**
	 * Appends the given id to the current selection.
	 * @param {string} id
	 * @memberof UmbCollectionContext
	 */
	public select(id: string) {
		this.#selectionManager.select(id);
	}

	/**
	 * Removes the given id from the current selection.
	 * @param {string} id
	 * @memberof UmbCollectionContext
	 */
	public deselect(id: string) {
		this.#selectionManager.deselect(id);
	}

	/**
	 * Returns the collection entity type
	 * @return {string}
	 * @memberof UmbCollectionContext
	 */
	public getEntityType() {
		return this.entityType;
	}

	/**
	 * Requests the collection from the repository.
	 * @return {*}
	 * @memberof UmbCollectionContext
	 */
	public async requestCollection() {
		if (!this.repository) return;

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
		this.#selectionManager.setMultiple(true);
		this.pagination.setPageSize(configuration.pageSize);
		this.#filter.next({ ...this.#filter.getValue(), skip: 0, take: configuration.pageSize });
	}

	#observeRepository(repositoryAlias: string) {
		return this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('repository', repositoryAlias),
			async (repositoryManifest) => {
				if (repositoryManifest) {
					// TODO: Maybe use the UmbExtensionApiController instead of createExtensionApi, to ensure usage of conditions:
					const result = await createExtensionApi(repositoryManifest, [this._host]);
					this.repository = result as UmbCollectionRepository;
					this.requestCollection();
				}
			},
			'umbCollectionRepositoryObserver'
		)
	}

	#observeViews() {
		return new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'collectionView', null, (views) => {
			this.#views.next(views.map(view => view.manifest));
			this.#setCurrentView();
		});
	}

	#onPageChange = (event: UmbChangeEvent) => {
		const target = event.target as UmbPaginationManager;
		const skipFilter = { skip: target.getSkip() } as Partial<FilterModelType>;
		this.setFilter(skipFilter);
	}

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
}

export const UMB_COLLECTION_CONTEXT = new UmbContextToken<UmbCollectionContext<any, any>>('UmbCollectionContext');
