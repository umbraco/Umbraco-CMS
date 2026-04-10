import { UmbPropertyEditorUITagsElement } from './property-editor-ui-tags.element.js';
import type { UmbTagsInputElement } from '../../components/tags-input/tags-input.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUITagsElement', () => {
	let element: UmbPropertyEditorUITagsElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-tags></umb-property-editor-ui-tags> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUITagsElement);
	});

	it('preserves tags containing commas', async () => {
		const tagsInput = element.shadowRoot!.querySelector('umb-tags-input') as UmbTagsInputElement;
		tagsInput.items = ['hello', 'world, with comma', 'foo'];
		tagsInput.dispatchEvent(new CustomEvent('change'));
		await element.updateComplete;
		expect(element.value).to.deep.equal(['hello', 'world, with comma', 'foo']);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
