import { mirrorSchedule } from './mirror-schedules.function.js';
import type { UmbDocumentScheduleSelectionModel } from './document-schedule-modal.token.js';
import { expect } from '@open-wc/testing';

const PUBLISH = '2099-01-01T10:00:00.000Z';
const UNPUBLISH = '2099-02-01T10:00:00.000Z';

describe('mirrorSchedule', () => {
	it('copies both dates onto a variant that has none', () => {
		const values: Array<UmbDocumentScheduleSelectionModel> = [
			{ unique: 'en-US', schedule: { publishTime: PUBLISH, unpublishTime: UNPUBLISH } },
			{ unique: 'da-DK', schedule: null },
		];

		mirrorSchedule(values, ['en-US', 'da-DK'], 'en-US');

		expect(values.find((v) => v.unique === 'da-DK')?.schedule).to.eql({
			publishTime: PUBLISH,
			unpublishTime: UNPUBLISH,
		});
	});

	it('overwrites a variant that already has its own custom dates', () => {
		const values: Array<UmbDocumentScheduleSelectionModel> = [
			{ unique: 'en-US', schedule: { publishTime: PUBLISH, unpublishTime: UNPUBLISH } },
			{ unique: 'da-DK', schedule: { publishTime: '2099-06-01T12:00:00.000Z', unpublishTime: null } },
		];

		mirrorSchedule(values, ['en-US', 'da-DK'], 'en-US');

		expect(values.find((v) => v.unique === 'da-DK')?.schedule).to.eql({
			publishTime: PUBLISH,
			unpublishTime: UNPUBLISH,
		});
	});

	it('leaves the source variant untouched', () => {
		const values: Array<UmbDocumentScheduleSelectionModel> = [
			{ unique: 'en-US', schedule: { publishTime: PUBLISH, unpublishTime: null } },
			{ unique: 'da-DK', schedule: { publishTime: '2099-06-01T12:00:00.000Z', unpublishTime: null } },
		];

		mirrorSchedule(values, ['en-US', 'da-DK'], 'en-US');

		expect(values.find((v) => v.unique === 'en-US')?.schedule).to.eql({
			publishTime: PUBLISH,
			unpublishTime: null,
		});
	});

	it('adds an entry for a target that is not yet present', () => {
		const values: Array<UmbDocumentScheduleSelectionModel> = [
			{ unique: 'en-US', schedule: { publishTime: PUBLISH, unpublishTime: null } },
		];

		mirrorSchedule(values, ['en-US', 'de-DE'], 'en-US');

		expect(values.find((v) => v.unique === 'de-DE')?.schedule).to.eql({
			publishTime: PUBLISH,
			unpublishTime: null,
		});
	});

	it('mirrors empty dates when the source has none', () => {
		const values: Array<UmbDocumentScheduleSelectionModel> = [
			{ unique: 'en-US', schedule: null },
			{ unique: 'da-DK', schedule: { publishTime: PUBLISH, unpublishTime: UNPUBLISH } },
		];

		mirrorSchedule(values, ['en-US', 'da-DK'], 'en-US');

		expect(values.find((v) => v.unique === 'da-DK')?.schedule).to.eql({
			publishTime: null,
			unpublishTime: null,
		});
	});
});
