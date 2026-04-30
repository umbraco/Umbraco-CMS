import { expect } from '@open-wc/testing';
import { getCharacterCountState, isCharacterLimitExceeded } from './character-count.js';

describe('getCharacterCountState', () => {
	it('is not visible when far from limit', () => {
		const state = getCharacterCountState(100, 50);
		expect(state.visible).to.be.false;
		expect(state.remaining).to.equal(50);
	});

	it('is visible at exactly 20% remaining', () => {
		const state = getCharacterCountState(100, 80);
		expect(state.visible).to.be.true;
		expect(state.remaining).to.equal(20);
	});

	it('is visible when within 20% of limit', () => {
		const state = getCharacterCountState(100, 90);
		expect(state.visible).to.be.true;
		expect(state.remaining).to.equal(10);
	});

	it('is visible at exactly 0 remaining', () => {
		const state = getCharacterCountState(100, 100);
		expect(state.visible).to.be.true;
		expect(state.remaining).to.equal(0);
	});

	it('is not visible when limit is exceeded', () => {
		const state = getCharacterCountState(100, 105);
		expect(state.visible).to.be.false;
		expect(state.remaining).to.equal(-5);
	});

	it('handles small maxChars correctly', () => {
		const state = getCharacterCountState(5, 4);
		expect(state.visible).to.be.true;
		expect(state.remaining).to.equal(1);
	});
});

describe('isCharacterLimitExceeded', () => {
	it('returns false when no maxChars', () => {
		expect(isCharacterLimitExceeded(undefined, 50)).to.be.false;
	});

	it('returns false when under limit', () => {
		expect(isCharacterLimitExceeded(100, 50)).to.be.false;
	});

	it('returns false when at exact limit', () => {
		expect(isCharacterLimitExceeded(100, 100)).to.be.false;
	});

	it('returns true when over limit', () => {
		expect(isCharacterLimitExceeded(100, 101)).to.be.true;
	});
});
