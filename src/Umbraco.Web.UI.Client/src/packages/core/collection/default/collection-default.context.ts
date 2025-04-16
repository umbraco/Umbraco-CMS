import { UmbCollectionViewManager } from '../collection-view.manager.js';
import type { UmbCollectionViewManagerConfig } from '../collection-view.manager.js';
import type {
	UmbCollectionColumnConfiguration,
	UmbCollectionConfiguration,
	UmbCollectionContext,
	UmbCollectionLayoutConfiguration,
} from '../types.js';
import type { UmbCollectionFilterModel } from '../collection-filter-model.interface.js';
import type { UmbCollectionRepository } from '../repository/collection-repository.interface.js';
import type { ManifestCollection } from '../extensions/types.js';
import { UMB_COLLECTION_CONTEXT } from './collection-default.context-token.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbArrayState, UmbBasicState, UmbNumberState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbSelectionManager, UmbPaginationManager, UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import type { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { UmbModalRouteRegistrationController, type UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';

const LOCAL_STORAGE_KEY = 'umb-collection-view';

export class UmbDefaultCollectionContext<
		CollectionItemType extends { entityType: string; unique: string } = any,
		FilterModelType extends UmbCollectionFilterModel = UmbCollectionFilterModel,
	>
	extends UmbContextBase<UmbDefaultCollectionContext>
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

	#workspacePathBuilder = new UmbBasicState<UmbModalRouteBuilder | undefined>(undefined);
	public readonly workspacePathBuilder = this.#workspacePathBuilder.asObservable();

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

	protected _init = new Promise<void>((resolve) => {
		if (this.#initialized) {
			resolve();
		} else {
			this.#initResolver = resolve;
		}
	});

	#actionEventContext: UmbActionEventContext | undefined;

	constructor(host: UmbControllerHost, defaultViewAlias: string, defaultFilter: Partial<FilterModelType> = {}) {
		super(host, UMB_COLLECTION_CONTEXT);

		this.#defaultViewAlias = defaultViewAlias;
		this.#defaultFilter = defaultFilter;

		this.pagination.addEventListener(UmbChangeEvent.TYPE, this.#onPageChange);
		this.#listenToEntityEvents();
	}

	setupView(viewElement: UmbControllerHost) {
		new UmbModalRouteRegistrationController(viewElement, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('entity/:entityType')
			.onSetup((params) => {
				return { data: { entityType: params.entityType, preset: {} } };
			})
			.onReject(() => {
				// TODO: Maybe this can be removed?
				this.requestCollection();
			})
			.onSubmit(() => {
				// TODO: Maybe this can be removed?
				this.requestCollection();
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

		this.selection.setMultiple(true);

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

	/**
	 * Requests the collection from the repository.
	 * @returns {*}
	 * @memberof UmbCollectionContext
	 */
	public async requestCollection() {
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

	#onReloadStructureRequest = (event: UmbRequestReloadStructureForEntityEvent) => {
		const items = this._items.getValue();
		const hasItem = items.some((item) => item.unique === event.getUnique());
		if (hasItem) {
			this.requestCollection();
		}
	};

	#onReloadChildrenRequest = async (event: UmbRequestReloadChildrenOfEntityEvent) => {
		// check if the collection is in the same context as the entity from the event
		const entityContext = await this.getContext(UMB_ENTITY_CONTEXT);
		if (!entityContext) return;
		const unique = entityContext.getUnique();
		const entityType = entityContext.getEntityType();

		if (unique === event.getUnique() && entityType === event.getEntityType()) {
			this.requestCollection();
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
	 * @param {ManifestCollection} manifest
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
	 * @returns {ManifestCollection}
	 * @memberof UmbCollectionContext
	 * @deprecated Use the `.manifest` property instead.
	 */
	public getManifest() {
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
	public getItems() {
		return this._items.getValue();
	}
}
