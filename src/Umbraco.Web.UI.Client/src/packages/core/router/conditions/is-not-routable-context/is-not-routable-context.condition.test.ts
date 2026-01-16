import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBoundary } from '@umbraco-cms/backoffice/context-api';
import { UmbIsNotRoutableContextCondition } from './is-not-routable-context.condition.js';
import { UMB_IS_NOT_ROUTABLE_CONTEXT_CONDITION_ALIAS } from './constants.js';
import { UMB_ROUTE_CONTEXT } from '../../route/route.context.js';

import '@umbraco-cms/backoffice/router';

@customElement('test-not-routable-condition-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbIsNotRoutableContextCondition', () => {
	describe('with route context available', () => {
		let hostElement: UmbTestControllerHostElement;
		let condition: UmbIsNotRoutableContextCondition;

		beforeEach(async () => {
			await fixture(html`
				<umb-router-slot id="router">
					<test-not-routable-condition-host id="host"></test-not-routable-condition-host>
				</umb-router-slot>
			`);

			hostElement = document.querySelector<UmbTestControllerHostElement>('#host')!;
		});

		afterEach(() => {
			condition?.hostDisconnected();
		});

		it('should not permit the condition when route context is available', (done) => {
			let callbackCount = 0;

			condition = new UmbIsNotRoutableContextCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_IS_NOT_ROUTABLE_CONTEXT_CONDITION_ALIAS,
				},
				onChange: () => {
					callbackCount++;
					// Second callback is when context is found (false)
					if (callbackCount === 2) {
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
		let condition: UmbIsNotRoutableContextCondition;

		beforeEach(async () => {
			await fixture(html`
				<umb-router-slot id="router">
					<div id="boundary">
						<test-not-routable-condition-host id="host"></test-not-routable-condition-host>
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

		it('should permit the condition when route context is blocked by boundary', (done) => {
			let callbackCount = 0;

			condition = new UmbIsNotRoutableContextCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_IS_NOT_ROUTABLE_CONTEXT_CONDITION_ALIAS,
				},
				onChange: () => {
					callbackCount++;
				},
			});

			// Wait and verify it remains permitted (only initial callback, no second callback to set false)
			setTimeout(() => {
				expect(callbackCount).to.equal(1);
				expect(condition.permitted).to.be.true;
				done();
			}, 50);
		});
	});

	describe('without route context directly available', () => {
		let hostElement: UmbTestControllerHostElement;
		let condition: UmbIsNotRoutableContextCondition;

		beforeEach(async () => {
			await fixture(html`
				<div>
					<test-not-routable-condition-host id="host"></test-not-routable-condition-host>
				</div>
			`);

			hostElement = document.querySelector<UmbTestControllerHostElement>('#host')!;
		});

		afterEach(() => {
			condition?.hostDisconnected();
		});

		it('should permit the condition when no route context exists', (done) => {
			let callbackCount = 0;

			condition = new UmbIsNotRoutableContextCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_IS_NOT_ROUTABLE_CONTEXT_CONDITION_ALIAS,
				},
				onChange: () => {
					callbackCount++;
				},
			});

			// Wait and verify it remains permitted (only initial callback, no second callback to set false)
			setTimeout(() => {
				expect(callbackCount).to.equal(1);
				expect(condition.permitted).to.be.true;
				done();
			}, 50);
		});
	});
});
