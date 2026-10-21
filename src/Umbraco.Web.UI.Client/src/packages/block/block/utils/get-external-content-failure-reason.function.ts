import type { UmbBlockExternalContentFailureReason } from '../types.js';
import { UmbApiError } from '@umbraco-cms/backoffice/resources';

/**
 * Maps the error of a failed external content request to the reason the content is unavailable.
 * @param {unknown} error The error returned by the request.
 * @returns {UmbBlockExternalContentFailureReason | undefined} The reason, or `undefined` when the failure says nothing about the content's availability (for example a server error).
 */
export function getExternalContentFailureReason(error: unknown): UmbBlockExternalContentFailureReason | undefined {
	if (!error || !UmbApiError.isUmbApiError(error)) return undefined;
	switch (error.status) {
		case 404:
			return 'deleted';
		case 403:
			return 'noAccess';
		default:
			return undefined;
	}
}
