import { UmbNotificationHandler } from './notification-handler.js';
import type { UmbNotificationOptions } from './types.js';
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

			it('has an updateData method', () => {
				expect(notificationHandler).to.have.property('updateData').that.is.a('function');
			});

			it('has an updateColor method', () => {
				expect(notificationHandler).to.have.property('updateColor').that.is.a('function');
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

	describe('updateData', () => {
		let handler: UmbNotificationHandler;
		let layoutElement: any;

		beforeEach(async () => {
			const options: UmbNotificationOptions = {
				color: 'warning',
				data: {
					headline: 'Initial headline',
					message: 'Initial message',
				},
			};
			handler = new UmbNotificationHandler(options);
			layoutElement = handler.element.querySelector('umb-notification-layout-default');
		});

		it('updates the data on the layout element', () => {
			expect(layoutElement.data.message).to.equal('Initial message');
			expect(layoutElement.data.headline).to.equal('Initial headline');

			handler.updateData({
				headline: 'Updated headline',
				message: 'Updated message',
			});

			expect(layoutElement.data.message).to.equal('Updated message');
			expect(layoutElement.data.headline).to.equal('Updated headline');
		});

		it('can update data multiple times', () => {
			handler.updateData({ headline: 'First update', message: '1 / 10' });
			expect(layoutElement.data.message).to.equal('1 / 10');

			handler.updateData({ headline: 'Second update', message: '5 / 10' });
			expect(layoutElement.data.message).to.equal('5 / 10');

			handler.updateData({ headline: 'Final update', message: '10 / 10' });
			expect(layoutElement.data.message).to.equal('10 / 10');
		});

		it('replaces all data properties', () => {
			handler.updateData({
				message: 'Only message, no headline',
			});

			expect(layoutElement.data.message).to.equal('Only message, no headline');
			expect(layoutElement.data.headline).to.be.undefined;
		});
	});

	describe('updateColor', () => {
		let handler: UmbNotificationHandler;

		beforeEach(async () => {
			const options: UmbNotificationOptions = {
				color: 'warning',
				data: {
					message: 'Test message',
				},
			};
			handler = new UmbNotificationHandler(options);
		});

		it('updates the color property on the handler', () => {
			expect(handler.color).to.equal('warning');

			handler.updateColor('positive');

			expect(handler.color).to.equal('positive');
		});

		it('updates the color on the toast notification element', () => {
			expect(handler.element.color).to.equal('warning');

			handler.updateColor('danger');

			expect(handler.element.color).to.equal('danger');
		});

		it('can change color multiple times', () => {
			handler.updateColor('positive');
			expect(handler.color).to.equal('positive');
			expect(handler.element.color).to.equal('positive');

			handler.updateColor('danger');
			expect(handler.color).to.equal('danger');
			expect(handler.element.color).to.equal('danger');

			handler.updateColor('default');
			expect(handler.color).to.equal('default');
			expect(handler.element.color).to.equal('default');
		});
	});
});
