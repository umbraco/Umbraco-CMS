import type { UmbEntryPointModule } from './entry-point.interface.js';

/**
 * Validate if an ESModule exports a known init function called 'onInit'
 */
export function hasInitExport(obj: unknown): obj is Pick<UmbEntryPointModule, 'onInit'> {
	return obj !== null && typeof obj === 'object' && 'onInit' in obj;
}
