import type { UmbVariantId } from '../../variant/variant-id.class.js';
import type { UmbPropertyValueData } from '../types.js';
import type { UmbContext } from '@umbraco-cms/backoffice/class-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

/**
 * A property dataset context, represents the data of a set of properties.
 * This can take form as many, so to list a few:
 * - A specific variant of content.
 * - Content that does not vary.
 * - A block.
 * - A DataType configuration.
 * - A property editor that hosts a set of properties.
 *
 * The base type of this holds a Name and some Properties.
 * Some might be enriches with Variant Info, like culture and segment.
 * Others might have saved publishing status.
 * Also setting the name is an additional feature.
 */
export interface UmbPropertyDatasetContext extends UmbContext {
	getEntityType(): string;
	getUnique(): UmbEntityUnique | undefined;
	getVariantId: () => UmbVariantId;

	getName(): string | undefined;
	readonly name: Observable<string | undefined>;

	getReadOnly(): boolean;
	readonly readOnly: Observable<boolean>;

	// Should it be possible to get the properties as a list of property aliases?
	readonly properties: Observable<Array<UmbPropertyValueData> | undefined>;
	getProperties(): Promise<Array<UmbPropertyValueData> | undefined>;
	//setProperties(properties: Array<UmbPropertyValueData>): void;

	// Property methods:
	propertyVariantId?: (propertyAlias: string) => Promise<Observable<UmbVariantId | undefined>>;
	propertyValueByAlias<ReturnType = unknown>(
		propertyAlias: string,
	): Promise<Observable<ReturnType | undefined> | undefined>;
	setPropertyValue(propertyAlias: string, value: unknown): void;
}
