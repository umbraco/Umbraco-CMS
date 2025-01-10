import type { UmbBlockGridLayoutModel } from '../types.js';

/**
 *
 * @param {number} target - The target number
 * @param {Array<number>} map - The map to search in
 * @param {number} max - The max value to return if no match is found
 * @returns {number | undefined} - The closest number in the map to the target
 */
export function closestColumnSpanOption(target: number, map: Array<number>, max: number): number | undefined {
	if (map.length > 0) {
		const result = map.reduce((a, b) => {
			if (a > max) {
				return b;
			}
			const aDiff = Math.abs(a - target);
			const bDiff = Math.abs(b - target);

			if (aDiff === bDiff) {
				return a < b ? a : b;
			} else {
				return bDiff < aDiff ? b : a;
			}
		});
		if (result) {
			return result;
		}
	}
	return;
}

/**
 *
 * @param {UmbBlockGridLayoutModel} entry - The entry to iterate over
 * @param {(entry:UmbBlockGridLayoutModel) => void } callback - The callback to call for each entry
 */
export function forEachBlockLayoutEntryOf(
	entry: UmbBlockGridLayoutModel,
	callback: (entry: UmbBlockGridLayoutModel, areaKey: string) => void,
): void {
	if (entry.areas) {
		entry.areas.forEach((area) => {
			area.items.forEach((item) => {
				forEachBlockLayoutEntryOf(item, callback);
				callback(item, area.key);
			});
		});
	}
}
