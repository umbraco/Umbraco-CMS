import type { UmbEntrypointModule } from './umb-lifecycle.interface';

/**
 * Validate if an ESModule exports a known init function called 'onInit'
 */
export function hasInitExport(obj: unknown): obj is Pick<UmbEntrypointModule, 'onInit'> {
	return obj !== null && typeof obj === 'object' && 'onInit' in obj;
}
