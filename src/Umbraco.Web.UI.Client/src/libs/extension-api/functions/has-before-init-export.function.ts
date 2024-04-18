import type { UmbEntryPointModule } from '../models/entry-point.interface.js';

/**
 * Validate if an ESModule has exported a function called `beforeInit`
 */
export function hasBeforeInitExport(obj: unknown): obj is Pick<UmbEntryPointModule, 'beforeInit'> {
	return obj !== null && typeof obj === 'object' && 'beforeInit' in obj;
}
