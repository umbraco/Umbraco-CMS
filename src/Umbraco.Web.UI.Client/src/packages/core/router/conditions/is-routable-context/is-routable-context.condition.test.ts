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

		it('should not permit the condition when route context is blocked by boundary', (done) => {
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

			// The onChange callback is not called when the condition remains false,
			// so we need to wait and check manually
			setTimeout(() => {
				expect(callbackCount).to.equal(0);
				expect(condition.permitted).to.be.false;
				done();
			}, 50);
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

		it('should not permit the condition when no route context exists', (done) => {
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

			// The onChange callback is not called when the condition remains false,
			// so we need to wait and check manually
			setTimeout(() => {
				expect(callbackCount).to.equal(0);
				expect(condition.permitted).to.be.false;
				done();
			}, 50);
		});
	});
});
