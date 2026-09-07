import type { UmbEntryPointModule } from '../models/entry-point.interface.js';

/**
 * Validate if an ESModule has exported a function called `onUnload`
 * @param {unknown} obj The object to check.
 * @returns {boolean} True if the object has an 'onUnload' function.
 */
export function hasOnUnloadExport(obj: unknown): obj is Pick<UmbEntryPointModule, 'onUnload'> {
	return obj !== null && typeof obj === 'object' && 'onUnload' in obj;
}
