import { UmbSubmittableWorkspaceContextBase } from '../submittable/index.js';
import { UmbEntityWorkspaceDataManager } from '../entity/entity-workspace-data-manager.js';
import { UMB_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UMB_DISCARD_CHANGES_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry, type ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';

export interface UmbEntityWorkspaceContextArgs {
	entityType: string;
	workspaceAlias: string;
	detailRepositoryAlias: string;
}

export interface UmbEntityDetailWorkspaceContextCreateArgs {
	parent: UmbEntityModel;
}

export abstract class UmbEntityDetailWorkspaceContextBase<
	EntityModelType extends UmbEntityModel,
	DetailRepositoryType extends UmbDetailRepository<EntityModelType> = UmbDetailRepository<EntityModelType>,
> extends UmbSubmittableWorkspaceContextBase<EntityModelType> {
	/**
	 * @description Data manager for the workspace.
	 * @protected
	 * @memberof UmbEntityWorkspaceContextBase
	 */
	protected readonly _data = new UmbEntityWorkspaceDataManager<EntityModelType>(this);

	protected _getDataPromise?: Promise<any>;

	protected _detailRepository?: DetailRepositoryType;

	#entityType: string;

	#parent = new UmbObjectState<{ entityType: string; unique: string | null } | undefined>(undefined);
	readonly parentUnique = this.#parent.asObservablePart((parent) => (parent ? parent.unique : undefined));
	readonly parentEntityType = this.#parent.asObservablePart((parent) => (parent ? parent.entityType : undefined));

	#activePathSegment = '';

	#initResolver?: () => void;
	#initialized = false;

	#init = new Promise<void>((resolve) => {
		if (this.#initialized) {
			resolve();
		} else {
			this.#initResolver = resolve;
		}
	});

	constructor(host: UmbControllerHost, args: UmbEntityWorkspaceContextArgs) {
		super(host, args.workspaceAlias);
		this.#entityType = args.entityType;
		window.addEventListener('willchangestate', this.#onWillNavigate);
		this.#observeRepository(args.detailRepositoryAlias);
	}

	getEntityType() {
		return this.#entityType;
	}

	getData() {
		return this._data.getCurrentData();
	}

	getUnique() {
		return this._data.getCurrentData()?.unique;
	}

	async load(unique: string) {
		await this.#init;
		this.resetState();
		this._getDataPromise = this._detailRepository!.requestByUnique(unique);
		type GetDataType = Awaited<ReturnType<UmbDetailRepository<EntityModelType>['requestByUnique']>>;
		const response = (await this._getDataPromise) as GetDataType;
		const data = response.data;

		if (data) {
			this.setIsNew(false);
			this._data.setPersistedData(data);
			this._data.setCurrentData(data);
		}

		return response;
	}

	public isLoaded() {
		return this._getDataPromise;
	}

	async submit() {
		await this.#init;
		const currentData = this._data.getCurrentData();

		if (!currentData) {
			throw new Error('Data is not set');
		}
		if (!currentData.unique) {
			throw new Error('Unique is not set');
		}

		if (this.getIsNew()) {
			const parent = this.#parent.getValue();
			if (!parent) throw new Error('Parent is not set');
			const { error, data } = await this._detailRepository!.create(currentData, parent.unique);
			if (error || !data) {
				throw error?.message ?? 'Repository did not return data after create.';
			}

			this._data.setPersistedData(data);

			// TODO: this might not be the right place to alert the tree, but it works for now
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadChildrenOfEntityEvent({
				entityType: parent.entityType,
				unique: parent.unique,
			});
			eventContext.dispatchEvent(event);
			this.setIsNew(false);
		} else {
			const { error, data } = await this._detailRepository!.save(currentData);
			if (error || !data) {
				throw error?.message ?? 'Repository did not return data after create.';
			}

			this._data.setPersistedData(data);

			const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadStructureForEntityEvent({
				unique: this.getUnique()!,
				entityType: this.getEntityType(),
			});

			actionEventContext.dispatchEvent(event);
		}
	}

	async create(args: UmbEntityDetailWorkspaceContextCreateArgs) {
		await this.#init;
		this.resetState();
		this.#parent.setValue(args.parent);
		const request = this._detailRepository!.createScaffold();
		this._getDataPromise = request;
		let { data } = await request;
		if (!data) return undefined;

		if (this.modalContext) {
			data = { ...data, ...this.modalContext.data.preset };
		}
		this.setIsNew(true);
		this._data.setPersistedData(data);
		this._data.setCurrentData(data);
		return data;
	}

	async delete(unique: string) {
		await this.#init;
		await this._detailRepository!.delete(unique);
	}

	protected _setActivePathSegment(segment: string) {
		this.#activePathSegment = segment;
	}

	protected _getActivePathSegment() {
		return this.#activePathSegment;
	}

	/**
	 * @description method to check if the workspace is about to navigate away.
	 * @protected
	 * @param {string} newUrl
	 * @returns {*}
	 * @memberof UmbEntityWorkspaceContextBase
	 */
	protected _checkWillNavigateAway(newUrl: string) {
		const workspaceBasePath = UMB_WORKSPACE_PATH_PATTERN.generateLocal({ entityType: this.getEntityType() });
		const currentWorkspacePathIdentifier = '/' + workspaceBasePath + '/' + this._getActivePathSegment();
		return !newUrl.includes(currentWorkspacePathIdentifier);
	}

	#onWillNavigate = async (e: CustomEvent) => {
		const newUrl = e.detail.url;

		if (this._checkWillNavigateAway(newUrl) && this._data.hasUnpersistedChanges()) {
			e.preventDefault();
			const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
			const modal = modalManager.open(this, UMB_DISCARD_CHANGES_MODAL);

			try {
				// navigate to the new url when discarding changes
				await modal.onSubmit();
				// Reset the current data so we don't end in a endless loop of asking to discard changes.
				this._data.resetCurrentData();
				history.pushState({}, '', e.detail.url);
				return true;
			} catch {
				return false;
			}
		}

		return true;
	};

	override resetState() {
		super.resetState();
		this._data.clearData();
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
			[this._host],
			(permitted, ctrl) => {
				this._detailRepository = permitted ? ctrl.api : undefined;
				this.#checkIfInitialized();
			},
		);
	}

	public override destroy(): void {
		this._data.destroy();
		this._detailRepository?.destroy();
		window.removeEventListener('willchangestate', this.#onWillNavigate);
		super.destroy();
	}
}
