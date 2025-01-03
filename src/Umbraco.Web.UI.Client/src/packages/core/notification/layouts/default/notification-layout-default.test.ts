import type { UmbNotificationDefaultData } from './notification-layout-default.element.js';
import { UmbNotificationLayoutDefaultElement } from './notification-layout-default.element.js';
import { fixture, expect, html } from '@open-wc/testing';
import type { UUIToastNotificationLayoutElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbNotificationHandler } from '@umbraco-cms/backoffice/notification';
import type { UmbTestRunnerWindow } from '@umbraco-cms/internal/test-utils';

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
		});
	});
});
