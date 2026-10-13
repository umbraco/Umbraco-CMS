import { UmbApiError, UmbCancelError, UmbError } from './umb-error.js';
import { expect } from '@open-wc/testing';

const guards = {
	isUmbError: UmbError.isUmbError,
	isUmbCancelError: UmbCancelError.isUmbCancelError,
	isUmbApiError: UmbApiError.isUmbApiError,
};

describe('UmbError type guards', () => {
	for (const [name, guard] of Object.entries(guards)) {
		describe(name, () => {
			it('returns false for any value that is not an error, including null and undefined', () => {
				for (const value of [undefined, null, 'boom', 0, false, {}]) {
					expect(guard(value), String(value)).to.be.false;
				}
			});
		});
	}

	it('recognises an error by its class or by its name', () => {
		expect(UmbError.isUmbError(new UmbError('boom'))).to.be.true;
		expect(UmbCancelError.isUmbCancelError(new UmbCancelError('cancelled'))).to.be.true;
		expect(UmbApiError.isUmbApiError({ name: 'UmbApiError' })).to.be.true;
	});
});
