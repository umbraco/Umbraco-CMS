import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBoundary } from '@umbraco-cms/backoffice/context-api';
import { UmbIsRoutableContextCondition } from './is-routable-context.condition.js';
import { UMB_IS_ROUTABLE_CONTEXT_CONDITION_ALIAS } from './constants.js';
import { UMB_ROUTE_CONTEXT } from '../../route/route.context.js';

import '@umbraco-cms/backoffice/router';

@customElement('test-routable-condition-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbIsRoutableContextCondition', () => {
	describe('match: true (default)', () => {
		describe('with route context available', () => {
			let hostElement: UmbTestControllerHostElement;
			let condition: UmbIsRoutableContextCondition;

			beforeEach(async () => {
				await fixture(html`
					<umb-router-slot id="router">
						<test-routable-condition-host id="host"></test-routable-condition-host>
					</umb-router-slot>
				`);

				hostElement = document.querySelector<UmbTestControllerHostElement>('#host')!;
			});

			afterEach(() => {
				condition?.hostDisconnected();
			});

			it('should permit the condition when route context is available', (done) => {
				let callbackCount = 0;

				condition = new UmbIsRoutableContextCondition(hostElement, {
					host: hostElement,
					config: {
						alias: UMB_IS_ROUTABLE_CONTEXT_CONDITION_ALIAS,
					},
					onChange: () => {
						callbackCount++;
						if (callbackCount === 1) {
							expect(condition.permitted).to.be.true;
							done();
						}
					},
				});
			});
		});

		describe('with context boundary blocking route context', () => {
			let hostElement: UmbTestControllerHostElement;
			let boundaryElement: HTMLDivElement;
			let condition: UmbIsRoutableContextCondition;

			beforeEach(async () => {
				await fixture(html`
					<umb-router-slot id="router">
						<div id="boundary">
							<test-routable-condition-host id="host"></test-routable-condition-host>
						</div>
					</umb-router-slot>
				`);

				boundaryElement = document.querySelector<HTMLDivElement>('#boundary')!;
				hostElement = document.querySelector<UmbTestControllerHostElement>('#host')!;

				// Create a context boundary that stops the route context from propagating
				new UmbContextBoundary(boundaryElement, UMB_ROUTE_CONTEXT).hostConnected();
			});

			afterEach(() => {
				condition?.hostDisconnected();
			});

			it('should not permit the condition when route context is blocked by boundary', async () => {
				let callbackCount = 0;

				condition = new UmbIsRoutableContextCondition(hostElement, {
					host: hostElement,
					config: {
						alias: UMB_IS_ROUTABLE_CONTEXT_CONDITION_ALIAS,
					},
					onChange: () => {
						callbackCount++;
					},
				});

				// The absence of the context is only known once the condition is rejected,
				// so we wait two frames (the lookup is rejected in the first) and check manually
				await new Promise((resolve) => {
					requestAnimationFrame(() => requestAnimationFrame(resolve));
				});

				expect(callbackCount).to.equal(1);
				expect(condition.permitted).to.be.false;
			});
		});

		describe('without route context directly available', () => {
			let hostElement: UmbTestControllerHostElement;
			let condition: UmbIsRoutableContextCondition;

			beforeEach(async () => {
				await fixture(html`
					<div>
						<test-routable-condition-host id="host"></test-routable-condition-host>
					</div>
				`);

				hostElement = document.querySelector<UmbTestControllerHostElement>('#host')!;
			});

			afterEach(() => {
				condition?.hostDisconnected();
			});

			it('should not permit the condition when no route context exists', async () => {
				let callbackCount = 0;

				condition = new UmbIsRoutableContextCondition(hostElement, {
					host: hostElement,
					config: {
						alias: UMB_IS_ROUTABLE_CONTEXT_CONDITION_ALIAS,
					},
					onChange: () => {
						callbackCount++;
					},
				});

				// The absence of the context is only known once the condition is rejected,
				// so we wait two frames (the lookup is rejected in the first) and check manually
				await new Promise((resolve) => {
					requestAnimationFrame(() => requestAnimationFrame(resolve));
				});

				expect(callbackCount).to.equal(1);
				expect(condition.permitted).to.be.false;
			});
		});
	});

	describe('match: false', () => {
		describe('with route context available', () => {
			let hostElement: UmbTestControllerHostElement;
			let condition: UmbIsRoutableContextCondition;

			beforeEach(async () => {
				await fixture(html`
					<umb-router-slot id="router">
						<test-routable-condition-host id="host"></test-routable-condition-host>
					</umb-router-slot>
				`);

				hostElement = document.querySelector<UmbTestControllerHostElement>('#host')!;
			});

			afterEach(() => {
				condition?.hostDisconnected();
			});

			it('should not permit the condition when route context is available', (done) => {
				let callbackCount = 0;

				condition = new UmbIsRoutableContextCondition(hostElement, {
					host: hostElement,
					config: {
						alias: UMB_IS_ROUTABLE_CONTEXT_CONDITION_ALIAS,
						match: false,
					},
					onChange: () => {
						callbackCount++;
						// Only callback: permitted = false (when context is found)
						if (callbackCount === 1) {
							expect(condition.permitted).to.be.false;
							done();
						}
					},
				});
			});
		});

		describe('with context boundary blocking route context', () => {
			let hostElement: UmbTestControllerHostElement;
			let boundaryElement: HTMLDivElement;
			let condition: UmbIsRoutableContextCondition;

			beforeEach(async () => {
				await fixture(html`
					<umb-router-slot id="router">
						<div id="boundary">
							<test-routable-condition-host id="host"></test-routable-condition-host>
						</div>
					</umb-router-slot>
				`);

				boundaryElement = document.querySelector<HTMLDivElement>('#boundary')!;
				hostElement = document.querySelector<UmbTestControllerHostElement>('#host')!;

				// Create a context boundary that stops the route context from propagating
				new UmbContextBoundary(boundaryElement, UMB_ROUTE_CONTEXT).hostConnected();
			});

			afterEach(() => {
				condition?.hostDisconnected();
			});

			it('should permit the condition when route context is blocked by boundary', async () => {
				let callbackCount = 0;

				condition = new UmbIsRoutableContextCondition(hostElement, {
					host: hostElement,
					config: {
						alias: UMB_IS_ROUTABLE_CONTEXT_CONDITION_ALIAS,
						match: false,
					},
					onChange: () => {
						callbackCount++;
					},
				});

				// Wait and verify it remains permitted (only initial callback, no second callback to set false)
				await new Promise((resolve) => {
					requestAnimationFrame(() => requestAnimationFrame(resolve));
				});

				expect(callbackCount).to.equal(1);
				expect(condition.permitted).to.be.true;
			});
		});

		describe('without route context directly available', () => {
			let hostElement: UmbTestControllerHostElement;
			let condition: UmbIsRoutableContextCondition;

			beforeEach(async () => {
				await fixture(html`
					<div>
						<test-routable-condition-host id="host"></test-routable-condition-host>
					</div>
				`);

				hostElement = document.querySelector<UmbTestControllerHostElement>('#host')!;
			});

			afterEach(() => {
				condition?.hostDisconnected();
			});

			it('should permit the condition when no route context exists', async () => {
				let callbackCount = 0;

				condition = new UmbIsRoutableContextCondition(hostElement, {
					host: hostElement,
					config: {
						alias: UMB_IS_ROUTABLE_CONTEXT_CONDITION_ALIAS,
						match: false,
					},
					onChange: () => {
						callbackCount++;
					},
				});

				// Wait and verify it remains permitted (only initial callback, no second callback to set false)
				await new Promise((resolve) => {
					requestAnimationFrame(() => requestAnimationFrame(resolve));
				});

				expect(callbackCount).to.equal(1);
				expect(condition.permitted).to.be.true;
			});
		});
	});
});
