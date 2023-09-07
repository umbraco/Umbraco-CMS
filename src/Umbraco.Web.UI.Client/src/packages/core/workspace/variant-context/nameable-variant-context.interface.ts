import { UmbVariantContext } from "./variant-context.interface.js";

/**
 * A variant context with ability to set the name of it.
*/
export interface UmbNameableVariantContext extends UmbVariantContext {
	setName(name:string): void
}
