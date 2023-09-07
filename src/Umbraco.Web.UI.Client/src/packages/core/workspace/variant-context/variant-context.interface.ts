import type { UmbVariantId } from "../../variant/variant-id.class.js";
import type {  Observable } from "@umbraco-cms/backoffice/external/rxjs";

/**
 * A variant context, represents a set of properties.
 * This can take form as many, so to list a few:
 * - A specific variant of content
 * - Content that does not vary
 * - A block.
 * - A DataType configuration.
 *
 * The base type of this holds a Name and some Properties.
 * Some might be enriches with Variant Info, like culture and segment.
 * Others might have saved publishing status.
 * Also setting the name is an additional feature.
 */
export interface UmbVariantContext {

	getType(): string;
	getUnique(): string | undefined;
	//getUniqueName(): string;
	getVariantId: (() => UmbVariantId);

	getName(): string | undefined;
	readonly name: Observable<string | undefined>;

	destroy(): void;

	// Property methods:
	propertyVariantId?: ((propertyAlias: string) => Promise<Observable<UmbVariantId | undefined>>);
	propertyValueByAlias<ReturnType = unknown>(propertyAlias: string): Promise<Observable<ReturnType | undefined>>;
	setPropertyValue(propertyAlias: string, value: unknown): Promise<void>;

}
