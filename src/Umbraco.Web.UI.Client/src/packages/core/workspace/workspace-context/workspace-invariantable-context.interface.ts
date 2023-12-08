import { UmbVariantId } from '../../variant/variant-id.class.js';
import { UmbPropertyDatasetContext } from '../property-dataset/property-dataset-context.interface.js';
import type { UmbSaveableWorkspaceContextInterface } from './saveable-workspace-context.interface.js';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbInvariantableWorkspaceContextInterface<T = unknown>
	extends UmbSaveableWorkspaceContextInterface<T> {
	// Name:
	name: Observable<string | undefined>;
	getName(): string | undefined;
	setName(name: string): void;

	// Property:
	propertyValueByAlias<ReturnType = unknown>(alias: string): Promise<Observable<ReturnType | undefined>>;
	getPropertyValue<ReturnType = unknown>(alias: string): ReturnType;
	setPropertyValue(alias: string, value: unknown): Promise<void>;

	createVariantContext(host: UmbControllerHost, variantId?: UmbVariantId): UmbPropertyDatasetContext;
}
