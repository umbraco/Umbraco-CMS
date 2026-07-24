import { getEndOfDayInLocalTime, getStartOfDayInLocalTime } from './utils.js';
import { expect } from '@open-wc/testing';

describe('log viewer date range utils', () => {
	describe('getStartOfDayInLocalTime', () => {
		it('returns an absolute UTC timestamp', () => {
			const result = getStartOfDayInLocalTime('2023-08-22');
			// A bare date is no longer sent; the server needs an instant to resolve the correct log files (#14710).
			expect(result).to.not.equal('2023-08-22');
			expect(result.endsWith('Z')).to.be.true;
		});

		it('represents the very start of the given day in local time', () => {
			const result = new Date(getStartOfDayInLocalTime('2023-08-22'));
			expect(result.getFullYear()).to.equal(2023);
			expect(result.getMonth()).to.equal(7); // August (0-indexed)
			expect(result.getDate()).to.equal(22);
			expect(result.getHours()).to.equal(0);
			expect(result.getMinutes()).to.equal(0);
			expect(result.getSeconds()).to.equal(0);
			expect(result.getMilliseconds()).to.equal(0);
		});

		it('returns the input unchanged when it cannot be parsed', () => {
			expect(getStartOfDayInLocalTime('')).to.equal('');
			expect(getStartOfDayInLocalTime('not-a-date')).to.equal('not-a-date');
			expect(getStartOfDayInLocalTime('2023-08-')).to.equal('2023-08-'); // partial: day would become 0
			expect(getStartOfDayInLocalTime('2023-13-40')).to.equal('2023-13-40'); // out of range: would roll over
			expect(getStartOfDayInLocalTime('2023-02-29')).to.equal('2023-02-29'); // not a leap year
		});
	});

	describe('getEndOfDayInLocalTime', () => {
		it('returns an absolute UTC timestamp', () => {
			const result = getEndOfDayInLocalTime('2023-08-22');
			expect(result).to.not.equal('2023-08-22');
			expect(result.endsWith('Z')).to.be.true;
		});

		it('represents the very end of the given day in local time', () => {
			const result = new Date(getEndOfDayInLocalTime('2023-08-22'));
			expect(result.getFullYear()).to.equal(2023);
			expect(result.getMonth()).to.equal(7); // August (0-indexed)
			expect(result.getDate()).to.equal(22);
			expect(result.getHours()).to.equal(23);
			expect(result.getMinutes()).to.equal(59);
			expect(result.getSeconds()).to.equal(59);
			expect(result.getMilliseconds()).to.equal(999);
		});

		it('returns the input unchanged when it cannot be parsed', () => {
			expect(getEndOfDayInLocalTime('')).to.equal('');
			expect(getEndOfDayInLocalTime('not-a-date')).to.equal('not-a-date');
			expect(getEndOfDayInLocalTime('2023-08-')).to.equal('2023-08-'); // partial: day would become 0
			expect(getEndOfDayInLocalTime('2023-13-40')).to.equal('2023-13-40'); // out of range: would roll over
			expect(getEndOfDayInLocalTime('2023-02-29')).to.equal('2023-02-29'); // not a leap year
		});
	});
});
