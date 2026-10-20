import type { UmbOffsetPaginationRequestModel, UmbTargetPaginationRequestModel } from '../types.js';

/**
 * Checks if the provided paging object is an target pagination request.
 * @param {object } paging - The paging object to check.
 * @returns {boolean} True if the paging object is an offset pagination request.
 */
export function isOffsetPaginationRequest(
	paging: UmbOffsetPaginationRequestModel | UmbTargetPaginationRequestModel,
): paging is UmbOffsetPaginationRequestModel {
	return (
		(paging as UmbOffsetPaginationRequestModel).skip !== undefined ||
		(paging as UmbOffsetPaginationRequestModel).take !== undefined
	);
}
