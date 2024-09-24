import type { UmbContentDetailModel } from '@umbraco-cms/backoffice/content';
import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbReadOnlyVariantStateManager } from '@umbraco-cms/backoffice/utils';
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
> extends UmbRoutableWorkspaceContext,
		UmbVariantDatasetWorkspaceContext<VariantModelType>,
		UmbPropertyStructureWorkspaceContext<ContentTypeModel> {
	readonly IS_CONTENT_WORKSPACE_CONTEXT: true;
	readonly readOnlyState: UmbReadOnlyVariantStateManager;

	// Data:
	getData(): ContentModel | undefined;

	isLoaded(): Promise<unknown> | undefined;
	variantById(variantId: UmbVariantId): Observable<VariantModelType | undefined>;

	initiatePropertyValueChange(): void;
	finishPropertyValueChange(): void;
}
