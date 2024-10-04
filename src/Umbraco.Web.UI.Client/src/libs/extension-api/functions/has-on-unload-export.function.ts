import type { UmbEntryPointModule } from '../models/entry-point.interface.js';

/**
 * Validate if an ESModule has exported a function called `onUnload`
 * @param obj
 */
export function hasOnUnloadExport(obj: unknown): obj is Pick<UmbEntryPointModule, 'onUnload'> {
	return obj !== null && typeof obj === 'object' && 'onUnload' in obj;
}
