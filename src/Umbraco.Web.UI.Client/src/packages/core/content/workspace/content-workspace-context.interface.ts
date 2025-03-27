import type { UmbContentDetailModel } from '../types.js';
import type { UmbElementPropertyDataOwner } from '../property-dataset-context/index.js';
import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbVariantId, UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';
import type {
	UmbPropertyStructureWorkspaceContext,
	UmbRoutableWorkspaceContext,
	UmbVariantDatasetWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';

export interface UmbContentWorkspaceContext<
	ContentModel extends UmbContentDetailModel = UmbContentDetailModel,
	ContentTypeModel extends UmbContentTypeModel = UmbContentTypeModel,
	VariantModelType extends UmbEntityVariantModel = UmbEntityVariantModel,
> extends UmbElementPropertyDataOwner<ContentModel, ContentTypeModel>,
		UmbRoutableWorkspaceContext,
		UmbVariantDatasetWorkspaceContext<VariantModelType>,
		UmbPropertyStructureWorkspaceContext<ContentTypeModel> {
	readonly IS_CONTENT_WORKSPACE_CONTEXT: true;
	getData(): ContentModel | undefined;
	isLoaded(): Promise<unknown> | undefined;
	variantById(variantId: UmbVariantId): Observable<VariantModelType | undefined>;
	varies: Observable<boolean>;
	variesByCulture: Observable<boolean>;
	variesBySegment: Observable<boolean>;
}
