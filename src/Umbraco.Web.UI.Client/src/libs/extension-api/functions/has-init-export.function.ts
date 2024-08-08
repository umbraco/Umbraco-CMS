import type { UmbEntryPointModule } from '../models/entry-point.interface.js';

/**
 * Validate if an ESModule export has a function called 'onInit'
 * @param obj
 */
export function hasInitExport(obj: unknown): obj is Pick<UmbEntryPointModule, 'onInit'> {
	return obj !== null && typeof obj === 'object' && 'onInit' in obj;
}
