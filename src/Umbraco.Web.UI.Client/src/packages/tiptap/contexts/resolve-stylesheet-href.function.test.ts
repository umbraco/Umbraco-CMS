import { resolveStylesheetHref } from './resolve-stylesheet-href.function.js';
import { expect } from '@open-wc/testing';

describe('resolveStylesheetHref', () => {
	it('prefixes the root path onto a configured stylesheet', () => {
		const href = resolveStylesheetHref('/rte-test.css', '/mycss', '');
		expect(href).to.equal(`${window.location.origin}/mycss/rte-test.css`);
	});

	it('does not double up the root path when the configured stylesheet already includes it', () => {
		const href = resolveStylesheetHref('/mycss/already-prefixed.css', '/mycss', '');
		expect(href).to.equal(`${window.location.origin}/mycss/already-prefixed.css`);
	});

	it('resolves against the server origin when one is set, e.g. a split Vite dev client', () => {
		const href = resolveStylesheetHref('/rte-test.css', '/mycss', 'https://localhost:44339');
		expect(href).to.equal('https://localhost:44339/mycss/rte-test.css');
	});

	it('leaves a protocol-relative stylesheet URL untouched', () => {
		const href = resolveStylesheetHref('//cdn.example.com/style.css', '/mycss', 'https://localhost:44339');
		expect(href).to.equal('//cdn.example.com/style.css');
	});

	it('leaves an absolute stylesheet URL untouched', () => {
		const href = resolveStylesheetHref('https://cdn.example.com/style.css', '/mycss', 'https://localhost:44339');
		expect(href).to.equal('https://cdn.example.com/style.css');
	});
});
