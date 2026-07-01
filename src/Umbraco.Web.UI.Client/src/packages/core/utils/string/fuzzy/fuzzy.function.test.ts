import { expect } from '@open-wc/testing';
import { fuzzyTokenize, fuzzyMatchScore } from './fuzzy.function.js';

describe('tokenize', () => {
	it('should split on spaces', () => {
		expect(fuzzyTokenize('hello world')).to.deep.equal(['hello', 'world']);
	});

	it('should split on hyphens', () => {
		expect(fuzzyTokenize('icon-truck')).to.deep.equal(['icon', 'truck']);
	});

	it('should split on dots', () => {
		expect(fuzzyTokenize('Umb.PropertyEditorUi.TextBox')).to.deep.equal(['umb', 'propertyeditorui', 'textbox']);
	});

	it('should lowercase all tokens', () => {
		expect(fuzzyTokenize('Hello World')).to.deep.equal(['hello', 'world']);
	});

	it('should filter empty strings from consecutive delimiters', () => {
		expect(fuzzyTokenize('a--b  c..d')).to.deep.equal(['a', 'b', 'c', 'd']);
	});

	it('should return empty array for empty string', () => {
		expect(fuzzyTokenize('')).to.deep.equal([]);
	});
});

describe('fuzzyMatchScore', () => {
	it('should return 1 for identical single tokens', () => {
		expect(fuzzyMatchScore(['truck'], ['truck'])).to.equal(1);
	});

	it('should return 0 when no token meets threshold', () => {
		expect(fuzzyMatchScore(['xyz'], ['abc'])).to.equal(0);
	});

	it('should return positive score for close typo', () => {
		const score = fuzzyMatchScore(['truk'], ['truck']);
		expect(score).to.be.greaterThan(0.6);
	});

	it('should return 0 when one query token fails threshold', () => {
		// "truck" matches, but "zzz" does not
		expect(fuzzyMatchScore(['truck', 'zzz'], ['truck', 'van'])).to.equal(0);
	});

	it('should return average similarity across multiple matching tokens', () => {
		// Both tokens are identical matches (similarity 1.0 each)
		const score = fuzzyMatchScore(['hello', 'world'], ['hello', 'world']);
		expect(score).to.equal(1);
	});

	it('should respect custom threshold', () => {
		// With a very high threshold, a close-but-not-perfect match should fail
		const score = fuzzyMatchScore(['truk'], ['truck'], 0.95);
		expect(score).to.equal(0);
	});

	it('should find best match across multiple searchable tokens', () => {
		// "truk" should fuzzy-match "truck" even with other tokens present
		const score = fuzzyMatchScore(['truk'], ['van', 'truck', 'car']);
		expect(score).to.be.greaterThan(0);
	});
});
