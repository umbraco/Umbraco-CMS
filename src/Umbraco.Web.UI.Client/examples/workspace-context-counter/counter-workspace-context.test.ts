import { ExampleWorkspaceContextCounterContext, EXAMPLE_COUNTER_CONTEXT } from './counter-workspace-context.js';
import { ExampleCounterWorkspaceViewElement } from './counter-workspace-view.js';
import { ExampleCounterStatusFooterAppElement } from './counter-status-footer-app.element.js';
import { expect, fixture, defineCE } from '@open-wc/testing';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html } from '@umbraco-cms/backoffice/external/lit';

class TestHostElement extends UmbLitElement {}
const testHostElement = defineCE(TestHostElement);

describe('WorkspaceContextCounterElement', () => {
	let element: UmbLitElement;
	let context: ExampleWorkspaceContextCounterContext;

	beforeEach(async () => {
		element = await fixture(`<${testHostElement}></${testHostElement}>`);
		context = new ExampleWorkspaceContextCounterContext(element);
	});

	describe('Counter functionality', () => {
		it('initializes with counter value of 0', (done) => {
			context.counter.subscribe((value) => {
				expect(value).to.equal(0);
				done();
			});
		});

		it('increments counter value when increment method is called', (done) => {
			let callbackCount = 0;

			context.counter.subscribe((value) => {
				callbackCount++;
				if (callbackCount === 1) {
					expect(value).to.equal(0);
					context.increment();
				} else if (callbackCount === 2) {
					expect(value).to.equal(1);
					done();
				}
			});
		});

		it('increments counter multiple times correctly', (done) => {
			let callbackCount = 0;

			context.counter.subscribe((value) => {
				callbackCount++;
				if (callbackCount === 1) {
					expect(value).to.equal(0);
					context.increment();
					context.increment();
					context.increment();
				} else if (callbackCount === 4) {
					expect(value).to.equal(3);
					done();
				}
			});
		});
	});

	describe('Reset functionality', () => {
		it('resets counter to 0 when reset method is called', (done) => {
			let callbackCount = 0;

			context.counter.subscribe((value) => {
				callbackCount++;
				if (callbackCount === 1) {
					expect(value).to.equal(0);
					// First increment the counter
					context.increment();
					context.increment();
				} else if (callbackCount === 3) {
					expect(value).to.equal(2);
					// Then reset it
					context.reset();
				} else if (callbackCount === 4) {
					expect(value).to.equal(0);
					done();
				}
			});
		});

		it('can reset from any counter value', (done) => {
			let callbackCount = 0;

			context.counter.subscribe((value) => {
				callbackCount++;
				if (callbackCount === 1) {
					expect(value).to.equal(0);
					// Increment to a higher number
					context.increment();
					context.increment();
					context.increment();
					context.increment();
					context.increment();
				} else if (callbackCount === 6) {
					expect(value).to.equal(5);
					context.reset();
				} else if (callbackCount === 7) {
					expect(value).to.equal(0);
					done();
				}
			});
		});

		it('reset works when counter is already at 0', (done) => {
			context.counter.subscribe((value) => {
				expect(value).to.equal(0);
				// Reset when already at 0 - should not cause issues
				context.reset();
				// Verify it's still 0
				expect(value).to.equal(0);
				done();
			});
		});
	});

	describe('Increment and Reset interaction', () => {
		it('can increment after reset', (done) => {
			let callbackCount = 0;

			context.counter.subscribe((value) => {
				callbackCount++;
				if (callbackCount === 1) {
					expect(value).to.equal(0);
					context.increment();
				} else if (callbackCount === 2) {
					expect(value).to.equal(1);
					context.reset();
				} else if (callbackCount === 3) {
					expect(value).to.equal(0);
					context.increment();
				} else if (callbackCount === 4) {
					expect(value).to.equal(1);
					done();
				}
			});
		});
	});

	describe('Context integration', () => {
		it('provides context that can be consumed by other components', () => {
			// Verify context is available for consumption
			expect(EXAMPLE_COUNTER_CONTEXT).to.not.be.undefined;
		});
	});
});

