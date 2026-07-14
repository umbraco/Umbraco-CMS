import type { UmbDocumentScheduleSelectionModel } from './document-schedule-modal.token.js';

/**
 * Overwrites the schedule of every target variant with a copy of the source variant's publish and
 * unpublish dates, so all targets share the source's schedule. The source variant itself is never
 * modified. Mutates `values` in place, adding an entry for any target that is not yet present.
 * @param {Array<UmbDocumentScheduleSelectionModel>} values - the current per-variant schedule selection.
 * @param {Array<string>} targetUniques - the variant uniques to mirror the source onto.
 * @param {string} sourceUnique - the variant to copy the dates from.
 */
export function mirrorSchedule(
	values: Array<UmbDocumentScheduleSelectionModel>,
	targetUniques: Array<string>,
	sourceUnique: string,
): void {
	const source = values.find((v) => v.unique === sourceUnique);
	const publishTime = source?.schedule?.publishTime ?? null;
	const unpublishTime = source?.schedule?.unpublishTime ?? null;

	for (const unique of targetUniques) {
		if (unique === sourceUnique) continue;

		let variant = values.find((v) => v.unique === unique);
		if (!variant) {
			variant = { unique, schedule: null };
			values.push(variant);
		}

		variant.schedule = { publishTime, unpublishTime };
	}
}
