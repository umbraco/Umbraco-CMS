import type { UmbSorterResolvePlacementArgs } from './sorter.controller.js';

/**
 * This function is used to resolve the placement of an item in a simple grid layout.
 * @param args
 * @returns { null | true }
 */
export function UmbSorterResolvePlacementAsGrid(args: UmbSorterResolvePlacementArgs<unknown>) {
	// If we are part of the same Sorter model
	if (args.itemIndex !== null && args.relatedIndex !== null) {
		// and the pointer is within the related rect
		if (args.relatedRect.left < args.pointerX && args.relatedRect.right > args.pointerX) {
			// Then we control the placeAfter property, making the active-drag-element allow to be placed at a spot already when just hovering that spot. (This only works when items have the same size)
			return {
				placeAfter: args.itemIndex < args.relatedIndex,
			};
		}
	}
	return false;
}
