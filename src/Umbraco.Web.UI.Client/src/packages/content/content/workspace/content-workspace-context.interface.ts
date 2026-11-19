import type { UmbContentDetailModel } from '../types.js';
import type { UmbElementPropertyDataOwner } from '../property-dataset-context/index.js';
import type { UmbContentTypeModel, UmbPropertyStructureWorkspaceContext } from '@umbraco-cms/backoffice/content-type';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbVariantId, UmbEntityVariantModel, UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type {
	UmbEntityDetailIncomingDataHookMeta,
	UmbEntityDetailInitHookMeta,
	UmbRoutableWorkspaceContext,
	UmbVariantDatasetWorkspaceContext,
	UmbVariantNameWriteGuardManager,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbVariantPropertyGuardManager } from '@umbraco-cms/backoffice/property';
import type { UmbParallelHookController } from '@umbraco-cms/backoffice/hook-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbContentWorkspaceContext<
	ContentModel extends UmbContentDetailModel = UmbContentDetailModel,
	ContentTypeModel extends UmbContentTypeModel = UmbContentTypeModel,
	VariantModelType extends UmbEntityVariantModel = UmbEntityVariantModel,
	VariantOptionModelType extends UmbEntityVariantOptionModel = UmbEntityVariantOptionModel<VariantModelType>,
>
	extends
		UmbElementPropertyDataOwner<ContentModel, ContentTypeModel>,
		UmbRoutableWorkspaceContext,
		UmbVariantDatasetWorkspaceContext<VariantModelType, VariantOptionModelType>,
		UmbPropertyStructureWorkspaceContext<ContentTypeModel> {
	readonly IS_CONTENT_WORKSPACE_CONTEXT: true;

	readonly initHook: UmbParallelHookController<UmbEntityDetailInitHookMeta>;
	readonly incomingDataHook: UmbParallelHookController<UmbContentDetailIncomingDataHookMeta>;

	getData(): ContentModel | undefined;
	isLoaded(): Promise<unknown> | undefined;
	variantById(variantId: UmbVariantId): Observable<VariantModelType | undefined>;

	readonly propertyViewGuard: UmbVariantPropertyGuardManager;
	readonly propertyWriteGuard: UmbVariantPropertyGuardManager;
	readonly nameWriteGuard: UmbVariantNameWriteGuardManager;
	isWritableVariant(variantId: UmbVariantId): Observable<boolean>;
	getIsWritableVariant(variantId: UmbVariantId): boolean;
}

export interface UmbContentDetailIncomingDataHookMeta extends UmbEntityDetailIncomingDataHookMeta {
	contentType: UmbReferenceByUnique | undefined;
}
