import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbVariantModel } from '@umbraco-cms/backoffice/variant';
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
}
