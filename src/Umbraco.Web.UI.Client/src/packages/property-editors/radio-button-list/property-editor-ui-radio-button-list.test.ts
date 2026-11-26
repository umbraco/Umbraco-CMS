import { UmbPropertyEditorUIRadioButtonListElement } from './property-editor-ui-radio-button-list.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIRadioButtonListElement', () => {
	let element: UmbPropertyEditorUIRadioButtonListElement;

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-radio-button-list></umb-property-editor-ui-radio-button-list>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIRadioButtonListElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}

	describe('labelled options', () => {
		function setItems(items: Array<unknown>) {
			element.config = {
				getValueByAlias: (alias: string) => (alias === 'items' ? items : undefined),
			} as any;
		}

		async function getRadios() {
			await element.updateComplete;
			const input = element.shadowRoot?.querySelector('umb-input-radio-button-list');
			await input?.updateComplete;
			return Array.from(input?.shadowRoot?.querySelectorAll('uui-radio') ?? []) as Array<
				HTMLElement & { label: string; value: string }
			>;
		}

		it('shows the label of an option', async () => {
			setItems([
				{ value: 'one', label: 'One' },
				{ value: 'two', label: 'Two' },
			]);

			const radios = await getRadios();
			expect(radios.map((radio) => radio.label)).to.deep.equal(['One', 'Two']);
			expect(radios.map((radio) => radio.value)).to.deep.equal(['one', 'two']);
		});

		it('shows a stored value that is not an option by its value', async () => {
			element.value = 'three';
			setItems([
				{ value: 'one', label: 'One' },
				{ value: 'two', label: 'Two' },
			]);

			const radios = await getRadios();
			expect(radios[2].label).to.match(/^three \(/);
		});
	});
});
