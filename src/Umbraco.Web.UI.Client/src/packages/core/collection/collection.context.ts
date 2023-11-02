import { UmbCollectionRepository } from '@umbraco-cms/backoffice/repository';
import { UmbBaseController, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import {
	UmbArrayState,
	UmbNumberState,
	UmbObjectState,
} from '@umbraco-cms/backoffice/observable-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { ManifestCollectionView, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';

export class UmbCollectionContext<ItemType, FilterModelType extends UmbCollectionFilterModel> extends UmbBaseController {
	protected entityType: string;
	protected init;

	#items = new UmbArrayState<ItemType>([]);
	public readonly items = this.#items.asObservable();

	#total = new UmbNumberState(0);
	public readonly total = this.#total.asObservable();

	#totalPages = new UmbNumberState(0);
	public readonly totalPages = this.#totalPages.asObservable();

	#currentPage = new UmbNumberState(1);
	public readonly currentPage = this.#currentPage.asObservable();

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

	constructor(host: UmbControllerHostElement, entityType: string, repositoryAlias: string) {
		super(host);
		this.entityType = entityType;

		this.#selectionManager.setMultiple(true);

		const currentUrl = new URL(window.location.href);
		this.collectionRootPathname = currentUrl.pathname.substring(0, currentUrl.pathname.lastIndexOf('/'));

		this.init = Promise.all([
			this.observe(
				umbExtensionsRegistry.getByTypeAndAlias('repository', repositoryAlias),
				async (repositoryManifest) => {
					if (repositoryManifest) {
						const result = await createExtensionApi(repositoryManifest, [this._host]);
						this.repository = result as UmbCollectionRepository;
						this.requestCollection();
					}
				},
				'umbCollectionRepositoryObserver'
			).asPromise(),

			this.observe(umbExtensionsRegistry.extensionsOfType('collectionView').pipe(
				map((extensions) => {
					return extensions.filter((extension) => extension.conditions.entityType === this.getEntityType());
				}),
			),
			(views) => {
				this.#views.next(views);
				this.#setCurrentView();
			}, 'umbCollectionViewsObserver').asPromise(),
		]);

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
			this.#total.next(data.total);
			this.#items.next(data.items);
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
