import { UmbDataPathVariantQuery } from './data-path-variant-query.function.js';
import { expect } from '@open-wc/testing';

describe('UmbDataPathVariantQuery', () => {
	describe('culture-only inputs', () => {
		it('renders only the culture filter when segment is absent', () => {
			expect(UmbDataPathVariantQuery({ culture: 'en-us' })).to.equal("?(@.culture == 'en-us')");
		});

		it('treats explicit segment: undefined the same as omitted', () => {
			expect(UmbDataPathVariantQuery({ culture: 'en-us', segment: undefined })).to.equal("?(@.culture == 'en-us')");
		});

		it('renders @.culture == null when culture is null (invariant variant)', () => {
			expect(UmbDataPathVariantQuery({ culture: null })).to.equal('?(@.culture == null)');
		});
	});

	describe('with segment', () => {
		it('emits both filters joined by && when both fields are non-empty strings', () => {
			expect(UmbDataPathVariantQuery({ culture: 'en-us', segment: 'mobile' })).to.equal(
				"?(@.culture == 'en-us' && @.segment == 'mobile')",
			);
		});

		it('renders @.segment == null when segment is null', () => {
			expect(UmbDataPathVariantQuery({ culture: 'en-us', segment: null })).to.equal(
				"?(@.culture == 'en-us' && @.segment == null)",
			);
		});

		it('renders both as null when both are null', () => {
			expect(UmbDataPathVariantQuery({ culture: null, segment: null })).to.equal(
				'?(@.culture == null && @.segment == null)',
			);
		});

		it('mixes a null culture with a segment string', () => {
			expect(UmbDataPathVariantQuery({ culture: null, segment: 'mobile' })).to.equal(
				"?(@.culture == null && @.segment == 'mobile')",
			);
		});
	});

	describe('empty string handling', () => {
		it('renders @.culture == null when culture is the empty string', () => {
			expect(UmbDataPathVariantQuery({ culture: '' })).to.equal('?(@.culture == null)');
		});

		it('renders @.segment == null when segment is the empty string', () => {
			expect(UmbDataPathVariantQuery({ culture: 'en-us', segment: '' })).to.equal(
				"?(@.culture == 'en-us' && @.segment == null)",
			);
		});
	});
});
