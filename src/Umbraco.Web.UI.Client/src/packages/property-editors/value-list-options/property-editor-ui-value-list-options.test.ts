import { UmbPropertyEditorUIValueListOptionsElement } from './property-editor-ui-value-list-options.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import type { UmbInputValueListOptionElement } from './input-value-list-option.element.js';

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
		return Array.from(getInput().shadowRoot!.querySelectorAll<UmbInputValueListOptionElement>('.option'));
	}

	function getField(row: UmbInputValueListOptionElement, field: 'value' | 'label') {
		return row.shadowRoot!.querySelector(`uui-input.${field}`) as UUIInputElement;
	}

	// The uniqueness rule is the only custom validator of the list.
	function hasUniquenessError() {
		getInput().checkValidity();
		return getInput().validity.customError === true;
	}

	async function add() {
		(getInput().shadowRoot!.querySelector('#action') as HTMLElement).click();
		await getInput().updateComplete;
		await getRows().at(-1)!.updateComplete;
	}

	async function leave(row: UmbInputValueListOptionElement, field: 'value' | 'label' = 'value') {
		getField(row, field).dispatchEvent(new FocusEvent('focusout', { bubbles: true, composed: true }));
		await row.updateComplete;
	}

	async function getFieldMessage(row: UmbInputValueListOptionElement, field: 'value' | 'label') {
		const wrapper = getField(row, field).closest('umb-form-validation-message')!;
		await wrapper.updateComplete;
		return wrapper.shadowRoot!.textContent!.trim();
	}

	async function type(row: UmbInputValueListOptionElement, field: 'value' | 'label', text: string) {
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
		await setValue([
			{ value: 'a', label: 'Ay' },
			{ value: 'b', label: 'Bee' },
		]);

		const rows = getRows();
		expect(rows).to.have.lengthOf(2);
		expect(rows.map((row) => getField(row, 'value').value)).to.deep.equal(['a', 'b']);
		expect(rows.map((row) => getField(row, 'label').value)).to.deep.equal(['Ay', 'Bee']);
	});

	it('names the label and value columns above the options, but only when there are options', async () => {
		const getHeader = () => getInput().shadowRoot!.querySelector<HTMLElement>('#header');

		await setValue([]);
		expect(getHeader()).to.be.null;

		await setValue([{ value: 'a', label: 'Ay' }]);
		const columns = Array.from(getHeader()!.querySelectorAll('span')).map((column) => column.textContent!.trim());
		expect(columns).to.deep.equal(['general_label', 'general_value']);
	});

	it('emits the edited label next to the value', async () => {
		await setValue([
			{ value: 'a', label: 'A' },
			{ value: 'b', label: 'Bee' },
		]);

		await type(getRows()[0], 'label', 'Ay');

		expect(element.value).to.deep.equal([
			{ value: 'a', label: 'Ay' },
			{ value: 'b', label: 'Bee' },
		]);
	});

	it('shows the message of a missing label below the label, and none below an empty value', async () => {
		await setValue([{ value: '', label: '' }]);
		const row = getRows()[0];

		await leave(row, 'label');
		await leave(row, 'value');

		expect(await getFieldMessage(row, 'label')).to.not.equal('');
		expect(await getFieldMessage(row, 'value')).to.equal('');
	});

	it('is valid when every option has a label, even with an empty value, as when saving', async () => {
		for (const items of [
			[{ value: 'a', label: 'A' }],
			[{ value: '', label: 'None' }],
			[
				{ value: 'a', label: 'A' },
				{ value: ' ', label: 'None' },
			],
		]) {
			await setValue(items);
			expect(getInput().checkValidity(), JSON.stringify(items)).to.be.true;
		}
	});

	it('is invalid, marking the label, when an option has no label, even with an empty value, as when saving', async () => {
		await setValue([
			{ value: 'a', label: 'A' },
			{ value: 'b', label: '' },
			{ value: '', label: '' },
		]);
		const [filledRow, ...unlabelledRows] = getRows();

		expect(getInput().checkValidity()).to.be.false;

		for (const row of unlabelledRows) {
			expect(getField(row, 'label').validity.valid).to.be.false;
			expect(getField(row, 'label').pristine).to.be.false;
			expect(getField(row, 'value').validity.valid).to.be.true;
		}
		expect(getField(filledRow, 'label').validity.valid).to.be.true;
	});

	it('fills in the value of an added option from its label until the value is edited', async () => {
		await setValue([{ value: 'a', label: 'A' }]);
		await add();
		const row = getRows()[1];

		await type(row, 'label', 'Bee');
		expect(element.value).to.deep.equal([
			{ value: 'a', label: 'A' },
			{ value: 'Bee', label: 'Bee' },
		]);

		await type(row, 'value', 'b');
		await type(row, 'label', 'Bees');
		expect(element.value).to.deep.equal([
			{ value: 'a', label: 'A' },
			{ value: 'b', label: 'Bees' },
		]);
	});

	it('keeps the value of an added option empty once it has been emptied', async () => {
		await setValue([{ value: 'a', label: 'A' }]);
		await add();
		const row = getRows()[1];

		await type(row, 'label', 'None');
		await type(row, 'value', '');
		await type(row, 'label', 'No size');

		expect(element.value).to.deep.equal([
			{ value: 'a', label: 'A' },
			{ value: '', label: 'No size' },
		]);
	});

	it('never changes the value of an existing option, even when its label is retyped to match the value', async () => {
		await setValue([{ value: 'banana', label: 'Banana' }]);
		const row = getRows()[0];

		await type(row, 'label', 'banana');
		await type(row, 'label', 'banana split');

		expect(element.value).to.deep.equal([{ value: 'banana', label: 'banana split' }]);
	});

	it('never fills in the value of an existing option without a value', async () => {
		await setValue([
			{ value: 'a', label: 'A' },
			{ value: '', label: '' },
		]);

		await type(getRows()[1], 'label', 'None');

		expect(element.value).to.deep.equal([
			{ value: 'a', label: 'A' },
			{ value: '', label: 'None' },
		]);
	});

	it('does not fill in the label from the value', async () => {
		await setValue([{ value: 'a', label: 'A' }]);
		await add();

		await type(getRows()[1], 'value', 'x');

		expect(element.value).to.deep.equal([
			{ value: 'a', label: 'A' },
			{ value: 'x', label: '' },
		]);
	});

	it('reports a value that is used more than once as not unique', async () => {
		await setValue([{ value: 'a' }, { value: 'b' }, { value: 'a' }]);

		expect(hasUniquenessError()).to.be.true;
	});

	it('reports blank values as duplicates of each other', async () => {
		await setValue([
			{ value: '', label: 'None' },
			{ value: ' ', label: 'Nothing' },
		]);

		expect(hasUniquenessError()).to.be.true;
	});

	it('shows the uniqueness message in the list once an option is left', async () => {
		await setValue([{ value: 'a' }, { value: 'b' }]);
		const message = getInput().shadowRoot!.querySelector('umb-form-validation-message')!;

		await type(getRows()[1], 'value', 'a');
		await message.updateComplete;
		expect(message.shadowRoot!.textContent).to.not.contain('MustBeUnique');

		await leave(getRows()[1]);
		await getInput().updateComplete;
		await message.updateComplete;
		expect(message.shadowRoot!.textContent).to.contain('MustBeUnique');
	});

	it('dispatches one change for each edit and one for each removal', async () => {
		await setValue([{ value: 'a' }, { value: 'b' }]);
		let changes = 0;
		element.addEventListener('change', () => changes++);

		await type(getRows()[0], 'value', 'x');
		expect(changes).to.equal(1);

		(getRows()[1].shadowRoot!.querySelector('.delete') as HTMLElement).click();
		await getInput().updateComplete;
		await element.updateComplete;
		expect(changes).to.equal(2);
	});

	it('marks the label of an added option as invalid once it is left empty, and valid again when it is filled in', async () => {
		await setValue([{ value: 'a', label: 'A' }]);
		await add();
		const row = getRows()[1];
		const field = getField(row, 'label');
		expect(field.pristine).to.be.true;

		await leave(row, 'label');
		expect(field.pristine).to.be.false;
		expect(field.validity.valid).to.be.false;
		expect(getField(row, 'value').validity.valid).to.be.true;

		await type(row, 'label', 'B');
		await field.updateComplete;
		expect(field.validity.valid).to.be.true;
	});

	it('does not rebuild a row when its option is edited', async () => {
		await setValue([{ value: 'a' }]);
		const row = getRows()[0];

		await type(row, 'value', 'ab');

		expect(getRows()[0]).to.equal(row);
	});

	it('adds an option', async () => {
		await setValue([{ value: 'a' }]);

		await add();

		expect(getRows()).to.have.lengthOf(2);
	});

	it('focuses the first field of an added option', async () => {
		await setValue([{ value: 'a' }]);

		await add();
		await new Promise((resolve) => requestAnimationFrame(resolve));

		const row = getRows()[1];
		expect(getInput().shadowRoot!.activeElement).to.equal(row);
		expect(row.shadowRoot!.activeElement).to.equal(getField(row, 'label'));
	});

	it('removes an option', async () => {
		await setValue([
			{ value: 'a', label: 'A' },
			{ value: 'b', label: 'B' },
		]);

		(getRows()[0].shadowRoot!.querySelector('.delete') as HTMLElement).click();
		await getInput().updateComplete;
		await element.updateComplete;

		expect(element.value).to.deep.equal([{ value: 'b', label: 'B' }]);
	});

	it('hides adding and removing options when read-only', async () => {
		element.readonly = true;
		await setValue([{ value: 'a' }]);

		expect(getInput().shadowRoot!.querySelector('#action')).to.be.null;
		expect(getRows()[0].shadowRoot!.querySelector('.delete')).to.be.null;
	});

	it('shows a disabled add button when read-only and there are no options', async () => {
		element.readonly = true;
		await setValue([]);

		const add = getInput().shadowRoot!.querySelector('#action') as HTMLElement & { disabled: boolean };
		expect(add.disabled).to.be.true;
	});
});
