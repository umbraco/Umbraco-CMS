import type { UmbWorkspaceSplitViewManager } from '../../controllers/workspace-split-view-manager.controller.js';
import type { UmbPropertyDatasetContext } from '../../../property/property-dataset/property-dataset-context.interface.js';
import type { UmbSubmittableWorkspaceContext } from './submittable-workspace-context.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbVariantId, UmbEntityVariantModel, UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbValidationController } from '@umbraco-cms/backoffice/validation';

export interface UmbVariantDatasetWorkspaceContext<VariantType extends UmbEntityVariantModel = UmbEntityVariantModel>
	extends UmbSubmittableWorkspaceContext {
	// Name:
	getName(variantId?: UmbVariantId): string | undefined;
	setName(name: string, variantId?: UmbVariantId): void;
	name(variantId?: UmbVariantId): Observable<string>;

	// Variant:
	variants: Observable<Array<VariantType>>;
	variantOptions: Observable<Array<UmbEntityVariantOptionModel<VariantType>>>;
	splitView: UmbWorkspaceSplitViewManager;
	getVariant(variantId: UmbVariantId): VariantType | undefined;

	// Property:
	// This one is async cause it needs to structure to provide this data: [NL]
	propertyValueByAlias<ReturnValue = unknown>(
		alias: string,
		variantId?: UmbVariantId,
	): Promise<Observable<ReturnValue | undefined> | undefined>;
	getPropertyValue<ReturnValue = unknown>(alias: string, variantId?: UmbVariantId): ReturnValue | undefined;
	setPropertyValue(alias: string, value: unknown, variantId?: UmbVariantId): Promise<void>;

	createPropertyDatasetContext(host: UmbControllerHost, variantId?: UmbVariantId): UmbPropertyDatasetContext;
	getVariantValidationContext(variantId: UmbVariantId): UmbValidationController | undefined;
}
