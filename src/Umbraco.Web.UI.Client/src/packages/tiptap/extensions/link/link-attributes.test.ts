import { umbIsLocalLinkHref, umbLinkFromAttributes } from './link-attributes.js';
import { expect } from '@open-wc/testing';

describe('umbIsLocalLinkHref', () => {
	it('recognises a local link', () => {
		expect(umbIsLocalLinkHref('/{localLink:eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f}')).to.be.true;
	});

	it('recognises a local link without a leading slash', () => {
		expect(umbIsLocalLinkHref('{localLink:eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f}')).to.be.true;
	});

	it('recognises a local link with URL-encoded braces', () => {
		expect(umbIsLocalLinkHref('/%7BlocalLink:eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f%7D')).to.be.true;
	});

	it('does not recognise a URL that merely contains the token', () => {
		expect(umbIsLocalLinkHref('https://example.com/?ref=/{localLink:eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f}')).to.be
			.false;
		expect(umbIsLocalLinkHref('https://example.com/{localLink:eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f}')).to.be.false;
	});

	it('does not recognise an ordinary URL, an anchor or a missing href', () => {
		expect(umbIsLocalLinkHref('https://example.com')).to.be.false;
		expect(umbIsLocalLinkHref('#section')).to.be.false;
		expect(umbIsLocalLinkHref('')).to.be.false;
		expect(umbIsLocalLinkHref(null)).to.be.false;
		expect(umbIsLocalLinkHref(undefined)).to.be.false;
	});
});

describe('umbLinkFromAttributes', () => {
	it('has no type when there is no anchor yet, so the picker asks for a source', () => {
		const link = umbLinkFromAttributes({});

		expect(link.type).to.be.undefined;
		expect(link.unique).to.be.null;
		expect(link.url).to.be.undefined;
	});

	it('treats an anchor with a URL as an external link', () => {
		const link = umbLinkFromAttributes({ href: 'https://example.com' });

		expect(link.type).to.equal('external');
		expect(link.unique).to.be.null;
		expect(link.url).to.equal('https://example.com');
	});

	it('treats an anchor stored with type="external" as an external link', () => {
		const link = umbLinkFromAttributes({ href: 'https://example.com', type: 'external' });

		expect(link.type).to.equal('external');
	});

	// Without a URL there is nothing to derive the type from but the anchor itself, and the picker has to offer
	// the URL and anchor fields rather than asking for a source again.
	it('treats an anchor with only a query string as an external link (regression)', () => {
		const link = umbLinkFromAttributes({ href: '#section', 'data-anchor': '#section' });

		expect(link.type).to.equal('external');
		expect(link.queryString).to.equal('#section');
		expect(link.url).to.equal('');
	});

	it('carries the entity type and unique of a local document link', () => {
		const link = umbLinkFromAttributes({
			href: '/{localLink:eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f}',
			type: 'document',
		});

		expect(link.type).to.equal('document');
		expect(link.unique).to.equal('eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f');
	});

	it('carries the entity type and unique of a local media link', () => {
		const link = umbLinkFromAttributes({
			href: '/{localLink:7e21a725-b905-4c5f-86dc-8c41ec116e39}',
			type: 'media',
		});

		expect(link.type).to.equal('media');
		expect(link.unique).to.equal('7e21a725-b905-4c5f-86dc-8c41ec116e39');
	});

	it('separates the query string from the URL of a local link', () => {
		const link = umbLinkFromAttributes({
			href: '/{localLink:eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f}#section',
			'data-anchor': '#section',
			type: 'document',
		});

		expect(link.url).to.equal('/{localLink:eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f}');
		expect(link.queryString).to.equal('#section');
		expect(link.unique).to.equal('eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f');
	});

	it('maps the remaining attributes onto the link', () => {
		const link = umbLinkFromAttributes({
			href: 'https://example.com',
			title: 'Example',
			target: '_blank',
			'data-culture': 'da-DK',
		});

		expect(link.name).to.equal('Example');
		expect(link.target).to.equal('_blank');
		expect(link.culture).to.equal('da-DK');
	});
});
