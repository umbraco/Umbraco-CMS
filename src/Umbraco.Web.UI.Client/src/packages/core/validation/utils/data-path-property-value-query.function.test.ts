import { UmbDataPathPropertyValueQuery } from './data-path-property-value-query.function.js';
import { expect } from '@open-wc/testing';

describe('UmbDataPathPropertyValueQuery', () => {
	describe('alias-only (config / non-variant scenarios)', () => {
		it('emits only the alias filter when culture and segment are absent', () => {
			expect(UmbDataPathPropertyValueQuery({ alias: 'myAlias' })).to.equal("?(@.alias == 'myAlias')");
		});

		it('treats explicit culture: undefined the same as omitted', () => {
			expect(UmbDataPathPropertyValueQuery({ alias: 'myAlias', culture: undefined })).to.equal(
				"?(@.alias == 'myAlias')",
			);
		});

		it('treats explicit segment: undefined the same as omitted', () => {
			expect(UmbDataPathPropertyValueQuery({ alias: 'myAlias', segment: undefined })).to.equal(
				"?(@.alias == 'myAlias')",
			);
		});
	});

	describe('with culture only', () => {
		it('renders the culture filter alongside alias when culture is a non-empty string', () => {
			expect(UmbDataPathPropertyValueQuery({ alias: 'myAlias', culture: 'en-us' })).to.equal(
				"?(@.alias == 'myAlias' && @.culture == 'en-us')",
			);
		});

		it('renders @.culture == null when culture is null (invariant scenario)', () => {
			expect(UmbDataPathPropertyValueQuery({ alias: 'myAlias', culture: null })).to.equal(
				"?(@.alias == 'myAlias' && @.culture == null)",
			);
		});
	});

	describe('with segment only', () => {
		it('renders the segment filter alongside alias when segment is a non-empty string', () => {
			expect(UmbDataPathPropertyValueQuery({ alias: 'myAlias', segment: 'mobile' })).to.equal(
				"?(@.alias == 'myAlias' && @.segment == 'mobile')",
			);
		});

		it('renders @.segment == null when segment is null', () => {
			expect(UmbDataPathPropertyValueQuery({ alias: 'myAlias', segment: null })).to.equal(
				"?(@.alias == 'myAlias' && @.segment == null)",
			);
		});
	});

	describe('with culture and segment', () => {
		it('emits all three filters joined by &&, in alias → culture → segment order', () => {
			expect(UmbDataPathPropertyValueQuery({ alias: 'myAlias', culture: 'en-us', segment: 'mobile' })).to.equal(
				"?(@.alias == 'myAlias' && @.culture == 'en-us' && @.segment == 'mobile')",
			);
		});

		it('renders both culture and segment as null when both are null (invariant + no segment)', () => {
			expect(UmbDataPathPropertyValueQuery({ alias: 'myAlias', culture: null, segment: null })).to.equal(
				"?(@.alias == 'myAlias' && @.culture == null && @.segment == null)",
			);
		});

		it('mixes a culture string with a null segment', () => {
			expect(UmbDataPathPropertyValueQuery({ alias: 'myAlias', culture: 'en-us', segment: null })).to.equal(
				"?(@.alias == 'myAlias' && @.culture == 'en-us' && @.segment == null)",
			);
		});

		it('mixes a null culture with a segment string', () => {
			expect(UmbDataPathPropertyValueQuery({ alias: 'myAlias', culture: null, segment: 'mobile' })).to.equal(
				"?(@.alias == 'myAlias' && @.culture == null && @.segment == 'mobile')",
			);
		});
	});

	describe('empty string handling', () => {
		it('renders @.culture == null when culture is the empty string', () => {
			expect(UmbDataPathPropertyValueQuery({ alias: 'myAlias', culture: '' })).to.equal(
				"?(@.alias == 'myAlias' && @.culture == null)",
			);
		});

		it('renders @.segment == null when segment is the empty string', () => {
			expect(UmbDataPathPropertyValueQuery({ alias: 'myAlias', segment: '' })).to.equal(
				"?(@.alias == 'myAlias' && @.segment == null)",
			);
		});
	});
});
