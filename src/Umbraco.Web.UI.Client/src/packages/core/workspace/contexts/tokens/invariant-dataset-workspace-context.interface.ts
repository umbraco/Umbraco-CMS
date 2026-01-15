import type { UmbNamableWorkspaceContext } from '../../namable/namable-workspace-context.interface.js';
import type { UmbSubmittableWorkspaceContext } from './submittable-workspace-context.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbPropertyDatasetContext, UmbPropertyValueData } from '@umbraco-cms/backoffice/property';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export interface UmbInvariantDatasetWorkspaceContext
	extends UmbSubmittableWorkspaceContext,
		UmbNamableWorkspaceContext {
	readonly values: Observable<Array<UmbPropertyValueData> | undefined>;
	getValues(): Promise<Array<UmbPropertyValueData> | undefined>;

	// Property:
	propertyValueByAlias<ReturnType = unknown>(alias: string): Promise<Observable<ReturnType | undefined>>;
	getPropertyValue<ReturnType = unknown>(alias: string): ReturnType;
	setPropertyValue(alias: string, value: unknown): Promise<void>;

	createPropertyDatasetContext(host: UmbControllerHost, variantId?: UmbVariantId): UmbPropertyDatasetContext;
}
