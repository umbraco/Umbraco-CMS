import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbReadOnlyVariantStateManager } from '@umbraco-cms/backoffice/utils';
import type { UmbVariantId, UmbVariantModel } from '@umbraco-cms/backoffice/variant';
import type {
	UmbPropertyStructureWorkspaceContext,
	UmbRoutableWorkspaceContext,
	UmbVariantDatasetWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';

export interface UmbContentWorkspaceContext<
	ContentTypeModel extends UmbContentTypeModel = UmbContentTypeModel,
	VariantModelType extends UmbVariantModel = UmbVariantModel,
> extends UmbRoutableWorkspaceContext,
		UmbVariantDatasetWorkspaceContext<VariantModelType>,
		UmbPropertyStructureWorkspaceContext<ContentTypeModel> {
	readonly IS_CONTENT_WORKSPACE_CONTEXT: true;
	readonly readOnlyState: UmbReadOnlyVariantStateManager;

	isLoaded(): Promise<unknown> | undefined;
	variantById(variantId: UmbVariantId): Observable<VariantModelType | undefined>;

	initiatePropertyValueChange(): void;
	finishPropertyValueChange(): void;
}
