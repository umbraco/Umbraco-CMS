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

export class UmbDefaultCollectionContext<
	CollectionItemType extends { entityType: string; unique: string } = any,
	FilterModelType extends UmbCollectionFilterModel = UmbCollectionFilterModel,
>
	extends UmbContextBase
	implements UmbCollectionContext, UmbApi {
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
	public readonly view = new UmbCollectionViewManager(this);
	public readonly bulkAction = new UmbCollectionBulkActionManager(this);

	#defaultViewAlias: string;
	#defaultFilter: Partial<FilterModelType>;

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

		// The parent entity context is used to get the parent entity for the collection items
		// All items in the collection are children of the current entity context
		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			const currentEntityUnique = context?.getUnique();
			const currentEntityType = context?.getEntityType();

			const parent: UmbEntityModel | undefined =
				currentEntityUnique && currentEntityType
					? {
						unique: currentEntityUnique,
						entityType: currentEntityType,
					}
					: undefined;

			this.#parentEntityContext?.setParent(parent);
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

		if (this.#config.pageSize) {
			this.pagination.setPageSize(this.#config.pageSize);
		}

		const filterValue = this._filter.getValue() as FilterModelType;

		this._filter.setValue({
			...this.#defaultFilter,
			...this.#config,
			...filterValue,
			skip: filterValue.skip ?? 0,
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

		this._configured = true;
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
			this._totalItems.setValue(data.total);
			this.pagination.setTotalItems(data.total);
		}

		this._loading.setValue(false);
	}

	/**
	 * Sets the filter for the collection and refreshes the collection.
	 * @param {Partial<FilterModelType>} filter
	 * @memberof UmbCollectionContext
	 */
	public setFilter(filter: Partial<FilterModelType>) {
		this._filter.setValue({ ...this._filter.getValue(), ...filter });
		this.loadCollection();
	}

	public updateFilter(filter: Partial<FilterModelType>) {
		this._filter.setValue({ ...this._filter.getValue(), ...filter });
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
	 * Sets the manifest for the collection.
	 * @param {ManifestCollection} manifest - The manifest for the collection.
	 * @memberof UmbCollectionContext
	 * @deprecated Use set the `.manifest` property instead.
	 */
	public setManifest(manifest: ManifestCollection | undefined) {
		if (this._manifest === manifest) return;
		this._manifest = manifest;

		if (!this._manifest) return;
		this.#observeRepository(this._manifest.meta.repositoryAlias);
	}

	/**
	 * Returns the manifest for the collection.
	 * @returns {ManifestCollection} - The manifest for the collection.
	 * @memberof UmbCollectionContext
	 * @deprecated Use the `.manifest` property instead.
	 */
	public getManifest(): ManifestCollection | undefined {
		new UmbDeprecation({
			removeInVersion: '18.0.0',
			deprecated: 'getManifest',
			solution: 'Use .manifest property instead',
		}).warn();
		return this._manifest;
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
}
