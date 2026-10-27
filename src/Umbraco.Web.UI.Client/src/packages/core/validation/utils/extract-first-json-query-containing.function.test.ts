import { extractFirstJsonQueryContaining } from './extract-first-json-query-containing.function.js';
import { expect } from '@open-wc/testing';

describe('extractFirstJsonQueryContaining', () => {
	describe('no brackets in path', () => {
		it('returns undefined', () => {
			const result = extractFirstJsonQueryContaining('$.values', () => true);
			expect(result).to.be.undefined;
		});
	});

	describe('single query', () => {
		it('returns the properties when the query matches', () => {
			const path = `$.values[?(@.alias == 'title')]`;
			const result = extractFirstJsonQueryContaining(path, (props) => props.alias === 'title');
			expect(result).to.deep.equal({ alias: 'title' });
		});

		it('returns undefined when the only query does not match', () => {
			const path = `$.values[?(@.alias == 'title')]`;
			const result = extractFirstJsonQueryContaining(path, (props) => props.alias === 'somethingElse');
			expect(result).to.be.undefined;
		});
	});

	describe('multiple queries', () => {
		const path = `$.values[?(@.key == 'block-key')].value.contentData[?(@.alias == 'title' && @.culture == 'en-us')]`;

		it('skips a non-matching query and returns the properties of the first matching one', () => {
			const result = extractFirstJsonQueryContaining(path, (props) => props.alias !== undefined);
			expect(result).to.deep.equal({ alias: 'title', culture: 'en-us' });
		});

		it('returns the properties of the first query, when it matches, without inspecting later queries', () => {
			const result = extractFirstJsonQueryContaining(path, (props) => props.key !== undefined);
			expect(result).to.deep.equal({ key: 'block-key' });
		});

		it('returns undefined when none of the queries match', () => {
			const result = extractFirstJsonQueryContaining(path, (props) => props.segment !== undefined);
			expect(result).to.be.undefined;
		});
	});
});
