import { UmbEntityBulkActionProgressModalElement } from './entity-bulk-action-progress-modal.element.js';
import type {
	UmbEntityBulkActionProgressModalData,
	UmbEntityBulkActionProgressModalValue,
} from './entity-bulk-action-progress-modal.token.js';
import { expect, fixture, html } from '@open-wc/testing';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

/**
 * Builds a minimal modal-context stand-in. The element only consumes `value` (observed) from the
 * context, so a state-backed observable is all that is required to drive the counter.
 * @param value Initial modal value.
 */
function createValueContext(value: UmbEntityBulkActionProgressModalValue) {
	const state = new UmbObjectState<UmbEntityBulkActionProgressModalValue>(value);
	return { value: state.asObservable() };
}

describe('UmbEntityBulkActionProgressModalElement', () => {
	describe('determinate mode', () => {
		let element: UmbEntityBulkActionProgressModalElement;

		beforeEach(async () => {
			element = await fixture(html`<umb-entity-bulk-action-progress-modal></umb-entity-bulk-action-progress-modal>`);
			element.modalContext = createValueContext({ total: 5, completed: 2 }) as any;
			element.data = { headline: 'Deletion in progress', mode: 'determinate' } as UmbEntityBulkActionProgressModalData;
			await element.updateComplete;
		});

		it('renders a determinate progress bar reflecting the completed count', () => {
			const bar = element.shadowRoot!.querySelector('uui-loader-bar');
			expect(bar).to.not.equal(null);
			expect((bar as any).progress).to.equal(40); // 2 / 5
		});

		it('renders the "X / Y" counter text', () => {
			const counter = element.shadowRoot!.querySelector('#progress span');
			expect(counter).to.not.equal(null);
			expect(counter!.textContent!.replace(/\s+/g, ' ').trim()).to.equal('2 / 5');
		});

		it('renders a Cancel button', () => {
			expect(element.shadowRoot!.querySelector('uui-button[slot="actions"]')).to.not.equal(null);
		});
	});

	describe('indeterminate mode', () => {
		let element: UmbEntityBulkActionProgressModalElement;

		beforeEach(async () => {
			element = await fixture(html`<umb-entity-bulk-action-progress-modal></umb-entity-bulk-action-progress-modal>`);
			element.data = { headline: 'Move in progress', mode: 'indeterminate' } as UmbEntityBulkActionProgressModalData;
			await element.updateComplete;
		});

		it('renders a looped (indeterminate) progress bar', () => {
			const bar = element.shadowRoot!.querySelector('uui-loader-bar');
			expect(bar).to.not.equal(null);
			expect((bar as any).progress).to.equal(0); // 0 => looped animation
		});

		it('does not render a counter', () => {
			expect(element.shadowRoot!.querySelector('#progress')).to.equal(null);
		});

		it('does not render a Cancel button', () => {
			expect(element.shadowRoot!.querySelector('uui-button[slot="actions"]')).to.equal(null);
		});
	});
});
