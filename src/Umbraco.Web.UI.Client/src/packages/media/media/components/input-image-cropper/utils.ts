import type { UmbFocalPointModel } from '../../types.js';

/**
 * @description Helper function to check if the focal point is centered.
 * @param focalPoint UmbFocalPointModel
 */
export function isCentered(focalPoint: UmbFocalPointModel): boolean {
	return focalPoint.left === 0.5 && focalPoint.top === 0.5;
}
