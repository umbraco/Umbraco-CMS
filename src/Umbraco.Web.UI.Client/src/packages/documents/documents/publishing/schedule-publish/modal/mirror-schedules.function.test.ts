import { mirrorSchedule } from './mirror-schedules.function.js';
import type { UmbDocumentScheduleSelectionModel } from './document-schedule-modal.token.js';
import { expect } from '@open-wc/testing';

const PUBLISH = '2099-01-01T10:00:00.000Z';
const UNPUBLISH = '2099-02-01T10:00:00.000Z';
const OWN_PUBLISH = '2099-06-01T12:00:00.000Z';

describe('mirrorSchedule', () => {
	it('copies both dates onto a variant that has none', () => {
		const selection: Array<UmbDocumentScheduleSelectionModel> = [
			{ unique: 'en-US', schedule: { publishTime: PUBLISH, unpublishTime: UNPUBLISH } },
			{ unique: 'da-DK', schedule: null },
		];

		const result = mirrorSchedule(selection, 'en-US');

		expect(result.find((v) => v.unique === 'da-DK')?.schedule).to.eql({
			publishTime: PUBLISH,
			unpublishTime: UNPUBLISH,
		});
	});

	it('replaces the dates of a variant that has its own schedule', () => {
		const selection: Array<UmbDocumentScheduleSelectionModel> = [
			{ unique: 'en-US', schedule: { publishTime: PUBLISH, unpublishTime: UNPUBLISH } },
			{ unique: 'da-DK', schedule: { publishTime: OWN_PUBLISH, unpublishTime: null } },
		];

		const result = mirrorSchedule(selection, 'en-US');

		expect(result.find((v) => v.unique === 'da-DK')?.schedule).to.eql({
			publishTime: PUBLISH,
			unpublishTime: UNPUBLISH,
		});
	});

	it('does not modify the input selection', () => {
		const selection: Array<UmbDocumentScheduleSelectionModel> = [
			{ unique: 'en-US', schedule: { publishTime: PUBLISH, unpublishTime: UNPUBLISH } },
			{ unique: 'da-DK', schedule: { publishTime: OWN_PUBLISH, unpublishTime: null } },
		];

		mirrorSchedule(selection, 'en-US');

		expect(selection.find((v) => v.unique === 'da-DK')?.schedule).to.eql({
			publishTime: OWN_PUBLISH,
			unpublishTime: null,
		});
	});

	it('leaves the source variant unchanged', () => {
		const source: UmbDocumentScheduleSelectionModel = {
			unique: 'en-US',
			schedule: { publishTime: PUBLISH, unpublishTime: null },
		};
		const selection: Array<UmbDocumentScheduleSelectionModel> = [
			source,
			{ unique: 'da-DK', schedule: { publishTime: OWN_PUBLISH, unpublishTime: null } },
		];

		const result = mirrorSchedule(selection, 'en-US');

		expect(result.find((v) => v.unique === 'en-US')).to.equal(source);
	});

	it('applies empty dates when the source has none', () => {
		const selection: Array<UmbDocumentScheduleSelectionModel> = [
			{ unique: 'en-US', schedule: null },
			{ unique: 'da-DK', schedule: { publishTime: PUBLISH, unpublishTime: UNPUBLISH } },
		];

		const result = mirrorSchedule(selection, 'en-US');

		expect(result.find((v) => v.unique === 'da-DK')?.schedule).to.eql({
			publishTime: null,
			unpublishTime: null,
		});
	});
});
