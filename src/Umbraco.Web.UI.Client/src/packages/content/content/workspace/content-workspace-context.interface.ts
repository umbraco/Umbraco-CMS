import type { UmbContentDetailModel } from '../types.js';
import type { UmbElementPropertyDataOwner } from '../property-dataset-context/index.js';
import type { UmbContentTypeModel, UmbPropertyStructureWorkspaceContext } from '@umbraco-cms/backoffice/content-type';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbVariantId, UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';
import type { UmbRoutableWorkspaceContext, UmbVariantDatasetWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import type { UmbVariantPropertyGuardManager } from '@umbraco-cms/backoffice/property';

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
	varies: Observable<boolean | undefined>;
	variesByCulture: Observable<boolean | undefined>;
	variesBySegment: Observable<boolean | undefined>;

	readonly propertyViewGuard: UmbVariantPropertyGuardManager;
	readonly propertyWriteGuard: UmbVariantPropertyGuardManager;

	//initiatePropertyValueChange(): void;
	//finishPropertyValueChange(): void;
}
