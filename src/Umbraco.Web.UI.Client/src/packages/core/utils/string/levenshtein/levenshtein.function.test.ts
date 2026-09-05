import { expect } from '@open-wc/testing';
import { levenshteinDistance, levenshteinSimilarity } from './levenshtein.function.js';

describe('levenshteinDistance', () => {
	it('should return 0 for identical strings', () => {
		expect(levenshteinDistance('hello', 'hello')).to.equal(0);
	});

	it('should return the length of the other string when one is empty', () => {
		expect(levenshteinDistance('', 'hello')).to.equal(5);
		expect(levenshteinDistance('hello', '')).to.equal(5);
	});

	it('should return 0 for two empty strings', () => {
		expect(levenshteinDistance('', '')).to.equal(0);
	});

	it('should handle single character edits', () => {
		expect(levenshteinDistance('cat', 'bat')).to.equal(1); // substitution
		expect(levenshteinDistance('cat', 'cats')).to.equal(1); // insertion
		expect(levenshteinDistance('cats', 'cat')).to.equal(1); // deletion
	});

	it('should handle multi-character edits', () => {
		expect(levenshteinDistance('kitten', 'sitting')).to.equal(3);
		expect(levenshteinDistance('saturday', 'sunday')).to.equal(3);
	});

	it('should be symmetric', () => {
		expect(levenshteinDistance('abc', 'xyz')).to.equal(levenshteinDistance('xyz', 'abc'));
		expect(levenshteinDistance('truck', 'truk')).to.equal(levenshteinDistance('truk', 'truck'));
	});

	it('should be case-sensitive', () => {
		expect(levenshteinDistance('Hello', 'hello')).to.equal(1);
	});
});

describe('levenshteinSimilarity', () => {
	it('should return 1 for identical strings', () => {
		expect(levenshteinSimilarity('hello', 'hello')).to.equal(1);
	});

	it('should return 1 for two empty strings', () => {
		expect(levenshteinSimilarity('', '')).to.equal(1);
	});

	it('should return 0 for completely different strings of equal length', () => {
		expect(levenshteinSimilarity('abc', 'xyz')).to.equal(0);
	});

	it('should return correct similarity for partial matches', () => {
		// "truk" vs "truck" → distance 1, max length 5 → similarity 0.8
		expect(levenshteinSimilarity('truk', 'truck')).to.equal(0.8);
	});

	it('should return 0 when one string is empty and the other is not', () => {
		expect(levenshteinSimilarity('', 'hello')).to.equal(0);
	});
});
