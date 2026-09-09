import { UmbPropertyEditorUIRelationTypePickerElement } from './property-editor-ui-relation-type-picker.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

// TODO: figure out why this is not imported by the tests as it should be globally available.
import '../../components/input-relation-type/input-relation-type.element.js';

describe('UmbPropertyEditorUIRelationTypePickerElement', () => {
	let element: UmbPropertyEditorUIRelationTypePickerElement;

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-relation-type-picker></umb-property-editor-ui-relation-type-picker>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIRelationTypePickerElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
