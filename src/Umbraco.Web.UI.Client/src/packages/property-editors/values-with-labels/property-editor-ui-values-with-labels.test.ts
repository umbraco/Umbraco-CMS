import { UmbPropertyEditorUIValuesWithLabelsElement } from './property-editor-ui-values-with-labels.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import type { UmbInputValueWithLabelElement } from './input-value-with-label.element.js';
import type { UmbLocalizationSet } from '@umbraco-cms/backoffice/localization-api';
import { UMB_DEFAULT_LOCALIZATION_CULTURE, umbLocalizationManager } from '@umbraco-cms/backoffice/localization-api';

describe('UmbPropertyEditorUIValuesWithLabelsElement', () => {
	let element: UmbPropertyEditorUIValuesWithLabelsElement;

	async function setValue(value: unknown) {
		element.value = value as typeof element.value;
		await element.updateComplete;
		await getInput().updateComplete;
	}

	function getInput() {
		return element.shadowRoot!.querySelector('umb-input-values-with-labels')!;
	}

	function getRows() {
		return Array.from(getInput().shadowRoot!.querySelectorAll<UmbInputValueWithLabelElement>('.option'));
	}

	function getField(row: UmbInputValueWithLabelElement, field: 'value' | 'label') {
		return row.shadowRoot!.querySelector(`uui-input.${field}`) as UUIInputElement;
	}

	// The uniqueness rules are the only custom validators of the list.
	function getUniquenessMessage() {
		getInput().checkValidity();
		return getInput().validity.customError === true ? getInput().validationMessage : undefined;
	}

	const messages: UmbLocalizationSet = {
		$code: UMB_DEFAULT_LOCALIZATION_CULTURE,
		$dir: 'ltr',
		general_label: 'Label',
		general_value: 'Value',
		validation_duplicateFieldValue: '%0% "%1%" is used more than once',
	};
	let previousFallback: ReturnType<typeof umbLocalizationManager.localizations.get>;

	before(() => {
		previousFallback = umbLocalizationManager.localizations.get(UMB_DEFAULT_LOCALIZATION_CULTURE);
		umbLocalizationManager.registerLocalization(messages);
	});

	after(() => {
		if (previousFallback) {
			umbLocalizationManager.localizations.set(UMB_DEFAULT_LOCALIZATION_CULTURE, previousFallback);
		} else {
			umbLocalizationManager.localizations.delete(UMB_DEFAULT_LOCALIZATION_CULTURE);
		}
	});

	async function add() {
		(getInput().shadowRoot!.querySelector('#action') as HTMLElement).click();
		await getInput().updateComplete;
		await getRows().at(-1)!.updateComplete;
	}

	async function leave(row: UmbInputValueWithLabelElement, field: 'value' | 'label' = 'value') {
		getField(row, field).dispatchEvent(new FocusEvent('focusout', { bubbles: true, composed: true }));
		await row.updateComplete;
	}

	async function getFieldMessage(row: UmbInputValueWithLabelElement, field: 'value' | 'label') {
		const wrapper = getField(row, field).closest('umb-form-validation-message')!;
		await wrapper.updateComplete;
		return wrapper.shadowRoot!.textContent!.trim();
	}

	async function type(row: UmbInputValueWithLabelElement, field: 'value' | 'label', text: string) {
		const input = getField(row, field);
		input.value = text;
		input.dispatchEvent(new CustomEvent('input', { bubbles: true, composed: true }));
		await getInput().updateComplete;
		await element.updateComplete;
	}

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-values-with-labels></umb-property-editor-ui-values-with-labels>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIValuesWithLabelsElement);
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
		expect(columns).to.deep.equal(['Label', 'Value']);
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

	it('fills in the value of an added option with an alias of its label until the value is edited', async () => {
		await setValue([{ value: 'a', label: 'A' }]);
		await add();
		const row = getRows()[1];

		await type(row, 'label', 'Bumble Bee');
		expect(element.value).to.deep.equal([
			{ value: 'a', label: 'A' },
			{ value: 'bumbleBee', label: 'Bumble Bee' },
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

	it('reports a value that is used more than once, naming the value', async () => {
		await setValue([{ value: 'a' }, { value: 'b' }, { value: 'a' }]);

		expect(getUniquenessMessage()).to.equal('Value "a" is used more than once');
	});

	it('reports blank values as duplicates of each other', async () => {
		await setValue([
			{ value: '', label: 'None' },
			{ value: ' ', label: 'Nothing' },
		]);

		expect(getUniquenessMessage()).to.equal('Value "" is used more than once');
	});

	it('reports a label that is used more than once, ignoring surrounding whitespace, naming the label', async () => {
		await setValue([
			{ value: 'a', label: 'Small' },
			{ value: 'b', label: ' Small ' },
		]);

		expect(getUniquenessMessage()).to.equal('Label "Small" is used more than once');
	});

	it('compares labels case-sensitively, and leaves missing labels to the required rule', async () => {
		await setValue([
			{ value: 'a', label: 'Small' },
			{ value: 'b', label: 'small' },
			{ value: 'c', label: '' },
			{ value: 'd', label: ' ' },
		]);

		expect(getUniquenessMessage()).to.be.undefined;
	});

	it('reports a repeated label before the value filled in from it', async () => {
		await setValue([{ value: 'small', label: 'Small' }]);
		await add();

		await type(getRows()[1], 'label', 'Small');

		expect(element.value).to.deep.equal([
			{ value: 'small', label: 'Small' },
			{ value: 'small', label: 'Small' },
		]);
		expect(getUniquenessMessage()).to.equal('Label "Small" is used more than once');
	});

	it('shows the uniqueness message in the list once an option is left', async () => {
		await setValue([
			{ value: 'a', label: 'A' },
			{ value: 'b', label: 'B' },
		]);
		const message = getInput().shadowRoot!.querySelector('umb-form-validation-message')!;

		await type(getRows()[1], 'value', 'a');
		await message.updateComplete;
		expect(message.shadowRoot!.textContent).to.not.contain('is used more than once');

		await leave(getRows()[1]);
		await getInput().updateComplete;
		await message.updateComplete;
		expect(message.shadowRoot!.textContent).to.contain('Value "a" is used more than once');
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
