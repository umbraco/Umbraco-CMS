import type { UmbOffsetPaginationRequestModel } from '../types.js';
import type { UmbTargetPaginationRequestModel } from './types.js';

/**
 * Checks if the provided paging object is an target pagination request.
 * @param {object } paging - The paging object to check.
 */
export function isTargetPaginationRequest(
	paging: UmbOffsetPaginationRequestModel | UmbTargetPaginationRequestModel,
): paging is UmbTargetPaginationRequestModel {
	return (paging as UmbTargetPaginationRequestModel).target !== undefined;
}
