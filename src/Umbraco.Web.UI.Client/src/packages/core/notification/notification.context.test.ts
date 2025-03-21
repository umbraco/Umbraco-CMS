import type { UmbNotificationHandler } from './index.js';
import { UmbNotificationContext } from './index.js';
import { expect } from '@open-wc/testing';
import { UmbControllerHostElementElement } from '@umbraco-cms/backoffice/controller-api';

describe('UmbNotificationContext', () => {
	let notificationContext: UmbNotificationContext;

	beforeEach(async () => {
		notificationContext = new UmbNotificationContext(new UmbControllerHostElementElement());
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has a dialog property', () => {
				expect(notificationContext).to.have.property('notifications');
			});
		});

		describe('methods', () => {
			it('has a peek method', () => {
				expect(notificationContext).to.have.property('peek').that.is.a('function');
			});

			it('has a stay method', () => {
				expect(notificationContext).to.have.property('stay').that.is.a('function');
			});
		});
	});

	describe('peek', () => {
		let peekNotificationHandler: UmbNotificationHandler | undefined = undefined;
		let layoutElement: any;

		beforeEach(async () => {
			const peekOptions = {
				data: { headline: 'Peek notification headline', message: 'Peek notification message' },
			};

			peekNotificationHandler = notificationContext.peek('positive', peekOptions);
			layoutElement = peekNotificationHandler.element.querySelector('umb-notification-layout-default');
		});

		it('it sets notification color', () => {
			expect(peekNotificationHandler?.color).to.equal('positive');
		});

		it('should set peek data on the notification element', () => {
			const data = layoutElement.data;
			expect(data.headline).to.equal('Peek notification headline');
			expect(data.message).to.equal('Peek notification message');
		});

		it('it sets duration to 6000 ms', () => {
			expect(peekNotificationHandler?.duration).to.equal(6000);
		});
	});

	describe('stay', () => {
		let stayNotificationHandler: UmbNotificationHandler | undefined = undefined;
		let layoutElement: any;

		beforeEach(async () => {
			const stayOptions = {
				data: { headline: 'Stay notification headline', message: 'Stay notification message' },
			};

			stayNotificationHandler = notificationContext.stay('danger', stayOptions);
			layoutElement = stayNotificationHandler.element.querySelector('umb-notification-layout-default');
		});

		it('it sets notification color', () => {
			expect(stayNotificationHandler?.color).to.equal('danger');
		});

		it('should set stay data on the notification element', () => {
			const data = layoutElement?.data;
			expect(data.headline).to.equal('Stay notification headline');
			expect(data.message).to.equal('Stay notification message');
		});

		it('it sets the duration to null', () => {
			expect(stayNotificationHandler?.duration).to.equal(null);
		});
	});
});
