import { UmbCollectionViewManager } from '../view/collection-view.manager.js';
import type { UmbCollectionViewManagerConfig } from '../view/collection-view.manager.js';
import type {
	UmbCollectionColumnConfiguration,
	UmbCollectionConfiguration,
	UmbCollectionContext,
	UmbCollectionLayoutConfiguration,
} from '../types.js';
import type { UmbCollectionFilterModel } from '../collection-filter-model.interface.js';
import type { UmbCollectionRepository } from '../repository/collection-repository.interface.js';
import type { ManifestCollection } from '../extensions/types.js';
import { UmbCollectionBulkActionManager } from '../bulk-action/collection-bulk-action.manager.js';
import { UmbCollectionSelectionManager } from '../selection/collection-selection.manager.js';
import { UMB_COLLECTION_CONTEXT } from './collection-default.context-token.js';
import { UmbInteractionMemoryManager } from '@umbraco-cms/backoffice/interaction-memory';
import { UmbValueSummaryCoordinatorContext } from '@umbraco-cms/backoffice/value-summary';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbArrayState,
	UmbBasicState,
	UmbBooleanState,
	UmbNumberState,
	UmbObjectState,
} from '@umbraco-cms/backoffice/observable-api';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbPaginationManager, UmbDeprecation, debounce } from '@umbraco-cms/backoffice/utils';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import type { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_ENTITY_CONTEXT, UmbParentEntityContext, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { UmbModalRouteRegistrationController, type UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';

const LOCAL_STORAGE_KEY = 'umb-collection-view';

const FILTER_MEMORY_UNIQUE = 'UmbCollectionFilter';
const ORDER_MEMORY_UNIQUE = 'UmbCollectionOrder';
const PAGINATION_MEMORY_UNIQUE = 'UmbCollectionPagination';

/**
 * The parts of a collection filter that are remembered in the interaction memory.
 */
type UmbCollectionMemorizedFilter = {
	filter?: string;
	orderBy?: string;
	orderDirection?: string;
	skip?: number;
};

export class UmbDefaultCollectionContext<
	CollectionItemType extends { entityType: string; unique: string } = any,
	FilterModelType extends UmbCollectionFilterModel = UmbCollectionFilterModel,
>
	extends UmbContextBase
	implements UmbCollectionContext, UmbApi
{
	#config?: UmbCollectionConfiguration = { pageSize: 50 };
	protected _manifest?: ManifestCollection;
	protected _repository?: UmbCollectionRepository;

	// TODO: replace with a state manager
	protected _loading = new UmbObjectState<boolean>(false);
	public readonly loading = this._loading.asObservable();

	protected _items = new UmbArrayState<CollectionItemType>([], (x) => x.unique);
	public readonly items = this._items.asObservable();

	protected _totalItems = new UmbNumberState(0);
	public readonly totalItems = this._totalItems.asObservable();

	protected _filter = new UmbObjectState<FilterModelType | object>({});
	public readonly filter = this._filter.asObservable();

	protected _selectOnly = new UmbBooleanState(undefined);
	public readonly selectOnly = this._selectOnly.asObservable();

	#workspacePathBuilder = new UmbBasicState<UmbModalRouteBuilder | undefined>(undefined);
	public readonly workspacePathBuilder = this.#workspacePathBuilder.asObservable();

	#userDefinedProperties = new UmbArrayState<UmbCollectionColumnConfiguration>([], (x) => x.alias);
	public readonly userDefinedProperties = this.#userDefinedProperties.asObservable();

	#viewLayouts = new UmbArrayState<UmbCollectionLayoutConfiguration>([], (x) => x.collectionView);
	public readonly viewLayouts = this.#viewLayouts.asObservable();

	public readonly pagination = new UmbPaginationManager();
	public readonly selection = new UmbCollectionSelectionManager(this);
	public readonly interactionMemory = new UmbInteractionMemoryManager(this);
	public readonly view = new UmbCollectionViewManager(this, { interactionMemoryManager: this.interactionMemory });
	public readonly bulkAction = new UmbCollectionBulkActionManager(this);
	public readonly valueSummaryCoordinator = new UmbValueSummaryCoordinatorContext(this);

	#defaultViewAlias: string;
	#defaultFilter: Partial<FilterModelType>;
	#pageNumberToRestore?: number;

	#initResolver?: () => void;
	#initialized = false;

	protected _init = new Promise<void>((resolve) => {
		if (this.#initialized) {
			resolve();
		} else {
			this.#initResolver = resolve;
		}
	});

	#actionEventContext: UmbActionEventContext | undefined;
	#parentEntityContext = new UmbParentEntityContext(this);

	constructor(host: UmbControllerHost, defaultViewAlias: string, defaultFilter: Partial<FilterModelType> = {}) {
		super(host, UMB_COLLECTION_CONTEXT);

		this.#defaultViewAlias = defaultViewAlias;
		this.#defaultFilter = defaultFilter;

		this.pagination.addEventListener(UmbChangeEvent.TYPE, this.#onPageChange);
		this.#listenToEntityEvents();
		this.#observeInteractionMemory();

		// The parent entity context is used to get the parent entity for the collection items
		// All items in the collection are children of the current entity context
		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			this.observe(
				context?.unique,
				(currentEntityUnique) => {
					const currentEntityType = context?.getEntityType();

					const parent: UmbEntityModel | undefined =
						currentEntityUnique && currentEntityType
							? ({
									unique: currentEntityUnique,
									entityType: currentEntityType,
								} satisfies UmbEntityModel)
							: undefined;

					this.#parentEntityContext?.setParent(parent);
				},
				'_observeEntityContextUnique',
			);
		});
	}

	setupView(viewElement: UmbControllerHost) {
		// TODO: Consider to remove this one as well:
		new UmbModalRouteRegistrationController(viewElement, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('entity/:entityType')
			.onSetup((params) => {
				return { data: { entityType: params.entityType, preset: {} } };
			})
			.onReject(() => {
				// TODO: Maybe this can be removed?
				this._requestCollection();
			})
			.onSubmit(() => {
				// TODO: Maybe this can be removed?
				this._requestCollection();
			})
			.observeRouteBuilder((routeBuilder) => {
				this.#workspacePathBuilder.setValue(routeBuilder);
			});
	}

	async #listenToEntityEvents() {
		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
			this.#actionEventContext = context;

			context?.removeEventListener(
				UmbRequestReloadStructureForEntityEvent.TYPE,
				this.#onReloadStructureRequest as unknown as EventListener,
			);

			context?.removeEventListener(
				UmbRequestReloadChildrenOfEntityEvent.TYPE,
				this.#onReloadChildrenRequest as unknown as EventListener,
			);

			context?.addEventListener(
				UmbRequestReloadStructureForEntityEvent.TYPE,
				this.#onReloadStructureRequest as unknown as EventListener,
			);

			context?.addEventListener(
				UmbRequestReloadChildrenOfEntityEvent.TYPE,
				this.#onReloadChildrenRequest as unknown as EventListener,
			);
		});
	}

	protected _configured = false;

	protected _configure() {
		if (!this.#config) return;

		this.#configureSelection();
		this.bulkAction.setConfig(this.#config.bulkActionConfiguration);

		// Observe bulk actions to enable selection when bulk actions are available
		// Bulk Actions are an integrated part of a Collection so we handle it here instead of a configuration
		this.observe(
			this.bulkAction.hasBulkActions,
			(hasBulkActions) => {
				// Allow selection if there are bulk actions available
				if (hasBulkActions) {
					// TODO: This is a temporary workaround until we support two types of selection (bulk action selection and normal selection)
					// We have to use the same selection configuration for both types of selection to ensure that selection works as expected in multi vs single select mode (ex: pickers).
					// We currently disable bulk actions in pickers until we have a solution in place for supporting both types of selection.
					// With this workaround the experience will be that a collection, supporting bulk actions configured as single select, will only be able to select one item at a time.
					const config = this.#config?.selectionConfiguration;
					const selectable = config?.selectable ?? true;
					const multiple = config?.multiple ?? true;
					this.selection.setSelectable(selectable);
					this.selection.setMultiple(multiple);
				}
			},
			'umbCollectionHasBulkActionsObserver',
		);

		this.pagination.setPageSize(this.#config.pageSize ?? 50);

		const filterValue = this._filter.getValue() as FilterModelType;

		this._filter.setValue({
			...this.#defaultFilter,
			...this.#config,
			...filterValue,
			skip: filterValue.skip ?? 0,
			take: this.pagination.getPageSize(),
		});

		this.#userDefinedProperties.setValue(this.#config?.userDefinedProperties ?? []);

		this.#configureViews();

		this._configured = true;

		this.#applyInteractionMemory();
	}

	#checkIfInitialized() {
		if (this._repository) {
			this.#initialized = true;
			this.#initResolver?.();
		}
	}

	#observeRepository(repositoryAlias?: string) {
		if (!repositoryAlias) throw new Error('Tree must have a repository alias.');

		new UmbExtensionApiInitializer<ManifestRepository<UmbCollectionRepository>>(
			this,
			umbExtensionsRegistry,
			repositoryAlias,
			[this._host],
			(permitted, ctrl) => {
				this._repository = permitted ? ctrl.api : undefined;
				this.#checkIfInitialized();
			},
		);
	}

	#onPageChange = (event: UmbChangeEvent) => {
		const target = event.target as UmbPaginationManager;
		const skip = target.getSkip();
		// The page can be corrected by the pagination manager itself, ex. when the total amount of items shrinks.
		// Only request the collection again when this actually moves us somewhere else.
		if (this.#getFilterValue().skip === skip) return;
		this.setFilter({ skip } as Partial<FilterModelType>);
	};

	/**
	 * Sets the configuration for the collection.
	 * @param {UmbCollectionConfiguration} config The collection configuration.
	 * @memberof UmbCollectionContext
	 */
	public setConfig(config: UmbCollectionConfiguration) {
		this.#config = config;
		this._configure();
	}

	public getConfig() {
		return this.#config;
	}

	public set manifest(manifest: ManifestCollection | undefined) {
		if (this._manifest === manifest) return;
		this._manifest = manifest;
		this.#observeRepository(this._manifest?.meta.repositoryAlias);
	}
	public get manifest() {
		return this._manifest;
	}

	public getEmptyLabel(): string {
		return this.manifest?.meta.noItemsLabel ?? this.#config?.noItemsLabel ?? '#collection_noItemsTitle';
	}

	/* debouncing the load collection method because multiple filters can be set at the same time
	that will trigger multiple load calls with different filter arguments */
	public loadCollection = debounce(() => this._requestCollection(), 100);

	/**
	 * Requests the collection from the repository.
	 * @returns {Promise<void>}
	 * @deprecated Deprecated since v.17.0.0. Use `loadCollection` instead.
	 * @memberof UmbCollectionContext
	 */
	public async requestCollection() {
		new UmbDeprecation({
			removeInVersion: '19.0.0',
			deprecated: 'requestCollection',
			solution: 'Use .loadCollection method instead',
		}).warn();

		return this._requestCollection();
	}

	protected async _requestCollection() {
		await this._init;

		if (!this._configured) this._configure();
		if (!this._repository) throw new Error(`Missing repository for ${this._manifest}`);

		this._loading.setValue(true);

		const filter = this._filter.getValue();
		const { data } = await this._repository.requestCollection(filter);

		if (data) {
			this._items.setValue(data.items);
			this._setTotalItems(data.total);
		}

		this._loading.setValue(false);
	}

	/**
	 * Sets the filter for the collection and refreshes the collection.
	 * @param {Partial<FilterModelType>} filter The filter to merge into the current filter.
	 * @memberof UmbCollectionContext
	 */
	public setFilter(filter: Partial<FilterModelType>) {
		this._filter.setValue({ ...this._filter.getValue(), ...filter });
		this.loadCollection();
	}

	public updateFilter(filter: Partial<FilterModelType>) {
		this._filter.setValue({ ...this._filter.getValue(), ...filter });
	}

	/**
	 * Returns the current filter of the collection.
	 * @returns {(FilterModelType | object)} The current filter.
	 * @memberof UmbCollectionContext
	 */
	public getFilter(): FilterModelType | object {
		return this._filter.getValue();
	}

	#getFilterValue(): UmbCollectionMemorizedFilter {
		return this._filter.getValue() as UmbCollectionMemorizedFilter;
	}

	// TODO: The filter, order and pagination memories below would ideally live in the managers owning that state, like
	// the view manager does with its own. That requires a filter manager first, so we move them over time. [MR]
	#observeInteractionMemory() {
		this.observe(this.filter, () => this.#writeInteractionMemory(), 'umbCollectionInteractionMemoryWriteObserver');

		this.observe(
			this.interactionMemory.memories,
			() => this.#applyInteractionMemory(),
			'umbCollectionInteractionMemoryObserver',
		);
	}

	/**
	 * Sets the total amount of items of the collection and moves the pagination to the remembered page.
	 * @param {number} totalItems - The total amount of items in the collection.
	 * @memberof UmbCollectionContext
	 */
	protected _setTotalItems(totalItems: number) {
		this._totalItems.setValue(totalItems);
		this.pagination.setTotalItems(totalItems);

		// The remembered page can only be given to the pagination manager once it knows the amount of pages, as it
		// corrects a page beyond the last one.
		if (this.#pageNumberToRestore === undefined) return;
		const pageNumber = this.#pageNumberToRestore;
		this.#pageNumberToRestore = undefined;
		this.pagination.setCurrentPageNumber(pageNumber);
	}

	/**
	 * Writes the parts of the filter that are worth remembering to the interaction memory.
	 * Only deviations from the configured defaults are remembered, so a collection without memories behaves as configured.
	 */
	#writeInteractionMemory() {
		if (!this._configured) return;

		const filter = this.#getFilterValue();

		if (filter.filter) {
			this.interactionMemory.setMemory({
				unique: FILTER_MEMORY_UNIQUE,
				value: { filter: filter.filter },
			});
		} else {
			this.interactionMemory.deleteMemory(FILTER_MEMORY_UNIQUE);
		}

		const configuredOrder = this.#getConfiguredOrder();
		const isConfiguredOrder =
			filter.orderBy === configuredOrder.orderBy && filter.orderDirection === configuredOrder.orderDirection;

		if (filter.orderBy && !isConfiguredOrder) {
			this.interactionMemory.setMemory({
				unique: ORDER_MEMORY_UNIQUE,
				value: { orderBy: filter.orderBy, orderDirection: filter.orderDirection },
			});
		} else {
			this.interactionMemory.deleteMemory(ORDER_MEMORY_UNIQUE);
		}

		const pageNumber = this.#getPageNumberOfSkip(filter.skip);

		if (pageNumber > 1) {
			this.interactionMemory.setMemory({
				unique: PAGINATION_MEMORY_UNIQUE,
				value: { pageNumber },
			});
		} else {
			this.interactionMemory.deleteMemory(PAGINATION_MEMORY_UNIQUE);
		}
	}

	/**
	 * Folds the interaction memory into the filter. Values already present in the filter are left alone, which is
	 * what makes this safe to run again when memories arrive after the collection has been configured.
	 */
	#applyInteractionMemory() {
		if (!this._configured) return;

		const filter = this.#getFilterValue();
		const filterMemory = this.interactionMemory.getMemory(FILTER_MEMORY_UNIQUE)?.value;
		const orderMemory = this.interactionMemory.getMemory(ORDER_MEMORY_UNIQUE)?.value;
		const paginationMemory = this.interactionMemory.getMemory(PAGINATION_MEMORY_UNIQUE)?.value;

		const memorized: UmbCollectionMemorizedFilter = {};

		if (filterMemory?.filter !== undefined && filterMemory.filter !== filter.filter) {
			memorized.filter = filterMemory.filter;
		}

		if (orderMemory?.orderBy !== undefined && orderMemory.orderBy !== filter.orderBy) {
			memorized.orderBy = orderMemory.orderBy;
		}

		if (orderMemory?.orderDirection !== undefined && orderMemory.orderDirection !== filter.orderDirection) {
			memorized.orderDirection = orderMemory.orderDirection;
		}

		const pageNumber = paginationMemory?.pageNumber;

		if (pageNumber !== undefined && pageNumber !== this.#getPageNumberOfSkip(filter.skip)) {
			this.#pageNumberToRestore = pageNumber;
			memorized.skip = (pageNumber - 1) * this.pagination.getPageSize();
		}

		if (Object.keys(memorized).length === 0) return;

		this.setFilter(memorized as Partial<FilterModelType>);
	}

	/**
	 * The ordering the collection has when nothing has been remembered. It can come from the default filter of a
	 * specialized collection context as well as from the configuration.
	 * @returns {Pick<UmbCollectionMemorizedFilter, 'orderBy' | 'orderDirection'>} The configured ordering.
	 */
	#getConfiguredOrder(): Pick<UmbCollectionMemorizedFilter, 'orderBy' | 'orderDirection'> {
		const defaults = { ...this.#defaultFilter, ...this.#config } as UmbCollectionMemorizedFilter;
		return { orderBy: defaults.orderBy, orderDirection: defaults.orderDirection };
	}

	#getPageNumberOfSkip(skip: number | undefined): number {
		const pageSize = this.pagination.getPageSize();
		if (!skip || !pageSize) return 1;
		return Math.floor(skip / pageSize) + 1;
	}

	/**
	 * Returns the alias of the view the collection was last left in.
	 * @param {(string | undefined)} unique - The unique of the entity holding the collection.
	 * @returns {(string | undefined)} The alias of the last selected view.
	 * @deprecated Deprecated since v17. The current view is remembered in the interaction memory of the collection,
	 * see the `UmbCollectionCurrentView` memory on `.interactionMemory`. Scheduled for removal in Umbraco 19.
	 */
	public getLastSelectedView(unique: string | undefined): string | undefined {
		new UmbDeprecation({
			deprecated: 'UmbDefaultCollectionContext.getLastSelectedView()',
			removeInVersion: '19.0.0',
			solution: 'Read the UmbCollectionCurrentView memory from the interactionMemory of the collection instead.',
		}).warn();

		if (!unique) return undefined;

		const layouts = JSON.parse(localStorage.getItem(LOCAL_STORAGE_KEY) ?? '{}') ?? {};
		if (!layouts) return undefined;

		return layouts[unique];
	}

	/**
	 * Stores the alias of the view the collection was last left in.
	 * @param {(string | undefined)} unique - The unique of the entity holding the collection.
	 * @param {string} viewAlias - The alias of the view.
	 * @deprecated Deprecated since v17. The current view is remembered in the interaction memory of the collection,
	 * see the `UmbCollectionCurrentView` memory on `.interactionMemory`. Scheduled for removal in Umbraco 19.
	 */
	public setLastSelectedView(unique: string | undefined, viewAlias: string) {
		new UmbDeprecation({
			deprecated: 'UmbDefaultCollectionContext.setLastSelectedView()',
			removeInVersion: '19.0.0',
			solution: 'The current view is remembered by the collection view manager, no call is needed.',
		}).warn();

		if (!unique) return;

		const layouts = JSON.parse(localStorage.getItem(LOCAL_STORAGE_KEY) ?? '{}') ?? {};
		if (!layouts) return;

		layouts[unique] = viewAlias;

		localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(layouts));
	}

	#onReloadStructureRequest = (event: UmbRequestReloadStructureForEntityEvent) => {
		const items = this._items.getValue();
		const hasItem = items.some((item) => item.unique === event.getUnique());
		if (hasItem) {
			this._requestCollection();
		}
	};

	#onReloadChildrenRequest = async (event: UmbRequestReloadChildrenOfEntityEvent) => {
		// check if the collection is in the same context as the entity from the event
		const entityContext = await this.getContext(UMB_ENTITY_CONTEXT);
		if (!entityContext) return;
		const unique = entityContext.getUnique();
		const entityType = entityContext.getEntityType();

		if (unique === event.getUnique() && entityType === event.getEntityType()) {
			this._requestCollection();
		}
	};

	override destroy(): void {
		this.#actionEventContext?.removeEventListener(
			UmbRequestReloadStructureForEntityEvent.TYPE,
			this.#onReloadStructureRequest as unknown as EventListener,
		);

		this.#actionEventContext?.removeEventListener(
			UmbRequestReloadChildrenOfEntityEvent.TYPE,
			this.#onReloadChildrenRequest as unknown as EventListener,
		);

		super.destroy();
	}

	/**
	 * Returns the items in the collection.
	 * @returns {Array<CollectionItemType>} - The items in the collection.
	 */
	public getItems(): Array<CollectionItemType> {
		return this._items.getValue();
	}

	/**
	 * Returns the href for a specific collection item.
	 * Override this method in specialized collection contexts to provide item-specific hrefs.
	 * @param {CollectionItemType} _item  - The collection item to get the href for.
	 * @returns {Promise<string | undefined>} - Undefined. The collection item does not link to anything by default.
	 */
	public async requestItemHref(_item: CollectionItemType): Promise<string | undefined> {
		return undefined;
	}

	#configureSelection() {
		const selectionConfiguration = this.#config?.selectionConfiguration;
		this.selection.setConfig(selectionConfiguration);

		const selectOnly = selectionConfiguration?.selectOnly;
		this._selectOnly.setValue(selectOnly === true);

		// If there is an selection, and selectOnly is not explicitly set, set selectOnly in context when there is more than 0 items selected.
		this.observe(this.selection.selection, (selection) => {
			if (selectOnly === undefined) {
				this._selectOnly.setValue(selection.length > 0);
			}
		});
	}

	#configureViews() {
		const viewManagerConfig: UmbCollectionViewManagerConfig = { defaultViewAlias: this.#defaultViewAlias };
		const layouts = this.#config?.layouts;
		if (layouts && layouts.length > 0) {
			this.#viewLayouts.setValue(layouts);
			viewManagerConfig.viewsOverride = layouts;
		}
		this.view.setConfig(viewManagerConfig);
	}
}
