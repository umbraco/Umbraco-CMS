import { expect } from '@open-wc/testing';
import { sanitizeHTML } from './sanitize-html.function.js';

describe('sanitizeHTML', () => {
	it('should allow benign HTML', () => {
		expect(sanitizeHTML('<strong>Test</strong>')).to.equal('<strong>Test</strong>');
	});

	it('should remove potentially harmful content', () => {
		expect(sanitizeHTML('<script>alert("XSS")</script>')).to.equal('');
	});

	it('should remove potentially harmful attributes', () => {
		expect(sanitizeHTML('<a href="javascript:alert(\'XSS\')">Test</a>')).to.equal('<a>Test</a>');
	});

	it('should remove potentially harmful content and attributes', () => {
		expect(sanitizeHTML('<a href="javascript:alert(\'XSS\')"><script>alert("XSS")</script></a>')).to.equal('<a></a>');
	});

	it('should allow benign attributes', () => {
		expect(sanitizeHTML('<a href="/test">Test</a>')).to.equal('<a href="/test">Test</a>');
	});
});
