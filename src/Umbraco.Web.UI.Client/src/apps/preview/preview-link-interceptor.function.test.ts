import { expect } from '@open-wc/testing';
import { attachPreviewLinkInterceptor } from './preview-link-interceptor.function.js';

type WindowOpenSpy = {
	calls: Array<{ url?: string | URL; target?: string; features?: string }>;
	restore: () => void;
};

function spyOnWindowOpen(): WindowOpenSpy {
	const original = window.open;
	const calls: WindowOpenSpy['calls'] = [];
	window.open = ((url?: string | URL, target?: string, features?: string) => {
		calls.push({ url, target, features });
		return null;
	}) as typeof window.open;
	return {
		calls,
		restore: () => {
			window.open = original;
		},
	};
}

function clickAnchor(anchor: HTMLAnchorElement): MouseEvent {
	const event = new MouseEvent('click', { bubbles: true, cancelable: true });
	anchor.dispatchEvent(event);
	return event;
}

describe('attachPreviewLinkInterceptor', () => {
	let iframe: HTMLIFrameElement;
	let openSpy: WindowOpenSpy;

	beforeEach(() => {
		iframe = document.createElement('iframe');
		document.body.appendChild(iframe);
		// Same-origin about:blank — contentDocument is immediately writable.
		iframe.contentDocument!.body.innerHTML = '';
		attachPreviewLinkInterceptor(iframe);
		openSpy = spyOnWindowOpen();
	});

	afterEach(() => {
		openSpy.restore();
		iframe.remove();
	});

	function appendAnchor(
		attrs: Partial<Record<'href' | 'download' | 'target', string>> & { html?: string } = {},
	): HTMLAnchorElement {
		const anchor = iframe.contentDocument!.createElement('a');
		if (attrs.href !== undefined) anchor.setAttribute('href', attrs.href);
		if (attrs.download !== undefined) anchor.setAttribute('download', attrs.download);
		if (attrs.target !== undefined) anchor.setAttribute('target', attrs.target);
		if (attrs.html) anchor.innerHTML = attrs.html;
		iframe.contentDocument!.body.appendChild(anchor);
		return anchor;
	}

	describe('when the iframe has no contentDocument', () => {
		it('returns silently and does not throw', () => {
			const detachedIframe = document.createElement('iframe');
			Object.defineProperty(detachedIframe, 'contentDocument', { value: null });
			expect(() => attachPreviewLinkInterceptor(detachedIframe)).to.not.throw();
		});
	});

	describe('non-HTML resource links', () => {
		it('intercepts a .pdf link and opens it in a new tab', () => {
			const anchor = appendAnchor({ href: '/files/brochure.pdf' });

			const event = clickAnchor(anchor);

			expect(event.defaultPrevented, 'preventDefault should be called').to.be.true;
			expect(openSpy.calls).to.have.lengthOf(1);
			expect(openSpy.calls[0].url).to.equal(anchor.href);
			expect(openSpy.calls[0].target).to.equal('_blank');
			expect(openSpy.calls[0].features).to.equal('noopener,noreferrer');
		});

		it('matches extensions case-insensitively', () => {
			const anchor = appendAnchor({ href: '/files/REPORT.PDF' });

			const event = clickAnchor(anchor);

			expect(event.defaultPrevented).to.be.true;
			expect(openSpy.calls).to.have.lengthOf(1);
		});

		it('intercepts office, archive and media extensions', () => {
			const samples = [
				'/a.docx',
				'/a.xlsx',
				'/a.pptx',
				'/a.zip',
				'/a.7z',
				'/a.mp3',
				'/a.mp4',
				'/a.mkv',
				'/a.flac',
				'/a.opus',
				'/a.avi',
				'/a.wmv',
			];

			for (const href of samples) {
				const anchor = appendAnchor({ href });
				const event = clickAnchor(anchor);
				expect(event.defaultPrevented, `${href} should be intercepted`).to.be.true;
				anchor.remove();
			}

			expect(openSpy.calls).to.have.lengthOf(samples.length);
		});

		it('intercepts a click on a child element inside the anchor (closest() lookup)', () => {
			const anchor = appendAnchor({ href: '/files/brochure.pdf', html: '<span>Download</span>' });
			const span = anchor.querySelector('span')!;

			const event = new MouseEvent('click', { bubbles: true, cancelable: true });
			span.dispatchEvent(event);

			expect(event.defaultPrevented).to.be.true;
			expect(openSpy.calls).to.have.lengthOf(1);
		});
	});

	describe('download attribute', () => {
		it('intercepts any link with a download attribute, even with an HTML-like extension', () => {
			const anchor = appendAnchor({ href: '/files/page.html', download: '' });

			const event = clickAnchor(anchor);

			expect(event.defaultPrevented).to.be.true;
			expect(openSpy.calls).to.have.lengthOf(1);
			expect(openSpy.calls[0].url).to.equal(anchor.href);
		});
	});

	describe('non-matching links', () => {
		it('does not intercept a plain HTML link', () => {
			const anchor = appendAnchor({ href: '/about' });

			const event = clickAnchor(anchor);

			expect(event.defaultPrevented).to.be.false;
			expect(openSpy.calls).to.have.lengthOf(0);
		});

		it('does not intercept a link to a .html page', () => {
			const anchor = appendAnchor({ href: '/contact.html' });

			const event = clickAnchor(anchor);

			expect(event.defaultPrevented).to.be.false;
			expect(openSpy.calls).to.have.lengthOf(0);
		});

		it('ignores clicks not originating from an anchor', () => {
			const div = iframe.contentDocument!.createElement('div');
			iframe.contentDocument!.body.appendChild(div);

			const event = clickAnchor(div as unknown as HTMLAnchorElement);

			expect(event.defaultPrevented).to.be.false;
			expect(openSpy.calls).to.have.lengthOf(0);
		});

		it('ignores anchors without an href', () => {
			const anchor = iframe.contentDocument!.createElement('a');
			iframe.contentDocument!.body.appendChild(anchor);

			const event = clickAnchor(anchor);

			expect(event.defaultPrevented).to.be.false;
			expect(openSpy.calls).to.have.lengthOf(0);
		});

		it('does not intercept same-origin links with target="_self"', () => {
			const anchor = appendAnchor({ href: '/about', target: '_self' });

			const event = clickAnchor(anchor);

			expect(event.defaultPrevented).to.be.false;
			expect(openSpy.calls).to.have.lengthOf(0);
		});

		it('does not intercept same-origin links with a named target', () => {
			const anchor = appendAnchor({ href: '/about', target: 'main-content' });

			const event = clickAnchor(anchor);

			expect(event.defaultPrevented).to.be.false;
			expect(openSpy.calls).to.have.lengthOf(0);
		});

		it('does not intercept mailto: links so the OS handler still fires', () => {
			// window.open('mailto:...') would spawn a blank tab — let the browser hand
			// these off to the system mail client instead.
			const anchor = appendAnchor({ href: 'mailto:hello@example.com' });

			const event = clickAnchor(anchor);

			expect(event.defaultPrevented).to.be.false;
			expect(openSpy.calls).to.have.lengthOf(0);
		});

		it('does not intercept tel: links', () => {
			const anchor = appendAnchor({ href: 'tel:+15555550100' });

			const event = clickAnchor(anchor);

			expect(event.defaultPrevented).to.be.false;
			expect(openSpy.calls).to.have.lengthOf(0);
		});
	});

	describe('cross-origin links', () => {
		it('intercepts an external http(s) link and opens it in a new tab', () => {
			// Reproduces issue #20323 — without an interceptor, the sandbox swallows
			// the click and no tab opens.
			const anchor = appendAnchor({ href: 'https://www.google.com/' });

			const event = clickAnchor(anchor);

			expect(event.defaultPrevented).to.be.true;
			expect(openSpy.calls).to.have.lengthOf(1);
			expect(openSpy.calls[0].url).to.equal('https://www.google.com/');
			expect(openSpy.calls[0].target).to.equal('_blank');
			expect(openSpy.calls[0].features).to.equal('noopener,noreferrer');
		});

		it('intercepts an external link even when it has target="_blank"', () => {
			const anchor = appendAnchor({ href: 'https://example.com/', target: '_blank' });

			const event = clickAnchor(anchor);

			expect(event.defaultPrevented).to.be.true;
			expect(openSpy.calls).to.have.lengthOf(1);
		});
	});

	describe('sandbox-blocked targets', () => {
		it('intercepts a same-origin link with target="_blank" (sandbox lacks allow-popups)', () => {
			const anchor = appendAnchor({ href: '/about', target: '_blank' });

			const event = clickAnchor(anchor);

			expect(event.defaultPrevented).to.be.true;
			expect(openSpy.calls).to.have.lengthOf(1);
			expect(openSpy.calls[0].url).to.equal(anchor.href);
		});

		it('intercepts a same-origin link with target="_top" (sandbox lacks allow-top-navigation)', () => {
			// Without interception this would either be silently blocked or — worse, if
			// the sandbox were relaxed — replace the backoffice window itself.
			const anchor = appendAnchor({ href: '/about', target: '_top' });

			const event = clickAnchor(anchor);

			expect(event.defaultPrevented).to.be.true;
			expect(openSpy.calls).to.have.lengthOf(1);
		});

		it('intercepts a same-origin link with target="_parent"', () => {
			const anchor = appendAnchor({ href: '/about', target: '_parent' });

			const event = clickAnchor(anchor);

			expect(event.defaultPrevented).to.be.true;
			expect(openSpy.calls).to.have.lengthOf(1);
		});
	});

	describe('idempotency', () => {
		it('does not stack listeners when called multiple times on the same document', () => {
			// Simulates the back/forward cache scenario where `load` fires repeatedly for the
			// same Document — the interceptor should remain a single listener.
			attachPreviewLinkInterceptor(iframe);
			attachPreviewLinkInterceptor(iframe);

			const anchor = appendAnchor({ href: '/files/brochure.pdf' });
			clickAnchor(anchor);

			expect(openSpy.calls, 'window.open should be called exactly once per click').to.have.lengthOf(1);
		});

		it('attaches a fresh listener when the iframe loads a new Document', () => {
			// First document gets clicked once.
			const firstAnchor = appendAnchor({ href: '/files/first.pdf' });
			clickAnchor(firstAnchor);
			expect(openSpy.calls).to.have.lengthOf(1);

			// Replace the iframe content with a new Document and re-attach.
			iframe.remove();
			iframe = document.createElement('iframe');
			document.body.appendChild(iframe);
			attachPreviewLinkInterceptor(iframe);

			const secondAnchor = iframe.contentDocument!.createElement('a');
			secondAnchor.setAttribute('href', '/files/second.pdf');
			iframe.contentDocument!.body.appendChild(secondAnchor);
			clickAnchor(secondAnchor);

			expect(openSpy.calls, 'new document should still be intercepted').to.have.lengthOf(2);
		});
	});

	describe('SPA-router compatibility', () => {
		it('still intercepts non-HTML resources when an SPA router has preventDefaulted the click', () => {
			// SPAs commonly attach a bubble-phase click listener that calls preventDefault on every
			// anchor for client-side routing. The interceptor runs in capture phase and matches on
			// the resource type, so the PDF link still opens in a new tab.
			iframe.contentDocument!.addEventListener('click', (event) => event.preventDefault());

			const anchor = appendAnchor({ href: '/files/brochure.pdf' });
			clickAnchor(anchor);

			expect(openSpy.calls, 'PDF should still open in a new tab').to.have.lengthOf(1);
			expect(openSpy.calls[0].url).to.equal(anchor.href);
		});

		it('runs before bubble-phase handlers and stops propagation for matched links', () => {
			const order: string[] = [];
			iframe.contentDocument!.addEventListener('click', () => order.push('spa-router'));

			const anchor = appendAnchor({ href: '/files/brochure.pdf' });
			clickAnchor(anchor);

			expect(order, 'matched click should not reach the SPA-router bubble handler').to.deep.equal([]);
			expect(openSpy.calls).to.have.lengthOf(1);
		});

		it('lets HTML links flow through to bubble-phase handlers (so SPA routing still works)', () => {
			const order: string[] = [];
			iframe.contentDocument!.addEventListener('click', () => order.push('spa-router'));

			const anchor = appendAnchor({ href: '/about' });
			clickAnchor(anchor);

			expect(order, 'unmatched click should reach the SPA-router handler').to.deep.equal(['spa-router']);
			expect(openSpy.calls).to.have.lengthOf(0);
		});
	});
});
