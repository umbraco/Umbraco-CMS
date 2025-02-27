import type { UmbContentTypeCompositionModel, UmbContentTypeDetailModel, UmbContentTypeSortModel } from '../types.js';
import { UmbContentTypeStructureManager } from '../structure/index.js';
import type { UmbContentTypeWorkspaceContext } from './content-type-workspace-context.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import {
	UmbEntityDetailWorkspaceContextBase,
	type UmbEntityDetailWorkspaceContextArgs,
	type UmbEntityDetailWorkspaceContextCreateArgs,
	type UmbRoutableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { jsonStringComparison, type Observable } from '@umbraco-cms/backoffice/observable-api';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbContentTypeWorkspaceContextArgs extends UmbEntityDetailWorkspaceContextArgs {}

const LOADING_STATE_UNIQUE = 'umbLoadingContentTypeDetail';

export abstract class UmbContentTypeWorkspaceContextBase<
		DetailModelType extends UmbContentTypeDetailModel = UmbContentTypeDetailModel,
		DetailRepositoryType extends UmbDetailRepository<DetailModelType> = UmbDetailRepository<DetailModelType>,
	>
	extends UmbEntityDetailWorkspaceContextBase<DetailModelType, DetailRepositoryType>
	implements UmbContentTypeWorkspaceContext<DetailModelType>, UmbRoutableWorkspaceContext
{
	public readonly IS_CONTENT_TYPE_WORKSPACE_CONTEXT = true;

	public readonly name: Observable<string | undefined>;
	public readonly alias: Observable<string | undefined>;
	public readonly description: Observable<string | undefined>;
	public readonly icon: Observable<string | undefined>;

	public readonly allowedAtRoot: Observable<boolean | undefined>;
	public readonly variesByCulture: Observable<boolean | undefined>;
	public readonly variesBySegment: Observable<boolean | undefined>;
	public readonly isElement: Observable<boolean | undefined>;
	public readonly allowedContentTypes: Observable<Array<UmbContentTypeSortModel> | undefined>;
	public readonly compositions: Observable<Array<UmbContentTypeCompositionModel> | undefined>;
	public readonly collection: Observable<UmbReferenceByUnique | null | undefined>;

	public readonly structure: UmbContentTypeStructureManager<DetailModelType>;

	constructor(host: UmbControllerHost, args: UmbContentTypeWorkspaceContextArgs) {
		super(host, args);

		this.structure = new UmbContentTypeStructureManager<DetailModelType>(this, args.detailRepositoryAlias);

		this.name = this.structure.ownerContentTypeObservablePart((data) => data?.name);
		this.alias = this.structure.ownerContentTypeObservablePart((data) => data?.alias);
		this.description = this.structure.ownerContentTypeObservablePart((data) => data?.description);
		this.icon = this.structure.ownerContentTypeObservablePart((data) => data?.icon);
		this.allowedAtRoot = this.structure.ownerContentTypeObservablePart((data) => data?.allowedAtRoot);
		this.variesByCulture = this.structure.ownerContentTypeObservablePart((data) => data?.variesByCulture);
		this.variesBySegment = this.structure.ownerContentTypeObservablePart((data) => data?.variesBySegment);
		this.isElement = this.structure.ownerContentTypeObservablePart((data) => data?.isElement);
		this.allowedContentTypes = this.structure.ownerContentTypeObservablePart((data) => data?.allowedContentTypes);
		this.compositions = this.structure.ownerContentTypeObservablePart((data) => data?.compositions);
		this.collection = this.structure.ownerContentTypeObservablePart((data) => data?.collection);

		// Keep current data in sync with the owner content type - This is used for the discard changes feature
		this.observe(this.structure.ownerContentType, (data) => this._data.setCurrent(data));
	}

	/**
	 * Creates a new scaffold
	 * @param { UmbEntityDetailWorkspaceContextCreateArgs<DetailModelType> } args The arguments for creating a new scaffold
	 * @returns { Promise<DetailModelType | undefined> } The new scaffold
	 */
	public override async createScaffold(
		args: UmbEntityDetailWorkspaceContextCreateArgs<DetailModelType>,
	): Promise<DetailModelType | undefined> {
		this.resetState();
		this.loading.addState({ unique: LOADING_STATE_UNIQUE, message: `Creating ${this.getEntityType()} scaffold` });
		this.setParent(args.parent);

		const request = this.structure.createScaffold(args.preset);
		this._getDataPromise = request;
		let { data } = await request;

		if (data) {
			data = await this._scaffoldProcessData(data);

			if (this.modalContext) {
				// Notice if the preset comes with values, they will overwrite the scaffolded values... [NL]
				data = { ...data, ...this.modalContext.data.preset };
			}

			this.setUnique(data.unique);
			this.setIsNew(true);
			this._data.setPersisted(data);
			this._data.setCurrent(data);
		}

		this.loading.removeState(LOADING_STATE_UNIQUE);

		return data;
	}

	/**
	 * Loads the data for the workspace
	 * @param { string } unique The unique identifier of the data to load
	 * @returns { Promise<DetailModelType> } The loaded data
	 */
	override async load(unique: string) {
		if (unique === this.getUnique() && this._getDataPromise) {
			return (await this._getDataPromise) as any;
		}

		this.resetState();
		this.setUnique(unique);
		this.loading.addState({ unique: LOADING_STATE_UNIQUE, message: `Loading ${this.getEntityType()} Details` });
		this._getDataPromise = this.structure.loadType(unique);
		const response = await this._getDataPromise;
		const data = response.data;

		if (data) {
			this._data.setPersisted(data);
			this._data.setCurrent(data);
			this.setIsNew(false);

			this.observe(
				response.asObservable(),
				(entity: any) => this.#onDetailStoreChange(entity),
				'umbContentTypeDetailStoreObserver',
			);
		}

		this.loading.removeState(LOADING_STATE_UNIQUE);
		return response;
	}

	#onDetailStoreChange(entity: DetailModelType | undefined) {
		if (!entity) {
			this._data.clear();
		}
	}

	/**
	 * Creates the Content Type
	 * @param { DetailModelType } currentData The current data
	 * @param { UmbEntityModel } parent The parent entity
	 * @memberof UmbContentTypeWorkspaceContextBase
	 */
	override async _create(currentData: DetailModelType, parent: UmbEntityModel) {
		try {
			await this.structure.create(parent?.unique);

			this._data.setPersisted(this.structure.getOwnerContentType());

			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadChildrenOfEntityEvent({
				entityType: parent.entityType,
				unique: parent.unique,
			});
			eventContext.dispatchEvent(event);

			this.setIsNew(false);
		} catch (error) {
			console.error(error);
		}
	}

	/**
	 * Updates the content type for the workspace
	 * @memberof UmbContentTypeWorkspaceContextBase
	 */
	override async _update() {
		try {
			await this.structure.save();

			this._data.setPersisted(this.structure.getOwnerContentType());

			const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadStructureForEntityEvent({
				unique: this.getUnique()!,
				entityType: this.getEntityType(),
			});

			actionEventContext.dispatchEvent(event);
		} catch (error) {
			console.error(error);
		}
	}

	/**
	 * Gets the name of the content type
	 * @returns { string | undefined } The name of the content type
	 */
	public getName(): string | undefined {
		return this.structure.getOwnerContentType()?.name;
	}

	/**
	 * Sets the name of the content type
	 * @param { string } name The name of the content type
	 */
	public setName(name: string) {
		this.structure.updateOwnerContentType({ name } as Partial<DetailModelType>);
	}

	/**
	 * Gets the alias of the content type
	 * @returns { string | undefined } The alias of the content type
	 */
	public getAlias(): string | undefined {
		return this.structure.getOwnerContentType()?.alias;
	}

	/**
	 * Sets the alias of the content type
	 * @param { string } alias The alias of the content type
	 */
	public setAlias(alias: string) {
		this.structure.updateOwnerContentType({ alias } as Partial<DetailModelType>);
	}

	/**
	 * Gets the description of the content type
	 * @returns { string | undefined } The description of the content type
	 */
	public getDescription(): string | undefined {
		return this.structure.getOwnerContentType()?.description;
	}

	/**
	 * Sets the description of the content type
	 * @param { string } description The description of the content type
	 */
	public setDescription(description: string) {
		this.structure.updateOwnerContentType({ description } as Partial<DetailModelType>);
	}

	/**
	 * Gets the compositions of the content type
	 * @returns { string | undefined } The icon of the content type
	 */
	public getCompositions(): Array<UmbContentTypeCompositionModel> | undefined {
		return this.structure.getOwnerContentType()?.compositions;
	}

	/**
	 * Sets the compositions of the content type
	 * @param { string } compositions The compositions of the content type
	 * @returns { void }
	 */
	public setCompositions(compositions: Array<UmbContentTypeCompositionModel>) {
		this.structure.updateOwnerContentType({ compositions } as Partial<DetailModelType>);
	}

	// TODO: manage setting icon color alias?
	public setIcon(icon: string) {
		this.structure.updateOwnerContentType({ icon } as Partial<DetailModelType>);
	}

	public override getData() {
		return this.structure.getOwnerContentType();
	}

	public override destroy(): void {
		this.structure.destroy();
		super.destroy();
	}
}
