import { apiErrorWasNotified } from './api-error-was-notified.function.js';
import { UmbApiError, UmbCancelError, UmbError } from '../umb-error.js';
import { expect } from '@open-wc/testing';
import type { UmbProblemDetails } from '../types.js';

function apiError(problemDetails: Partial<UmbProblemDetails>): UmbApiError {
	const details = {
		type: 'Error',
		title: 'Something went wrong',
		status: 400,
		...problemDetails,
	} as UmbProblemDetails;

	return new UmbApiError(details.title, details.status, null, details);
}

describe('apiErrorWasNotified', () => {
	it('reports that a problem-details error was notified', () => {
		expect(apiErrorWasNotified(apiError({ status: 400, title: 'Parent not published' }))).to.be.true;
	});

	it('reports that a conflict was notified', () => {
		expect(apiErrorWasNotified(apiError({ status: 409 }))).to.be.true;
	});

	// 401/403/404 are suppressed by tryExecute because the UI handles them itself, so a caller adding its own
	// fallback message must still show it.
	[401, 403, 404].forEach((status) => {
		it(`reports that a ${status} was not notified`, () => {
			expect(apiErrorWasNotified(apiError({ status }))).to.be.false;
		});
	});

	it('reports that a cancelled-by-notification operation was not notified', () => {
		// The server already messaged the user via the Umb-Notifications header.
		expect(apiErrorWasNotified(apiError({ status: 400, operationStatus: 'CancelledByNotification' }))).to.be.false;
	});

	it('reports that another operation status was notified', () => {
		expect(apiErrorWasNotified(apiError({ status: 400, operationStatus: 'PathNotPublished' }))).to.be.true;
	});

	it('reports that a cancel error was not notified', () => {
		expect(apiErrorWasNotified(new UmbCancelError('cancelled'))).to.be.false;
	});

	it('reports that a non-api error was notified, since it falls through to the generic message', () => {
		expect(apiErrorWasNotified(new UmbError('boom'))).to.be.true;
		expect(apiErrorWasNotified(new Error('boom'))).to.be.true;
	});

	// The UmbError type guards read `.name` off the argument, so these would throw without the guard. They also
	// never came from tryExecute, so nothing was notified for them.
	it('reports that a missing or primitive error was not notified', () => {
		expect(apiErrorWasNotified(undefined)).to.be.false;
		expect(apiErrorWasNotified(null)).to.be.false;
		expect(apiErrorWasNotified('boom')).to.be.false;
	});
});
