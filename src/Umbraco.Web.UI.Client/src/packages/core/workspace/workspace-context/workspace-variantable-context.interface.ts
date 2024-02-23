import type { UmbWorkspaceSplitViewManager } from '../workspace-split-view-manager.class.js';
import type { UmbPropertyDatasetContext } from '../../property/property-dataset/property-dataset-context.interface.js';
import type { UmbSaveableWorkspaceContextInterface } from './saveable-workspace-context.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbVariantId, UmbVariantModel } from '@umbraco-cms/backoffice/variant';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbVariantableWorkspaceContextInterface extends UmbSaveableWorkspaceContextInterface {
	// Name:
	getName(variantId?: UmbVariantId): string | undefined;
	setName(name: string, variantId?: UmbVariantId): void;

	// Variant:
	variants: Observable<Array<UmbVariantModel>>;
	variantsWithLanguages: Observable<Array<UmbVariantModel>>;
	splitView: UmbWorkspaceSplitViewManager;
	getVariant(variantId: UmbVariantId): UmbVariantModel | undefined;

	// Property:
	// This one is async cause it needs to structure to provide this data:
	propertyValueByAlias<ReturnValue = unknown>(
		alias: string,
		variantId?: UmbVariantId,
	): Promise<Observable<ReturnValue | undefined>>;
	getPropertyValue<ReturnValue = unknown>(alias: string, variantId?: UmbVariantId): ReturnValue | undefined;
	setPropertyValue(alias: string, value: unknown, variantId?: UmbVariantId): Promise<void>;
	//propertyDataByAlias(alias: string, variantId?: UmbVariantId): Observable<ValueModelBaseModel | undefined>;

	createPropertyDatasetContext(host: UmbControllerHost, variantId?: UmbVariantId): UmbPropertyDatasetContext;
}
