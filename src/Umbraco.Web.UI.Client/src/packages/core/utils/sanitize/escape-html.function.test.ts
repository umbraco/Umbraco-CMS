import { expect } from '@open-wc/testing';
import { escapeHTML } from './escape-html.function.js';

describe('escapeHtml', () => {
	it('should escape html', () => {
		expect(escapeHTML('<script>alert("XSS")</script>')).to.equal('&lt;script&gt;alert(&#34;XSS&#34;)&lt;/script&gt;');
	});
});
