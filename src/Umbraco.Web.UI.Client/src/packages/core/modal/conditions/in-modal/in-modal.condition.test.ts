import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UMB_MODAL_CONTEXT } from '../../context/modal.context-token.js';
import { UmbInModalCondition } from './in-modal.condition.js';
import { UMB_IN_MODAL_CONDITION_ALIAS } from './constants.js';

@customElement('test-controller-host-in-modal')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

@customElement('test-controller-child-in-modal')
class UmbTestControllerChildElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbInModalCondition', () => {
	let hostElement: UmbTestControllerHostElement;
	let childElement: UmbTestControllerChildElement;
	let condition: UmbInModalCondition;

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		childElement = new UmbTestControllerChildElement();
		hostElement.appendChild(childElement);
		document.body.appendChild(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('match: true (default)', () => {
		it('should return true when modal context is available', (done) => {
			// Create a mock modal context with required getHostElement method
			const mockModalContext = {
				getHostElement: () => hostElement,
			} as any;

			const provider = new UmbContextProvider(hostElement, UMB_MODAL_CONTEXT, mockModalContext);
			provider.hostConnected();

			let callbackCount = 0;
			condition = new UmbInModalCondition(childElement, {
				host: childElement,
				config: {
					alias: UMB_IN_MODAL_CONDITION_ALIAS,
					// match defaults to true
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

		it('should return false when modal context is not available', async () => {
			let callbackCount = 0;
			condition = new UmbInModalCondition(childElement, {
				host: childElement,
				config: {
					alias: UMB_IN_MODAL_CONDITION_ALIAS,
					match: true,
				},
				onChange: () => {
					callbackCount++;
				},
			});

			await new Promise((resolve) => {
				requestAnimationFrame(() => {
					expect(condition.permitted).to.be.false;
					resolve(true);
				});
			});

			expect(callbackCount).to.equal(0);
			condition.hostDisconnected();
		});
	});

	describe('match: false', () => {
		it('should return true when modal context is not available', (done) => {
			let callbackCount = 0;
			let permittedValue: boolean | undefined;

			condition = new UmbInModalCondition(childElement, {
				host: childElement,
				config: {
					alias: UMB_IN_MODAL_CONDITION_ALIAS,
					match: false,
				},
				onChange: (permitted) => {
					callbackCount++;
					permittedValue = permitted;
					if (callbackCount === 1) {
						expect(permittedValue).to.be.true;
						setTimeout(() => {
							condition.hostDisconnected();
							done();
						}, 0);
					}
				},
			});
		});

		it('should return false when modal context is available', (done) => {
			// Create a mock modal context with required getHostElement method
			const mockModalContext = {
				getHostElement: () => hostElement,
			} as any;

			const provider = new UmbContextProvider(hostElement, UMB_MODAL_CONTEXT, mockModalContext);
			provider.hostConnected();

			let callbackCount = 0;
			condition = new UmbInModalCondition(childElement, {
				host: childElement,
				config: {
					alias: UMB_IN_MODAL_CONDITION_ALIAS,
					match: false,
				},
				onChange: () => {
					callbackCount++;
					// First callback: permitted = true (from constructor, since match is false)
					// Second callback: permitted = false (when context is found)
					if (callbackCount === 2) {
						expect(condition.permitted).to.be.false;
						condition.hostDisconnected();
						done();
					}
				},
			});
		});
	});
});
