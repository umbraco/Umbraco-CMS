import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbPropertyValueData } from '@umbraco-cms/backoffice/property';
import type { UmbContentTypeModel, UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { UmbReadonlyVariantGuardManager } from '@umbraco-cms/backoffice/utils';

/**
 * The data supplier for a Element Property Dataset
 */
export interface UmbElementPropertyDataOwner<
	ContentModel extends { values: Array<UmbPropertyValueData> },
	ContentTypeModel extends UmbContentTypeModel = UmbContentTypeModel,
> extends UmbApi {
	unique: Observable<UmbEntityUnique | undefined>;
	getUnique(): UmbEntityUnique | undefined;
	getEntityType(): string;
	readonly structure: UmbContentTypeStructureManager<ContentTypeModel>;
	readonly values: Observable<ContentModel['values'] | undefined>;
	getValues(): ContentModel['values'] | undefined;

	isLoaded(): Promise<unknown> | undefined;
	readonly readonlyGuard: UmbReadonlyVariantGuardManager;

	// Same as from UmbVariantDatasetWorkspaceContext, could be refactored later [NL]
	propertyValueByAlias<ReturnValue = unknown>(
		alias: string,
		variantId?: UmbVariantId,
	): Promise<Observable<ReturnValue | undefined> | undefined>;
	getPropertyValue<ReturnValue = unknown>(alias: string, variantId?: UmbVariantId): ReturnValue | undefined;
	setPropertyValue(alias: string, value: unknown, variantId?: UmbVariantId): Promise<void>;

	initiatePropertyValueChange(): void;
	finishPropertyValueChange(): void;
}
