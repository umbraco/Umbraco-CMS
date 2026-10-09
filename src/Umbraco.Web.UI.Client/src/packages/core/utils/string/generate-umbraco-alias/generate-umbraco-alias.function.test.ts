import { generateAlias } from './generate-umbraco-alias.function.js';
import { expect } from '@open-wc/testing';

// Comprehensive camelCase coverage lives with toCamelCase; these tests pin
// the public generateAlias contract so a refactor cannot silently change it.
describe('generateAlias', () => {
	it('camelCases a multi-word title', () => {
		expect(generateAlias('My Document Type')).to.equal('myDocumentType');
	});

	it('returns an empty string for empty input', () => {
		expect(generateAlias('')).to.equal('');
	});

	it('is idempotent — applying twice yields the same result', () => {
		const once = generateAlias('My Document Type');
		expect(generateAlias(once)).to.equal(once);
	});
});
