import { expect } from '@open-wc/testing';
import { getLinkableMediaUrl, renderImagePreview } from './media-link.function.js';

const markupInjectingUrl =
	'https://example.com/media/abc/image.jpg" ><script src=https://example.org/payload.js></script><img src="x.svg';

describe('getLinkableMediaUrl', () => {
	const base = 'https://example.com/umbraco/section/media';

	it('returns http and https URLs', () => {
		expect(getLinkableMediaUrl('https://example.com/media/abc/image.jpg', base)).to.equal(
			'https://example.com/media/abc/image.jpg',
		);
		expect(getLinkableMediaUrl('http://example.com/media/abc/image.jpg', base)).to.equal(
			'http://example.com/media/abc/image.jpg',
		);
	});

	it('returns relative URLs, as they resolve against the base', () => {
		expect(getLinkableMediaUrl('/media/abc/image.jpg', base)).to.equal('/media/abc/image.jpg');
	});

	it('rejects URLs with a non-web scheme', () => {
		expect(getLinkableMediaUrl('javascript:alert(1)', base)).to.be.undefined;
		expect(getLinkableMediaUrl(' JavaScript:alert(1)', base)).to.be.undefined;
		expect(getLinkableMediaUrl('data:text/html,<script>alert(1)</script>', base)).to.be.undefined;
		expect(getLinkableMediaUrl('vbscript:msgbox(1)', base)).to.be.undefined;
	});

	it('rejects empty values', () => {
		expect(getLinkableMediaUrl(undefined, base)).to.be.undefined;
		expect(getLinkableMediaUrl('', base)).to.be.undefined;
	});
});

describe('renderImagePreview', () => {
	it('references the image by URL without interpreting it as markup', () => {
		const document = window.document.implementation.createHTMLDocument('');

		renderImagePreview(document, markupInjectingUrl);

		expect(document.querySelectorAll('script')).to.have.length(0);
		const images = document.body.querySelectorAll('img');
		expect(images).to.have.length(1);
		expect(images[0].getAttribute('src')).to.equal(markupInjectingUrl);
	});

	it('replaces any existing content of the document body', () => {
		const document = window.document.implementation.createHTMLDocument('');
		document.body.append(document.createElement('p'));

		renderImagePreview(document, '/media/abc/image.svg');

		expect(document.body.children).to.have.length(1);
		expect(document.body.firstElementChild?.tagName).to.equal('IMG');
	});
});
