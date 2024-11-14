import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import {
	UmbEntityDetailWorkspaceContextBase,
	type UmbEntityDetailWorkspaceContextArgs,
	type UmbEntityDetailWorkspaceContextCreateArgs,
	type UmbRoutableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbContentTypeWorkspaceContext } from './content-type-workspace-context.interface';
import type { UmbContentTypeCompositionModel, UmbContentTypeDetailModel, UmbContentTypeSortModel } from '../types';
import { UmbValidationContext } from '@umbraco-cms/backoffice/validation';
import type { Observable } from 'rxjs';
import { UmbContentTypeStructureManager } from '../structure/index.js';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { jsonStringComparison } from '@umbraco-cms/backoffice/observable-api';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbContentTypeWorkspaceContextArgs extends UmbEntityDetailWorkspaceContextArgs {}

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

		this.addValidationContext(new UmbValidationContext(this));

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
	}

	override async createScaffold(args: UmbEntityDetailWorkspaceContextCreateArgs<DetailModelType>) {
		this.resetState();
		this.setParent(args.parent);

		const request = this.structure.createScaffold();
		this._getDataPromise = request;
		let { data } = await request;
		if (!data) return undefined;

		this.setUnique(data.unique);

		if (this.modalContext) {
			data = { ...data, ...this.modalContext.data.preset };
		}

		this.setIsNew(true);
		this._data.setPersisted(data);

		return data;
	}

	override async load(unique: string) {
		this.setUnique(unique);
		this.resetState();
		this._getDataPromise = this.structure.loadType(unique);
		const response = await this._getDataPromise;
		const data = response.data;

		if (data) {
			this._data.setPersisted(data);
			this.setIsNew(false);
		}

		return response;
	}

	override async _create(currentData: DetailModelType, parent: UmbEntityModel) {
		try {
			await this.structure.create(parent?.unique);

			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadChildrenOfEntityEvent({
				entityType: parent.entityType,
				unique: parent.unique,
			});
			eventContext.dispatchEvent(event);

			this.setIsNew(false);
		} catch (error) {}
	}

	override async _update() {
		try {
			await this.structure.save();

			const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadStructureForEntityEvent({
				unique: this.getUnique()!,
				entityType: this.getEntityType(),
			});

			actionEventContext.dispatchEvent(event);
		} catch (error) {}
	}

	public getName(): string | undefined {
		return this.structure.getOwnerContentType()?.name;
	}

	public setName(name: string) {
		this.structure.updateOwnerContentType({ name });
	}

	public getAlias(): string | undefined {
		return this.structure.getOwnerContentType()?.alias;
	}

	public setAlias(alias: string) {
		this.structure.updateOwnerContentType({ alias });
	}

	public getDescription(): string | undefined {
		return this.structure.getOwnerContentType()?.description;
	}

	public setDescription(description: string) {
		this.structure.updateOwnerContentType({ description });
	}

	public getCompositions(): Array<UmbContentTypeCompositionModel> | undefined {
		return this.structure.getOwnerContentType()?.compositions;
	}

	public setCompositions(compositions: Array<UmbContentTypeCompositionModel>) {
		this.structure.updateOwnerContentType({ compositions });
	}

	// TODO: manage setting icon color alias?
	public setIcon(icon: string) {
		this.structure.updateOwnerContentType({ icon });
	}

	public override getData() {
		return this.structure.getOwnerContentType();
	}

	protected override _getHasUnpersistedChanges(): boolean {
		const currentData = this.structure.getOwnerContentType();
		const persistedData = this._data.getPersisted();
		debugger;
		return jsonStringComparison(persistedData, currentData) === false;
	}

	public override destroy(): void {
		this.structure.destroy();
		super.destroy();
	}
}
