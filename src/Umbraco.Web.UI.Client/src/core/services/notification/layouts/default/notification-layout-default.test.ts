import { fixture, expect, html } from '@open-wc/testing';
import { UUIToastNotificationLayoutElement } from '@umbraco-ui/uui';
import { UmbNotificationHandler } from '../../';
import type { UmbNotificationLayoutDefaultElement, UmbNotificationDefaultData } from '.';
import '.';

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
				.data=${options.data}></umb-notification-layout-default>`
		);
	});

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
