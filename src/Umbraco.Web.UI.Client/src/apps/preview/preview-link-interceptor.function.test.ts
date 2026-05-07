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

	function appendAnchor(attrs: Partial<Record<'href' | 'download', string>> & { html?: string } = {}): HTMLAnchorElement {
		const anchor = iframe.contentDocument!.createElement('a');
		if (attrs.href !== undefined) anchor.setAttribute('href', attrs.href);
		if (attrs.download !== undefined) anchor.setAttribute('download', attrs.download);
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
	});

	describe('defaultPrevented guard', () => {
		it('does not open a new tab when an earlier listener has already prevented default', () => {
			// Re-create the iframe so we can attach a "preventer" listener BEFORE the interceptor —
			// click listeners run in registration order, so the preventer must be registered first
			// for the interceptor to observe defaultPrevented === true when it runs.
			iframe.remove();
			iframe = document.createElement('iframe');
			document.body.appendChild(iframe);
			iframe.contentDocument!.addEventListener('click', (event) => event.preventDefault());
			attachPreviewLinkInterceptor(iframe);

			const anchor = iframe.contentDocument!.createElement('a');
			anchor.setAttribute('href', '/files/brochure.pdf');
			iframe.contentDocument!.body.appendChild(anchor);

			const event = clickAnchor(anchor);

			expect(event.defaultPrevented).to.be.true;
			expect(openSpy.calls, 'window.open should not be called').to.have.lengthOf(0);
		});
	});
});
