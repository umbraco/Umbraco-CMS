import { UmbPropertyEditorUIOverlaySizeElement } from './property-editor-ui-overlay-size.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

describe('UmbPropertyEditorUIOverlaySizeElement', () => {
	let element: UmbPropertyEditorUIOverlaySizeElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-overlay-size></umb-property-editor-ui-overlay-size> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIOverlaySizeElement);
	});

	it('should use custom default option label from config', async () => {
		element.config = new UmbPropertyEditorConfigCollection([{ alias: 'defaultOptionLabel', value: 'Auto' }]);
		await element.updateComplete;

		const select = element.shadowRoot!.querySelector('uui-select')!;
		const options = (select as any).options as Array<Option>;
		const defaultOption = options.find((o: Option) => o.value === undefined);
		expect(defaultOption?.name).to.equal('Auto');
	});

	it('should keep Default label when no defaultOptionLabel config is provided', async () => {
		element.config = new UmbPropertyEditorConfigCollection([]);
		await element.updateComplete;

		const select = element.shadowRoot!.querySelector('uui-select')!;
		const options = (select as any).options as Array<Option>;
		const defaultOption = options.find((o: Option) => o.value === undefined);
		expect(defaultOption?.name).to.equal('Default');
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
