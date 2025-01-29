import { UmbNotificationHandler } from './notification-handler.js';
import type { UmbNotificationOptions } from './notification.context.js';
import { assert, expect } from '@open-wc/testing';
import { UmbId } from '@umbraco-cms/backoffice/id';

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
