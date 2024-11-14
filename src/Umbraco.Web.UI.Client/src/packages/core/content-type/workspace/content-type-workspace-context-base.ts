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

	public getName(): string | undefined {
		return this.structure.getOwnerContentType()?.name;
	}

	public setName(name: string) {
		this.structure.updateOwnerContentType({ name });
	}

	public setAlias(alias: string) {
		this.structure.updateOwnerContentType({ alias });
	}

	public setDescription(description: string) {
		this.structure.updateOwnerContentType({ description });
	}

	// TODO: manage setting icon color alias?
	public setIcon(icon: string) {
		this.structure.updateOwnerContentType({ icon });
	}

	public override getData() {
		return this.structure.getOwnerContentType();
	}

	public override getUnique() {
		return this.getData()?.unique;
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
