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

	it('should ignore parenthetical suffixes when generating initials', () => {
		expect(getInitials('Henrik Christensen (HC)')).to.equal('HC');
		expect(getInitials('John Doe (Admin)')).to.equal('JD');
	});

	it('should ignore role descriptions in parentheses', () => {
		expect(getInitials('Jane Smith (CEO)')).to.equal('JS');
	});

	it('should handle names with brackets and special characters', () => {
		expect(getInitials('John [Admin] Doe')).to.equal('JD');
		expect(getInitials('Alice @Company')).to.equal('A');
	});

	it('should handle names with only parentheses content', () => {
		expect(getInitials('(Test)')).to.equal('');
	});

	it('should support non-latin characters', () => {
		expect(getInitials('Привет Ša')).to.equal('ПŠ');
		expect(getInitials('Привет')).to.equal('П');
		expect(getInitials('åse hylle')).to.equal('ÅH');
	});
});

