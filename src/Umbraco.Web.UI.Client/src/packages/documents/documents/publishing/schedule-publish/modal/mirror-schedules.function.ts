import type { UmbDocumentScheduleSelectionModel } from './document-schedule-modal.token.js';

/**
 * Returns a copy of the selection in which every variant other than the source carries the source variant's
 * publish and unpublish dates. The input is not modified, so the variants' own schedules are preserved.
 * @param {Array<UmbDocumentScheduleSelectionModel>} selection - the selected variants and their schedules.
 * @param {string} sourceUnique - the variant to copy the dates from.
 * @returns {Array<UmbDocumentScheduleSelectionModel>} The selection with the source variant's dates applied to every variant.
 */
export function mirrorSchedule(
	selection: Array<UmbDocumentScheduleSelectionModel>,
	sourceUnique: string,
): Array<UmbDocumentScheduleSelectionModel> {
	const source = selection.find((v) => v.unique === sourceUnique);
	const publishTime = source?.schedule?.publishTime ?? null;
	const unpublishTime = source?.schedule?.unpublishTime ?? null;

	return selection.map((variant) =>
		variant.unique === sourceUnique ? variant : { unique: variant.unique, schedule: { publishTime, unpublishTime } },
	);
}
