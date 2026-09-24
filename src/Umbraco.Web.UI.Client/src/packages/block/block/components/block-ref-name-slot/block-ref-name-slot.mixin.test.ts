import { UmbBlockRefNameSlotMixin } from './block-ref-name-slot.mixin.js';
import type { UmbBlockLabelUfmValueType } from '../../types.js';
import { aTimeout, elementUpdated, expect, fixture, html } from '@open-wc/testing';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const TEST_VALUE: UmbBlockLabelUfmValueType = { $index: 0 };

@customElement('umb-test-block-ref-name-slot')
class UmbTestBlockRefNameSlotElement extends UmbBlockRefNameSlotMixin(UmbLitElement) {
	@property({ attribute: false })
	slotName?: string;

	override render() {
		return html`<div id="info">${this.renderNameSlot(TEST_VALUE, this.slotName)}</div>`;
	}
}

// Deprecation warnings are only logged once per subclass, so the warning test uses its own element.
@customElement('umb-test-block-ref-name-slot-warn')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestBlockRefNameSlotWarnElement extends UmbBlockRefNameSlotMixin(UmbLitElement) {
	override render() {
		return html`${this.renderNameSlot(TEST_VALUE)}`;
	}
}

/**
 * Monkey-patches `console.warn` to record calls instead of printing them — this project does not use
 * sinon, so calls are asserted against a plain recorded-call array. Call `restore()` in `afterEach`.
 */
function stubConsoleWarn() {
	const original = console.warn;
	const calls: Array<Array<unknown>> = [];
	console.warn = (...args: Array<unknown>) => {
		calls.push(args);
	};
	return {
		calls,
		restore: () => {
			console.warn = original;
		},
	};
}

describe('UmbBlockRefNameSlotMixin', () => {
	let element: UmbTestBlockRefNameSlotElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-test-block-ref-name-slot></umb-test-block-ref-name-slot>`);
	});

	it('renders no fallback when nothing is slotted and label is undefined', async () => {
		await elementUpdated(element);
		expect(element.shadowRoot!.querySelector('umb-ufm-render')).to.not.exist;
	});

	it('renders the label as a fallback umb-ufm-render when nothing is slotted', async () => {
		element.label = 'Hello world';
		await elementUpdated(element);

		const fallback = element.shadowRoot!.querySelector('umb-ufm-render');
		expect(fallback).to.exist;
		expect(fallback!.id).to.equal('name');
		expect((fallback as unknown as { markdown?: string }).markdown).to.equal('Hello world');
	});

	it('hides the fallback once content is slotted into the name slot', async () => {
		element.label = 'Hello world';
		await elementUpdated(element);
		expect(element.shadowRoot!.querySelector('umb-ufm-render')).to.exist;

		const projected = document.createElement('span');
		projected.slot = 'name';
		element.appendChild(projected);
		await aTimeout(0);

		expect(element.shadowRoot!.querySelector('umb-ufm-render')).to.not.exist;
	});

	it('shows the fallback again once the slotted content is removed', async () => {
		element.label = 'Hello world';
		const projected = document.createElement('span');
		projected.slot = 'name';
		element.appendChild(projected);
		await aTimeout(0);
		expect(element.shadowRoot!.querySelector('umb-ufm-render')).to.not.exist;

		element.removeChild(projected);
		await aTimeout(0);

		expect(element.shadowRoot!.querySelector('umb-ufm-render')).to.exist;
	});

	it('forwards the name slot and the fallback into a nested slot when slotName is given', async () => {
		element.slotName = 'name';
		element.label = 'Hello world';
		await elementUpdated(element);

		const nameSlot = element.shadowRoot!.querySelector('slot[name="name"]')!;
		expect(nameSlot.getAttribute('slot')).to.equal('name');

		const fallback = element.shadowRoot!.querySelector('umb-ufm-render')!;
		expect(fallback.getAttribute('slot')).to.equal('name');
		expect(fallback.hasAttribute('id')).to.be.false;
	});

	describe('label deprecation warning', () => {
		let warnStub: ReturnType<typeof stubConsoleWarn>;
		const created: Array<HTMLElement> = [];

		beforeEach(() => {
			warnStub = stubConsoleWarn();
		});

		afterEach(() => {
			warnStub.restore();
			created.splice(0).forEach((el) => el.remove());
		});

		it('warns about the deprecated label property only once per subclass', () => {
			const first = document.createElement('umb-test-block-ref-name-slot-warn') as HTMLElement & {
				label?: string;
			};
			document.body.appendChild(first);
			created.push(first);
			first.label = 'one';

			const second = document.createElement('umb-test-block-ref-name-slot-warn') as HTMLElement & {
				label?: string;
			};
			document.body.appendChild(second);
			created.push(second);
			second.label = 'two';

			const warnCalls = warnStub.calls.filter((args) =>
				String(args[0]).includes('umb-test-block-ref-name-slot-warn.label'),
			);
			expect(warnCalls.length).to.equal(1);
			expect(String(warnCalls[0][0])).to.include('20.0.0');
		});
	});
});
