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

	async function leave(row: UmbInputValueListOptionElement) {
		getField(row, 'value').dispatchEvent(new FocusEvent('focusout', { bubbles: true, composed: true }));
		await row.updateComplete;
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
		await setValue([{ value: 'a' }, { value: 'b', label: 'Bee' }]);

		const rows = getRows();
		expect(rows).to.have.lengthOf(2);
		expect(rows.map((row) => getField(row, 'value').value)).to.deep.equal(['a', 'b']);
		expect(rows.map((row) => getField(row, 'label').value)).to.deep.equal(['', 'Bee']);
	});

	it('names the label and value columns above the options, but only when there are options', async () => {
		const getHeader = () => getInput().shadowRoot!.querySelector<HTMLElement>('#header');

		await setValue([]);
		expect(getHeader()).to.equal(null);

		await setValue([{ value: 'a' }]);
		const columns = Array.from(getHeader()!.querySelectorAll('span')).map((column) => column.textContent!.trim());
		expect(columns).to.deep.equal(['general_label', 'general_value']);
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

	it('reports a value that is used more than once as not unique', async () => {
		await setValue([{ value: 'a' }, { value: 'b' }, { value: 'a' }]);

		expect(hasUniquenessError()).to.be.true;
	});

	it('reports empty values as missing, not as duplicates of each other', async () => {
		await setValue([{ value: '' }, { value: '' }]);
		const fields = getRows().map((row) => getField(row, 'value'));
		fields.forEach((field) => field.checkValidity());

		expect(fields.map((field) => field.validity.valueMissing)).to.deep.equal([true, true]);
		expect(hasUniquenessError()).to.be.false;
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

	it('marks an added option as invalid once its empty value is left, and valid again when it is filled in', async () => {
		await setValue([{ value: 'a' }]);
		await add();
		const row = getRows()[1];
		const field = getField(row, 'value');
		expect(field.pristine).to.be.true;

		await leave(row);
		expect(field.pristine).to.be.false;
		expect(field.validity.valid).to.be.false;

		await type(row, 'value', 'b');
		await field.updateComplete;
		expect(field.validity.valid).to.be.true;
	});

	it('does not rebuild a row when its option is edited', async () => {
		await setValue([{ value: 'a' }]);
		const row = getRows()[0];

		await type(row, 'value', 'ab');

		expect(getRows()[0]).to.equal(row);
	});

	it('keeps the focus in place when the add button is pressed, so a shown error cannot move it', async () => {
		await setValue([{ value: '' }]);

		const event = new MouseEvent('mousedown', { bubbles: true, cancelable: true });
		getInput().shadowRoot!.querySelector('#action')!.dispatchEvent(event);

		expect(event.defaultPrevented).to.be.true;
	});

	it('keeps the focus in place when a delete button is pressed', async () => {
		await setValue([{ value: 'a' }, { value: 'b' }]);

		const event = new MouseEvent('mousedown', { bubbles: true, cancelable: true });
		getRows()[1].shadowRoot!.querySelector('.delete')!.dispatchEvent(event);

		expect(event.defaultPrevented).to.be.true;
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
		await setValue([{ value: 'a' }, { value: 'b' }]);

		(getRows()[0].shadowRoot!.querySelector('.delete') as HTMLElement).click();
		await getInput().updateComplete;
		await element.updateComplete;

		expect(element.value).to.deep.equal([{ value: 'b' }]);
	});

	it('hides adding and removing options when read-only', async () => {
		element.readonly = true;
		await setValue([{ value: 'a' }]);

		expect(getInput().shadowRoot!.querySelector('#action')).to.equal(null);
		expect(getRows()[0].shadowRoot!.querySelector('.delete')).to.equal(null);
	});

	it('shows a disabled add button when read-only and there are no options', async () => {
		element.readonly = true;
		await setValue([]);

		const add = getInput().shadowRoot!.querySelector('#action') as HTMLElement & { disabled: boolean };
		expect(add.disabled).to.be.true;
	});
});
