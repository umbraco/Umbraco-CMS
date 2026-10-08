import { UmbPeekErrorNotificationElement } from './peek-error-notification.element.js';
import { fixture, expect, html } from '@open-wc/testing';

describe('UmbPeekErrorNotification', () => {
	let element: UmbPeekErrorNotificationElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-peek-error-notification></umb-peek-error-notification>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPeekErrorNotificationElement);
	});

	describe('message rendering', () => {
		it('renders the message as plain text', async () => {
			element.data = { message: 'Something <strong>failed</strong>' };
			await element.updateComplete;
			expect(element.renderRoot.querySelector('strong')).to.be.null;
			expect(element.renderRoot.textContent).to.contain('Something <strong>failed</strong>');
		});

		it('renders an htmlMessage string as sanitized HTML', async () => {
			element.data = {
				message: 'fallback',
				htmlMessage: 'See <a href="/status">the status page</a><script>window.__xss = true;</script>',
			};
			await element.updateComplete;
			const anchor = element.renderRoot.querySelector('a');
			expect(anchor?.getAttribute('href')).to.equal('/status');
			expect(element.renderRoot.querySelector('script')).to.be.null;
			expect((window as unknown as { __xss?: boolean }).__xss).to.be.undefined;
		});

		it('renders an htmlMessage as HTML when a short detail is present', async () => {
			element.data = { message: 'fallback', htmlMessage: '<em>html message</em>', detail: 'A short detail' };
			await element.updateComplete;
			expect(element.renderRoot.querySelector('em')?.textContent).to.equal('html message');
			expect(element.renderRoot.querySelector('.detail')?.textContent).to.contain('A short detail');
		});
	});
});
