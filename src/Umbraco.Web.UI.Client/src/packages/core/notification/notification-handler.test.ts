import { UmbNotificationHandler } from './notification-handler.js';
import type { UmbNotificationOptions } from './types.js';
import { aTimeout, assert, expect, fixture, html, oneEvent } from '@open-wc/testing';
import { UmbId } from '@umbraco-cms/backoffice/id';

const PENDING = Symbol('pending');

/**
 * Resolves with the settled value, or with PENDING if the promise has not settled within the given time.
 * @param {Promise<T>} promise The promise to wait for.
 * @param {number} ms How long to wait.
 * @returns {Promise<Awaited<T> | typeof PENDING>} The settled value, or PENDING.
 */
async function settleWithin<T>(promise: Promise<T>, ms = 1000): Promise<Awaited<T> | typeof PENDING> {
	return Promise.race([promise, aTimeout(ms).then((): typeof PENDING => PENDING)]);
}

/**
 * Shows the notification in a toast container, the way the backoffice does, and waits until it is open.
 * @param {UmbNotificationHandler} handler The handler of the notification to show.
 */
async function show(handler: UmbNotificationHandler): Promise<void> {
	handler.element.style.setProperty('--uui-toast-notification-animation-duration', '1ms');
	const opened = oneEvent(handler.element, 'opened');
	await fixture(html`<uui-toast-notification-container>${handler.element}</uui-toast-notification-container>`);
	await opened;
}

describe('UmbNotificationHandler', () => {
	let notificationHandler: UmbNotificationHandler;

	beforeEach(async () => {
		const options: UmbNotificationOptions = {};
		notificationHandler = new UmbNotificationHandler(options);
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has a key property', () => {
				expect(notificationHandler).to.have.property('key');
				expect(UmbId.validate(notificationHandler.key)).to.be.true;
			});

			it('has an element property', () => {
				expect(notificationHandler).to.have.property('element');
			});

			it('has an color property', () => {
				expect(notificationHandler).to.have.property('color');
			});

			it('has an duration property', () => {
				expect(notificationHandler).to.have.property('duration');
			});

			it('sets a default duration to 6000 ms', () => {
				expect(notificationHandler.duration).to.equal(6000);
			});
		});

		describe('methods', () => {
			it('has a close method', () => {
				expect(notificationHandler).to.have.property('close').that.is.a('function');
			});

			it('has a onClose method', () => {
				expect(notificationHandler).to.have.property('onClose').that.is.a('function');
			});

			it('returns result from close method in onClose promise', () => {
				notificationHandler.close('result value');

				const onClose = notificationHandler.onClose();
				expect(onClose).that.is.a('promise');

				expect(onClose).to.be.not.null;

				if (onClose !== null) {
					return onClose.then((result) => {
						expect(result).to.equal('result value');
					});
				}

				return assert.fail('onClose should not have returned ´null´');
			});
		});
	});

	describe('onClose', () => {
		it('resolves when the user dismisses the notification', async () => {
			const handler = new UmbNotificationHandler({ duration: null, data: { message: 'Message' } });
			await show(handler);

			handler.element.shadowRoot!.querySelector<HTMLElement>('#close uui-button')!.click();

			expect(await settleWithin(handler.onClose())).to.not.equal(PENDING);
		});

		it('resolves when the notification closes after its duration', async () => {
			const handler = new UmbNotificationHandler({ duration: 50, data: { message: 'Message' } });
			await show(handler);

			expect(await settleWithin(handler.onClose())).to.not.equal(PENDING);
		});

		it('resolves with the value given to close() when the notification then finishes closing', async () => {
			const handler = new UmbNotificationHandler({ duration: null, data: { message: 'Message' } });
			await show(handler);
			const closed = oneEvent(handler.element, 'closed');

			handler.close('result value');
			await closed;

			expect(await handler.onClose()).to.equal('result value');
		});
	});

	describe('Layout', () => {
		describe('Default Layout', () => {
			let defaultLayoutNotification: UmbNotificationHandler;
			let layoutElement: any;

			beforeEach(async () => {
				const options: UmbNotificationOptions = {
					color: 'positive',
					data: {
						message: 'Notification default layout message',
					},
				};
				defaultLayoutNotification = new UmbNotificationHandler(options);
				layoutElement = defaultLayoutNotification.element.querySelector('umb-notification-layout-default');
			});

			it('creates a default layout if a custom element name havnt been specified', () => {
				expect(layoutElement.tagName).to.equal('UMB-NOTIFICATION-LAYOUT-DEFAULT');
			});

			it('it sets notificationHandler on custom element', () => {
				expect(layoutElement.notificationHandler).to.equal(defaultLayoutNotification);
			});

			it('it sets data on custom element', () => {
				expect(layoutElement.data.message).to.equal('Notification default layout message');
			});
		});

		describe('Custom Layout', () => {
			let customLayoutNotification: UmbNotificationHandler;
			let layoutElement: any;

			beforeEach(async () => {
				const options: UmbNotificationOptions = {
					elementName: 'umb-notification-test-element',
					color: 'positive',
					data: {
						message: 'Notification custom layout message',
					},
				};
				customLayoutNotification = new UmbNotificationHandler(options);
				layoutElement = customLayoutNotification.element.querySelector('umb-notification-test-element');
			});

			it('creates a custom element', () => {
				expect(layoutElement.tagName).to.equal('UMB-NOTIFICATION-TEST-ELEMENT');
			});

			it('it sets notificationHandler on custom element', () => {
				expect(layoutElement.notificationHandler).to.equal(customLayoutNotification);
			});

			it('it sets data on custom element', () => {
				expect(layoutElement.data.message).to.equal('Notification custom layout message');
			});
		});
	});
});
