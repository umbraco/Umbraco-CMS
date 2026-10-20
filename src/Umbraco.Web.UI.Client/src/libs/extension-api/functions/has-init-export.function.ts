import type { UmbEntryPointModule } from '../models/entry-point.interface.js';

/**
 * Validate if an ESModule export has a function called 'onInit'
 * @param {unknown} obj The object to check.
 * @returns {boolean} True if the object has an 'onInit' function.
 */
export function hasInitExport(obj: unknown): obj is Required<Pick<UmbEntryPointModule, 'onInit'>> {
	return obj !== null && typeof obj === 'object' && typeof (obj as { onInit?: unknown }).onInit === 'function';
}
