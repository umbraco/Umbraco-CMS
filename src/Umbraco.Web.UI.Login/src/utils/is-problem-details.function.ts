import type { UmbProblemDetails } from '../types.js';

export function isProblemDetails(obj: unknown): obj is UmbProblemDetails {
	return (
		typeof obj === 'object' && obj !== null && 'type' in obj && 'title' in obj && 'status' in obj && 'detail' in obj
	);
}
