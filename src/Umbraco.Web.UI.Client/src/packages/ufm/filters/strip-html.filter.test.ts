import { expect } from '@open-wc/testing';
import { UmbUfmStripHtmlFilterApi } from './strip-html.filter.js';

describe('UmbUfmStripHtmlFilter', () => {
	let filter: UmbUfmStripHtmlFilterApi;

	beforeEach(() => {
		filter = new UmbUfmStripHtmlFilterApi();
	});

	describe('filter', () => {
		it('should strip HTML tags from string', () => {
			const result = filter.filter('<p>Hello <strong>World</strong></p>');
			expect(result).to.equal('Hello World');
		});

		it('should handle empty string', () => {
			const result = filter.filter('');
			expect(result).to.equal('');
		});

		it('should handle null input', () => {
			const result = filter.filter(null);
			expect(result).to.equal('');
		});

		it('should handle undefined input', () => {
			const result = filter.filter(undefined);
			expect(result).to.equal('');
		});

		it('should handle markup object', () => {
			const result = filter.filter({ markup: '<p>Test</p>' });
			expect(result).to.equal('Test');
		});

		it('should strip complex HTML', () => {
			const result = filter.filter('<div><h1>Title</h1><p>Paragraph with <a href="#">link</a></p></div>');
			expect(result).to.equal('TitleParagraph with link');
		});

		it('should handle plain text without HTML', () => {
			const result = filter.filter('Plain text');
			expect(result).to.equal('Plain text');
		});
	});
});
