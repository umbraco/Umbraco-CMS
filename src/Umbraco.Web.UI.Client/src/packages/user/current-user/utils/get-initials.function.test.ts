import { expect } from '@open-wc/testing';
import { getInitials } from './get-initials.function.js';

describe('getInitials', () => {
	it('should extract first and last initials from a full name', () => {
		expect(getInitials('Andreas Lykke Borg')).to.equal('AB');
	});

	it('should extract first and last initials from a two-word name', () => {
		expect(getInitials('Rick Doesburg')).to.equal('RD');
	});

	it('should return uppercase initials', () => {
		expect(getInitials('john doe')).to.equal('JD');
	});

	it('should handle names with extra whitespace', () => {
		expect(getInitials('Jane  Mary  Smith')).to.equal('JS');
	});

	it('should return single initial for single-word names', () => {
		expect(getInitials('Madonna')).to.equal('M');
	});

	it('should return empty string for null name', () => {
		expect(getInitials(null as any)).to.equal('');
	});

	it('should return empty string for undefined name', () => {
		expect(getInitials(undefined as any)).to.equal('');
	});

	it('should return empty string for empty string', () => {
		expect(getInitials('')).to.equal('');
	});

	it('should return empty string for whitespace-only string', () => {
		expect(getInitials('   ')).to.equal('');
	});

	it('should return single initial for single digit numeric converted to string', () => {
		expect(getInitials('1')).to.equal('1');
	});

	it('should return single initial for multiple numbers converted to string', () => {
		expect(getInitials('1 2 3')).to.equal('13');
	});
});

