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
export async function forEachBlockLayoutEntryOf(
	entry: UmbBlockGridLayoutModel,
	callback: (entry: UmbBlockGridLayoutModel, parentUnique: string, areaKey: string) => PromiseLike<void>,
): Promise<void> {
	if (entry.areas) {
		const parentUnique = entry.contentKey;
		await Promise.all(
			entry.areas.map(async (area) => {
				const areaKey = area.key;
				await Promise.all(
					area.items.map(async (item) => {
						await callback(item, parentUnique, areaKey);
						await forEachBlockLayoutEntryOf(item, callback);
					}),
				);
			}),
		);
	}
}
