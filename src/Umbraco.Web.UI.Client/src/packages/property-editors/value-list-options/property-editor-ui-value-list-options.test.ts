import { UmbPropertyEditorUIValueListOptionsElement } from './property-editor-ui-value-list-options.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';

describe('UmbPropertyEditorUIValueListOptionsElement', () => {
	let element: UmbPropertyEditorUIValueListOptionsElement;

	async function setValue(value: unknown) {
		element.value = value as typeof element.value;
		await element.updateComplete;
		await getInput().updateComplete;
	}

	function getInput() {
		return element.shadowRoot!.querySelector('umb-input-value-list-options')!;
	}

	function getRows() {
		return Array.from(getInput().shadowRoot!.querySelectorAll('.option'));
	}

	function getField(row: Element, field: 'value' | 'label') {
		return row.querySelector(`uui-input.${field}`) as UUIInputElement;
	}

	async function type(row: Element, field: 'value' | 'label', text: string) {
		const input = getField(row, field);
		input.value = text;
		input.dispatchEvent(new CustomEvent('input', { bubbles: true, composed: true }));
		await getInput().updateComplete;
		await element.updateComplete;
	}

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-value-list-options></umb-property-editor-ui-value-list-options>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIValueListOptionsElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}

	it('shows a row with a value and a label for each option', async () => {
		await setValue([{ value: 'a' }, { value: 'b', label: 'Bee' }]);

		const rows = getRows();
		expect(rows).to.have.lengthOf(2);
		expect(rows.map((row) => getField(row, 'value').value)).to.deep.equal(['a', 'b']);
		expect(rows.map((row) => getField(row, 'label').value)).to.deep.equal(['', 'Bee']);
	});

	it('emits the edited label next to the value', async () => {
		await setValue([{ value: 'a' }, { value: 'b', label: 'Bee' }]);

		await type(getRows()[0], 'label', 'Ay');

		expect(element.value).to.deep.equal([
			{ value: 'a', label: 'Ay' },
			{ value: 'b', label: 'Bee' },
		]);
	});

	it('omits the label when it is cleared', async () => {
		await setValue([{ value: 'a' }, { value: 'b', label: 'Bee' }]);

		await type(getRows()[1], 'label', '');

		expect(element.value).to.deep.equal([{ value: 'a' }, { value: 'b' }]);
	});

	it('treats a label of only whitespace as no label', async () => {
		await setValue([{ value: 'a' }]);

		await type(getRows()[0], 'label', '   ');

		expect(element.value).to.deep.equal([{ value: 'a' }]);
	});

	it('marks a duplicate value as invalid', async () => {
		await setValue([{ value: 'a' }, { value: 'b' }, { value: 'c' }]);

		await type(getRows()[2], 'value', 'a');

		expect(getInput().checkValidity()).to.be.false;
		expect(getRows().map((row) => getField(row, 'value').error)).to.deep.equal([true, false, true]);
	});

	it('adds an option', async () => {
		await setValue([{ value: 'a' }]);

		(getInput().shadowRoot!.querySelector('#action') as HTMLElement).click();
		await getInput().updateComplete;
		await element.updateComplete;
		await getInput().updateComplete;

		expect(getRows()).to.have.lengthOf(2);
	});

	it('removes an option', async () => {
		await setValue([{ value: 'a' }, { value: 'b' }]);

		(getRows()[0].querySelector('.delete') as HTMLElement).click();
		await getInput().updateComplete;
		await element.updateComplete;

		expect(element.value).to.deep.equal([{ value: 'b' }]);
	});

	for (const mode of ['readonly', 'disabled'] as const) {
		it(`hides adding and removing options when ${mode}`, async () => {
			element[mode] = true;
			await setValue([{ value: 'a' }]);

			expect(getInput().shadowRoot!.querySelector('#action')).to.equal(null);
			expect(getRows()[0].querySelector('.delete')).to.equal(null);
		});

		it(`shows a disabled add button when ${mode} and there are no options`, async () => {
			element[mode] = true;
			await setValue([]);

			const add = getInput().shadowRoot!.querySelector('#action') as HTMLElement & { disabled: boolean };
			expect(add.disabled).to.be.true;
		});
	}
});
