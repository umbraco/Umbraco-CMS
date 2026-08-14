import type { UmbNotificationDefaultData } from './notification-layout-default.element.js';
import { UmbNotificationLayoutDefaultElement } from './notification-layout-default.element.js';
import { fixture, expect, html } from '@open-wc/testing';
import type { UUIToastNotificationLayoutElement } from '@umbraco-cms/backoffice/external/uui';
import type { UmbTestRunnerWindow } from '@umbraco-cms/internal/test-utils';
import { UmbNotificationHandler } from '../../notification-handler.js';

describe('UmbNotificationLayoutDefault', () => {
	let element: UmbNotificationLayoutDefaultElement;

	const data: UmbNotificationDefaultData = {
		headline: 'Notification Headline',
		message: 'Notification message',
	};

	const options = { elementName: 'umb-notification-layout-default', data };

	let notificationHandler: UmbNotificationHandler;

	beforeEach(async () => {
		notificationHandler = new UmbNotificationHandler(options);
		element = await fixture(
			html`<umb-notification-layout-default
				.notificationHandler=${notificationHandler}
				.data=${options.data}></umb-notification-layout-default>`,
		);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbNotificationLayoutDefaultElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).to.be.accessible();
		});
	}

	describe('Public API', () => {
		describe('properties', () => {
			it('has a notificationHandler property', () => {
				expect(element).to.have.property('notificationHandler');
			});

			it('has a data property', () => {
				expect(element).to.have.property('data');
			});
		});
	});

	describe('Data options', () => {
		describe('Headline', () => {
			it('sets headline on uui notification layout', () => {
				const uuiNotificationLayout: UUIToastNotificationLayoutElement | null =
					element.renderRoot.querySelector('#layout');
				expect(uuiNotificationLayout?.getAttribute('headline')).to.equal('Notification Headline');
			});
		});

		describe('message', () => {
			it('renders the message', () => {
				const messageElement: HTMLElement | null = element.renderRoot.querySelector('#message');
				expect(messageElement?.innerText).to.equal('Notification message');
			});

			it('renders HTML in the message as plain text', async () => {
				element.data = { message: 'Hello <strong>world</strong>' };
				await element.updateComplete;
				const messageElement: HTMLElement | null = element.renderRoot.querySelector('#message');
				expect(messageElement?.querySelector('strong')).to.be.null;
				expect(messageElement?.textContent).to.contain('Hello <strong>world</strong>');
			});
		});

		describe('htmlMessage', () => {
			it('renders an HTML string as HTML', async () => {
				element.data = { message: 'fallback', htmlMessage: 'Visit <a href="/umbraco">the backoffice</a>' };
				await element.updateComplete;
				const anchor = element.renderRoot.querySelector('#message a');
				expect(anchor).to.not.be.null;
				expect(anchor?.getAttribute('href')).to.equal('/umbraco');
			});

			it('takes precedence over message', async () => {
				element.data = { message: 'plain message', htmlMessage: '<em>html message</em>' };
				await element.updateComplete;
				const messageElement: HTMLElement | null = element.renderRoot.querySelector('#message');
				expect(messageElement?.querySelector('em')).to.not.be.null;
				expect(messageElement?.textContent).to.not.contain('plain message');
			});

			it('sanitizes an HTML string before rendering', async () => {
				element.data = {
					message: 'fallback',
					htmlMessage: 'Hello <img src="x" onerror="window.__xss = true"><script>window.__xss = true;</script>',
				};
				await element.updateComplete;
				const messageElement: HTMLElement | null = element.renderRoot.querySelector('#message');
				expect(messageElement?.querySelector('script')).to.be.null;
				expect(messageElement?.querySelector('img')?.getAttribute('onerror')).to.be.null;
				expect((window as unknown as { __xss?: boolean }).__xss).to.be.undefined;
			});

			it('renders a TemplateResult as-is', async () => {
				element.data = { message: 'fallback', htmlMessage: html`Rendered <code>template</code>` };
				await element.updateComplete;
				const messageElement: HTMLElement | null = element.renderRoot.querySelector('#message');
				expect(messageElement?.querySelector('code')?.textContent).to.equal('template');
			});
		});
	});
});
