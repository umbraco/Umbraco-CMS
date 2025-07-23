import type { UmbProblemDetails } from '../types.js';

/**
 * @deprecated Use `isProblemDetailsLike` from `@umbraco-cms/backoffice/resources` instead.
 */
export function isProblemDetails(obj: unknown): obj is UmbProblemDetails {
	return (
		typeof obj === 'object' && obj !== null && 'type' in obj && 'title' in obj && 'status' in obj && 'detail' in obj
	);
}
