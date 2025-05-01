import { UmbSubmittableWorkspaceContextBase } from '../submittable/index.js';
import { UmbEntityWorkspaceDataManager } from '../entity/entity-workspace-data-manager.js';
import type { UmbEntityDetailWorkspaceContextArgs, UmbEntityDetailWorkspaceContextCreateArgs } from './types.js';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityContext, type UmbEntityModel, type UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { UMB_DISCARD_CHANGES_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import {
	UmbEntityUpdatedEvent,
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry, type ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import { UmbDeprecation, UmbStateManager } from '@umbraco-cms/backoffice/utils';
import { UmbValidationContext } from '@umbraco-cms/backoffice/validation';
import { UmbId } from '@umbraco-cms/backoffice/id';

const LOADING_STATE_UNIQUE = 'umbLoadingEntityDetail';

export abstract class UmbEntityDetailWorkspaceContextBase<
	DetailModelType extends UmbEntityModel = UmbEntityModel,
	DetailRepositoryType extends UmbDetailRepository<DetailModelType> = UmbDetailRepository<DetailModelType>,
	CreateArgsType extends
		UmbEntityDetailWorkspaceContextCreateArgs<DetailModelType> = UmbEntityDetailWorkspaceContextCreateArgs<DetailModelType>,
> extends UmbSubmittableWorkspaceContextBase<DetailModelType> {
	// Just for context token safety:
	public readonly IS_ENTITY_DETAIL_WORKSPACE_CONTEXT = true;

	/**
	 * @description Data manager for the workspace.
	 * @protected
	 * @memberof UmbEntityWorkspaceContextBase
	 */
	protected readonly _data = new UmbEntityWorkspaceDataManager<DetailModelType>(this);

	#entityContext = new UmbEntityContext(this);
	public readonly entityType = this.#entityContext.entityType;
	public readonly unique = this.#entityContext.unique;

	public readonly data = this._data.current;
	public readonly persistedData = this._data.persisted;
	public readonly loading = new UmbStateManager(this);

	protected _getDataPromise?: Promise<any>;
	protected _detailRepository?: DetailRepositoryType;

	#eventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	#parent = new UmbObjectState<{ entityType: string; unique: UmbEntityUnique } | undefined>(undefined);
	public readonly parentUnique = this.#parent.asObservablePart((parent) => (parent ? parent.unique : undefined));
	public readonly parentEntityType = this.#parent.asObservablePart((parent) =>
		parent ? parent.entityType : undefined,
	);

	/**
	 * The base validation context for the workspace. This ensures that at least one validation context is always present.
	 * @example You can manually validate all properties on the context:
	 * ```ts
	 * try {
	 *   await this.validationContext.validate();
	 * } catch (error) {
	 *   console.error(error);
	 * }
	 * ```
	 * @example You can set the data path on the context to map to a specific part of the server validation:
	 * ```ts
	 * this.validationContext.setDataPath('path.to.data');
	 * ```
	 */
	protected validationContext = new UmbValidationContext(this);

	#initResolver?: () => void;
	#initialized = false;

	#init = new Promise<void>((resolve) => {
		if (this.#initialized) {
			resolve();
		} else {
			this.#initResolver = resolve;
		}
	});

	constructor(host: UmbControllerHost, args: UmbEntityDetailWorkspaceContextArgs) {
		super(host, args.workspaceAlias);
		this.addValidationContext(this.validationContext);
		this.#entityContext.setEntityType(args.entityType);
		window.addEventListener('willchangestate', this.#onWillNavigate);
		this.#observeRepository(args.detailRepositoryAlias);

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
			this.#eventContext = context;

			this.#eventContext.removeEventListener(
				UmbEntityUpdatedEvent.TYPE,
				this.#onEntityUpdatedEvent as unknown as EventListener,
			);
			this.#eventContext.addEventListener(
				UmbEntityUpdatedEvent.TYPE,
				this.#onEntityUpdatedEvent as unknown as EventListener,
			);
		});
	}

	/**
	 * Get the entity type
	 * @returns { string } The entity type
	 */
	getEntityType(): string {
		const entityType = this.#entityContext.getEntityType();
		if (!entityType) throw new Error('Entity type is not set');
		return entityType;
	}

	/**
	 * Get the current data
	 * @returns { DetailModelType | undefined } The entity context
	 */
	getData(): DetailModelType | undefined {
		return this._data.getCurrent();
	}

	/**
	 * Get the persisted data
	 * @returns { DetailModelType | undefined } The persisted data
	 */
	public getPersistedData(): DetailModelType | undefined {
		return this._data.getPersisted();
	}

	/**
	 * Get the unique
	 * @returns { string | undefined } The unique identifier
	 */
	getUnique(): UmbEntityUnique | undefined {
		return this.#entityContext.getUnique();
	}

	setUnique(unique: string) {
		this.#entityContext.setUnique(unique);
	}

	/**
	 * Get the parent
	 * @returns { UmbEntityModel | undefined } The parent entity
	 */
	getParent(): UmbEntityModel | undefined {
		return this.#parent.getValue();
	}

	setParent(parent: UmbEntityModel) {
		this.#parent.setValue(parent);
	}

	/**
	 * Get the parent unique
	 * @returns { string | undefined } The parent unique identifier
	 */
	getParentUnique(): UmbEntityUnique | undefined {
		return this.#parent.getValue()?.unique;
	}

	getParentEntityType() {
		return this.#parent.getValue()?.entityType;
	}

	async load(unique: string) {
		if (unique === this.getUnique() && this._getDataPromise) {
			return (await this._getDataPromise) as GetDataType;
		}
		this.resetState();
		this.setIsNew(false);
		this.#entityContext.setUnique(unique);
		this.loading.addState({ unique: LOADING_STATE_UNIQUE, message: `Loading ${this.getEntityType()} Details` });
		await this.#init;
		this._getDataPromise = this._detailRepository!.requestByUnique(unique);
		type GetDataType = Awaited<ReturnType<UmbDetailRepository<DetailModelType>['requestByUnique']>>;
		const response = (await this._getDataPromise) as GetDataType;
		const data = response.data;

		if (data) {
			this._data.setPersisted(data);
			this._data.setCurrent(data);

			this.observe(
				response.asObservable(),
				(entity) => this.#onDetailStoreChange(entity),
				'umbEntityDetailTypeStoreObserver',
			);
		}

		this.loading.removeState(LOADING_STATE_UNIQUE);
		return response;
	}

	/**
	 * Reload the workspace data
	 * @returns { Promise<void> } The promise of the reload
	 */
	public async reload(): Promise<void> {
		const unique = this.getUnique();
		if (!unique) throw new Error('Unique is not set');
		const { data } = await this._detailRepository!.requestByUnique(unique);

		if (data) {
			this._data.setPersisted(data);
			this._data.setCurrent(data);
		}
	}

	/**
	 * Method to check if the workspace data is loaded.
	 * @returns { Promise<any> | undefined } true if the workspace data is loaded.
	 * @memberof UmbEntityWorkspaceContextBase
	 */
	public isLoaded(): Promise<any> | undefined {
		return this._getDataPromise;
	}

	/**
	 * Create a data scaffold
	 * @param {CreateArgsType} args The arguments to create the scaffold.
	 * @param {UmbEntityModel} args.parent The parent entity.
	 * @param {UmbEntityUnique} args.parent.unique The unique identifier of the parent entity.
	 * @param {string} args.parent.entityType The entity type of the parent entity.
	 * @param {Partial<DetailModelType>} args.preset The preset data.
	 * @returns { Promise<any> | undefined } The data of the scaffold.
	 */
	public async createScaffold(args: CreateArgsType) {
		this.resetState();
		this.loading.addState({ unique: LOADING_STATE_UNIQUE, message: `Creating ${this.getEntityType()} scaffold` });
		await this.#init;
		this.setParent(args.parent);

		const request = this._detailRepository!.createScaffold(args.preset);
		this._getDataPromise = request;
		let { data } = await request;

		if (data) {
			data = await this._scaffoldProcessData(data);

			if (this.modalContext) {
				// Notice if the preset comes with values, they will overwrite the scaffolded values... [NL]
				data = { ...data, ...this.modalContext.data.preset };
			}

			this.setIsNew(true);
			this.#entityContext.setUnique(data.unique);
			this._data.setPersisted(data);
			this._data.setCurrent(data);
		}

		this.loading.removeState(LOADING_STATE_UNIQUE);

		return data;
	}

	protected async _scaffoldProcessData(data: DetailModelType): Promise<DetailModelType> {
		return data;
	}

	async submit() {
		await this.#init;
		const currentData = this.getData();

		if (!currentData) {
			throw new Error('Data is not set');
		}

		if (currentData.unique === undefined) {
			throw new Error('Unique is not set');
		}

		if (this.getIsNew()) {
			const parent = this.#parent.getValue();
			if (parent?.unique === undefined) throw new Error('Parent unique is missing');
			if (!parent.entityType) throw new Error('Parent entity type is missing');
			await this._create(currentData, parent);
		} else {
			await this._update(currentData);
		}
	}

	/**
	 * Deletes the entity.
	 * @param unique The unique identifier of the entity to delete.
	 */
	async delete(unique: string) {
		await this.#init;
		await this._detailRepository!.delete(unique);
	}

	/**
	 * Check if the workspace is about to navigate away.
	 * @protected
	 * @param {string} newUrl The new url that the workspace is navigating to.
	 * @returns { boolean} true if the workspace is navigating away.
	 * @memberof UmbEntityWorkspaceContextBase
	 */
	protected _checkWillNavigateAway(newUrl: string): boolean {
		return !newUrl.includes(this.routes.getActiveLocalPath());
	}

	protected async _create(currentData: DetailModelType, parent: UmbEntityModel) {
		if (!this._detailRepository) throw new Error('Detail repository is not set');

		const { error, data } = await this._detailRepository.create(currentData, parent.unique);
		if (error || !data) {
			throw error?.message ?? 'Repository did not return data after create.';
		}

		this.#entityContext.setUnique(data.unique);
		this._data.setPersisted(data);
		this._data.setCurrent(data);

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const event = new UmbRequestReloadChildrenOfEntityEvent({
			entityType: parent.entityType,
			unique: parent.unique,
		});
		eventContext.dispatchEvent(event);
		this.setIsNew(false);
	}

	protected async _update(currentData: DetailModelType) {
		const { error, data } = await this._detailRepository!.save(currentData);
		if (error || !data) {
			throw error?.message ?? 'Repository did not return data after create.';
		}

		this._data.setPersisted(data);
		this._data.setCurrent(data);

		const unique = this.getUnique()!;
		const entityType = this.getEntityType();

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const event = new UmbRequestReloadStructureForEntityEvent({ unique, entityType });

		eventContext.dispatchEvent(event);

		const updatedEvent = new UmbEntityUpdatedEvent({
			unique,
			entityType,
			eventUnique: this._workspaceEventUnique,
		});

		eventContext.dispatchEvent(updatedEvent);
	}

	#allowNavigateAway = false;

	#onWillNavigate = async (e: CustomEvent) => {
		const newUrl = e.detail.url;

		if (this.#allowNavigateAway) {
			return true;
		}

		if (this._checkWillNavigateAway(newUrl) && this.getHasUnpersistedChanges()) {
			/* Since ours modals are async while events are synchronous, we need to prevent the default behavior of the event, even if the modal hasn’t been resolved yet.
			Once the modal is resolved (the user accepted to discard the changes and navigate away from the route), we will push a new history state.
			This push will make the "willchangestate" event happen again and due to this somewhat "backward" behavior,
			we set an "allowNavigateAway"-flag to prevent the "discard-changes" functionality from running in a loop.*/
			e.preventDefault();
			const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
			const modal = modalManager.open(this, UMB_DISCARD_CHANGES_MODAL);

			try {
				// navigate to the new url when discarding changes
				await modal.onSubmit();
				this.#allowNavigateAway = true;
				history.pushState({}, '', e.detail.url);
				return true;
			} catch {
				return false;
			}
		}

		return true;
	};

	/**
	 * Check if there are unpersisted changes.
	 * @returns { boolean } true if there are unpersisted changes.
	 */
	public getHasUnpersistedChanges(): boolean {
		return this._data.getHasUnpersistedChanges();
	}
	// @deprecated use getHasUnpersistedChanges instead, will be removed in v17.0
	protected _getHasUnpersistedChanges(): boolean {
		new UmbDeprecation({
			removeInVersion: '17',
			deprecated: '_getHasUnpersistedChanges',
			solution: 'use public getHasUnpersistedChanges instead.',
		}).warn();
		return this.getHasUnpersistedChanges();
	}

	override resetState() {
		super.resetState();
		this.loading.clear();
		this._data.clear();
		this.#allowNavigateAway = false;
		this._getDataPromise = undefined;
	}

	#checkIfInitialized() {
		if (this._detailRepository) {
			this.#initialized = true;
			this.#initResolver?.();
		}
	}

	#observeRepository(repositoryAlias: string) {
		if (!repositoryAlias) throw new Error('Entity Workspace must have a repository alias.');

		new UmbExtensionApiInitializer<ManifestRepository<DetailRepositoryType>>(
			this,
			umbExtensionsRegistry,
			repositoryAlias,
			[],
			(permitted, ctrl) => {
				this._detailRepository = permitted ? ctrl.api : undefined;
				this.#checkIfInitialized();
			},
		);
	}

	#onDetailStoreChange(entity: DetailModelType | undefined) {
		if (!entity) {
			this._data.clear();
		}
	}

	// Discriminator to identify events from this workspace context
	protected readonly _workspaceEventUnique = UmbId.new();

	#onEntityUpdatedEvent = (event: UmbEntityUpdatedEvent) => {
		const eventEntityUnique = event.getUnique();
		const eventEntityType = event.getEntityType();
		const eventDiscriminator = event.getEventUnique();

		// Ignore events for other entities
		if (eventEntityType !== this.getEntityType()) return;
		if (eventEntityUnique !== this.getUnique()) return;

		// Ignore events from this workspace so we don't reload the data twice. Ex saving this workspace
		if (eventDiscriminator === this._workspaceEventUnique) return;

		this.reload();
	};

	public override destroy(): void {
		window.removeEventListener('willchangestate', this.#onWillNavigate);
		this.#eventContext?.removeEventListener(
			UmbEntityUpdatedEvent.TYPE,
			this.#onEntityUpdatedEvent as unknown as EventListener,
		);
		this._detailRepository?.destroy();
		this.#entityContext.destroy();
		this._getDataPromise = undefined;
		super.destroy();
	}
}