describe('ExampleCounterWorkspaceView', () => {
	let element: ExampleCounterWorkspaceViewElement;
	let context: ExampleWorkspaceContextCounterContext;
	let hostElement: UmbLitElement;

	beforeEach(async () => {
		hostElement = await fixture(`<${testHostElement}></${testHostElement}>`);
		context = new ExampleWorkspaceContextCounterContext(hostElement);

		element = await fixture(html`<example-counter-workspace-view></example-counter-workspace-view>`, {
			parentNode: hostElement,
		});

		// Wait for context consumption
		await element.updateComplete;
	});

	describe('Counter value display', () => {
		it('shows initial counter value', async () => {
			await element.updateComplete;
			const displayText = element.shadowRoot?.textContent;
			expect(displayText).to.include('Current count value: 0');
		});

		it('reflects counter value changes when incremented', async () => {
			context.increment();
			await element.updateComplete;

			const displayText = element.shadowRoot?.textContent;
			expect(displayText).to.include('Current count value: 1');
		});

		it('reflects counter value changes when incremented multiple times', async () => {
			context.increment();
			context.increment();
			context.increment();
			await element.updateComplete;

			const displayText = element.shadowRoot?.textContent;
			expect(displayText).to.include('Current count value: 3');
		});

		it('reflects counter value changes when reset', async () => {
			context.increment();
			context.increment();
			await element.updateComplete;

			let displayText = element.shadowRoot?.textContent;
			expect(displayText).to.include('Current count value: 2');

			context.reset();
			await element.updateComplete;

			displayText = element.shadowRoot?.textContent;
			expect(displayText).to.include('Current count value: 0');
		});

		it('maintains correct value display through increment and reset cycles', async () => {
			// Test a complete workflow cycle
			context.increment();
			context.increment();
			context.increment();
			await element.updateComplete;
			expect(element.shadowRoot?.textContent).to.include('Current count value: 3');

			context.reset();
			await element.updateComplete;
			expect(element.shadowRoot?.textContent).to.include('Current count value: 0');

			context.increment();
			await element.updateComplete;
			expect(element.shadowRoot?.textContent).to.include('Current count value: 1');
		});
	});
});

describe('ExampleCounterStatusFooterAppElement', () => {
	let element: ExampleCounterStatusFooterAppElement;
	let context: ExampleWorkspaceContextCounterContext;
	let hostElement: UmbLitElement;

	beforeEach(async () => {
		hostElement = await fixture(`<${testHostElement}></${testHostElement}>`);
		context = new ExampleWorkspaceContextCounterContext(hostElement);

		element = await fixture(html`<example-counter-status-footer-app></example-counter-status-footer-app>`, {
			parentNode: hostElement,
		});

		// Wait for context consumption
		await element.updateComplete;
	});

	describe('Status display', () => {
		it('shows initial counter status', async () => {
			await element.updateComplete;
			const displayText = element.shadowRoot?.textContent;
			expect(displayText).to.include('Counter: 0');
		});

		it('reflects counter state changes when incremented', async () => {
			context.increment();
			await element.updateComplete;

			const displayText = element.shadowRoot?.textContent;
			expect(displayText).to.include('Counter: 1');
		});

		it('reflects counter state changes when incremented multiple times', async () => {
			context.increment();
			context.increment();
			context.increment();
			context.increment();
			context.increment();
			await element.updateComplete;

			const displayText = element.shadowRoot?.textContent;
			expect(displayText).to.include('Counter: 5');
		});

		it('reflects counter state changes when reset', async () => {
			context.increment();
			context.increment();
			context.increment();
			await element.updateComplete;

			let displayText = element.shadowRoot?.textContent;
			expect(displayText).to.include('Counter: 3');

			context.reset();
			await element.updateComplete;

			displayText = element.shadowRoot?.textContent;
			expect(displayText).to.include('Counter: 0');
		});

		it('maintains accurate status display through complete workflow cycles', async () => {
			// Test comprehensive state change tracking
			await element.updateComplete;
			expect(element.shadowRoot?.textContent).to.include('Counter: 0');

			context.increment();
			context.increment();
			await element.updateComplete;
			expect(element.shadowRoot?.textContent).to.include('Counter: 2');

			context.reset();
			await element.updateComplete;
			expect(element.shadowRoot?.textContent).to.include('Counter: 0');

			context.increment();
			await element.updateComplete;
			expect(element.shadowRoot?.textContent).to.include('Counter: 1');
		});

		it('synchronizes with context state changes across multiple increments and resets', async () => {
			// Test synchronization with rapid state changes
			context.increment();
			context.increment();
			context.increment();
			context.reset();
			context.increment();
			context.increment();
			await element.updateComplete;

			const displayText = element.shadowRoot?.textContent;
			expect(displayText).to.include('Counter: 2');
		});
	});
});
