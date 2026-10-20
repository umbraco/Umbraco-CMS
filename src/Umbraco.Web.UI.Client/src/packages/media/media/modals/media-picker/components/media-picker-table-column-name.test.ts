import { UmbMediaPickerTableColumnNameElement } from './media-picker-table-column-name.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbMediaPickerTableColumnNameElement', () => {
	let element: UmbMediaPickerTableColumnNameElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-media-picker-table-column-name></umb-media-picker-table-column-name>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbMediaPickerTableColumnNameElement);
	});

	it('renders the name', async () => {
		element.value = { name: 'My image' };
		await element.updateComplete;
		expect(element.shadowRoot?.textContent).to.contain('My image');
	});

	it('renders the ancestor path when provided', async () => {
		element.value = { name: 'My image', ancestorPath: 'Media / Holiday' };
		await element.updateComplete;
		const path = element.shadowRoot?.querySelector('.ancestor-path');
		expect(path).to.exist;
		expect(path?.textContent).to.contain('Media / Holiday');
	});

	it('does not render an ancestor path when not provided', async () => {
		element.value = { name: 'My image' };
		await element.updateComplete;
		expect(element.shadowRoot?.querySelector('.ancestor-path')).to.not.exist;
	});

	it('renders the name as a plain span when not navigable', async () => {
		element.value = { name: 'My image' };
		await element.updateComplete;
		expect(element.shadowRoot?.querySelector('uui-button')).to.not.exist;
		expect(element.shadowRoot?.querySelector('span.name')).to.exist;
	});

	it('renders the name as a button and invokes navigate on click', async () => {
		let navigated = false;
		element.value = { name: 'A folder', navigate: () => (navigated = true) };
		await element.updateComplete;

		const button = element.shadowRoot?.querySelector('uui-button');
		expect(button).to.exist;

		button!.dispatchEvent(new MouseEvent('click', { bubbles: true, composed: true }));
		expect(navigated).to.be.true;
	});

	it('stops the click from bubbling so it does not trigger row selection', async () => {
		element.value = { name: 'A folder', navigate: () => {} };
		await element.updateComplete;

		const button = element.shadowRoot?.querySelector('uui-button');

		let bubbledToHost = false;
		element.addEventListener('click', () => (bubbledToHost = true));
		button!.dispatchEvent(new MouseEvent('click', { bubbles: true, composed: true }));

		expect(bubbledToHost).to.be.false;
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			element.value = { name: 'My image', ancestorPath: 'Media / Holiday' };
			await element.updateComplete;
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
