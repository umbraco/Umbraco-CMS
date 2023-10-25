import { UmbCollectionRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import {
	UmbArrayState,
	UmbNumberState,
	UmbObjectState,
	UmbObserverController,
} from '@umbraco-cms/backoffice/observable-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { ManifestCollectionView, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import { map } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbCollectionContext<ItemType, FilterModelType extends UmbCollectionFilterModel> {
	protected host: UmbControllerHostElement;
	protected entityType: string;
	protected init;

	#items = new UmbArrayState<ItemType>([]);
	public readonly items = this.#items.asObservable();

	#total = new UmbNumberState(0);
	public readonly total = this.#total.asObservable();

	#selection = new UmbArrayState<string>([]);
	public readonly selection = this.#selection.asObservable();

	#filter = new UmbObjectState<FilterModelType | object>({});
	public readonly filter = this.#filter.asObservable();

	#views = new UmbArrayState<ManifestCollectionView>([]);
	public readonly views = this.#views.asObservable();

	#currentView = new UmbObjectState<ManifestCollectionView | undefined>(undefined);
	public readonly currentView = this.#currentView.asObservable();

	repository?: UmbCollectionRepository;

	constructor(host: UmbControllerHostElement, entityType: string, repositoryAlias: string) {
		this.entityType = entityType;
		this.host = host;

		this.init = Promise.all([
			new UmbObserverController(
				this.host,
				umbExtensionsRegistry.getByTypeAndAlias('repository', repositoryAlias),
				async (repositoryManifest) => {
					if (repositoryManifest) {
						const result = await createExtensionApi(repositoryManifest, [this.host]);
						this.repository = result as UmbCollectionRepository;
						this.requestCollection();
					}
				},
			).asPromise(),

			new UmbObserverController(
				this.host,
				umbExtensionsRegistry.extensionsOfType('collectionView').pipe(
					map((extensions) => {
						return extensions.filter((extension) => extension.conditions.entityType === this.getEntityType());
					}),
				),
				(views) => {
					this.#views.next(views);

					if (!this.getCurrentView()) {
						/* TODO: Find a way to figure out which layout it starts with and set _currentLayout to that instead of [0]. eg. '/table'
						For document,media and members this will come as part of a data type configuration, but in other cases "users" we should find another way. */
						this.setCurrentView(views[0]);
					}
				},
			).asPromise(),
		]);
	}

	/**
	 * Returns true if the given id is selected.
	 * @param {string} id
	 * @return {Boolean}
	 * @memberof UmbCollectionContext
	 */
	public isSelected(id: string) {
		return this.#selection.getValue().includes(id);
	}

	/**
	 * Sets the current selection.
	 * @param {Array<string>} selection
	 * @memberof UmbCollectionContext
	 */
	public setSelection(selection: Array<string>) {
		if (!selection) return;
		this.#selection.next(selection);
	}

	/**
	 * Returns the current selection.
	 * @return {Array<string>}
	 * @memberof UmbCollectionContext
	 */
	public getSelection() {
		this.#selection.getValue();
	}

	/**
	 * Clears the current selection.
	 * @memberof UmbCollectionContext
	 */
	public clearSelection() {
		this.#selection.next([]);
	}

	/**
	 * Appends the given id to the current selection.
	 * @param {string} id
	 * @memberof UmbCollectionContext
	 */
	public select(id: string) {
		this.#selection.appendOne(id);
	}

	/**
	 * Removes the given id from the current selection.
	 * @param {string} id
	 * @memberof UmbCollectionContext
	 */
	public deselect(id: string) {
		this.#selection.filter((k) => k !== id);
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
	setFilter(filter: Partial<FilterModelType>) {
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
}

export const UMB_COLLECTION_CONTEXT = new UmbContextToken<UmbCollectionContext<any, any>>('UmbCollectionContext');
