import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbCurrentUserContext, UmbCurrentUserStore } from '@umbraco-cms/backoffice/current-user';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbFallbackUserPermissionCondition } from './fallback-user-permission.condition';
import { UMB_USER_PERMISSION_DOCUMENT_READ } from '@umbraco-cms/backoffice/document';
import { UMB_FALLBACK_USER_PERMISSION_CONDITION_ALIAS } from './constants';

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	currentUserContext = new UmbCurrentUserContext(this);

	constructor() {
		super();
		new UmbNotificationContext(this);
		new UmbCurrentUserStore(this);
	}

	async init() {
		await this.currentUserContext.load();
	}
}

describe('UmbFallbackUserPermissionCondition', () => {
	let hostElement: UmbTestControllerHostElement;
	let condition: UmbFallbackUserPermissionCondition;

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);
		await hostElement.init();
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('Fallback Permissions', () => {
		it('should permit the condition when allOf is satisfied', (done) => {
			let callbackCount = 0;

			// We expect to find the read permission in the fallback permissions
			condition = new UmbFallbackUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_FALLBACK_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ],
				},
				onChange: () => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(condition.permitted).to.be.true;
						condition.hostDisconnected();
						done();
					}
				},
			});
		});

		it('should forbid the condition when allOf is not satisfied', (done) => {
			let callbackCount = 0;

			// We expect to find the read permission in the fallback permissions
			condition = new UmbFallbackUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_FALLBACK_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ, 'non-existing-permission'],
				},
				onChange: () => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(condition.permitted).to.be.false;
						condition.hostDisconnected();
						done();
					}
				},
			});

			// The onChange callback is not called when the condition is false, so we need to wait and check manually
			setTimeout(() => {
				expect(condition.permitted).to.be.false;
				condition.hostDisconnected();
				done();
			}, 200);
		});

		it('should permit the condition when oneOf is satisfied', (done) => {
			let callbackCount = 0;

			// We expect to find the read permission in the fallback permissions
			condition = new UmbFallbackUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_FALLBACK_USER_PERMISSION_CONDITION_ALIAS,
					oneOf: [UMB_USER_PERMISSION_DOCUMENT_READ, 'non-existing-permission'],
				},
				onChange: () => {
					/* The onChange callback is not called when the condition is false, so this should never be called
					But in case it is, we want to fail the test */
					callbackCount++;
					if (callbackCount === 1) {
						expect(condition.permitted).to.be.true;
						condition.hostDisconnected();
						done();
					}
				},
			});
		});

		it('should forbid the condition when oneOf is not satisfied', (done) => {
			let callbackCount = 0;

			// We expect to find the read permission in the fallback permissions
			condition = new UmbFallbackUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_FALLBACK_USER_PERMISSION_CONDITION_ALIAS,
					oneOf: ['non-existing-permission', 'another-non-existing-permission'],
				},
				onChange: () => {
					/* The onChange callback is not called when the condition is false, so this should never be called
					But in case it is, we want to fail the test */
					callbackCount++;
					if (callbackCount === 1) {
						expect(condition.permitted).to.be.false;
						condition.hostDisconnected();
						done();
					}
				},
			});

			// The onChange callback is not called when the condition is false, so we need to wait and check manually
			setTimeout(() => {
				expect(condition.permitted).to.be.false;
				condition.hostDisconnected();
				done();
			}, 200);
		});
	});
});
