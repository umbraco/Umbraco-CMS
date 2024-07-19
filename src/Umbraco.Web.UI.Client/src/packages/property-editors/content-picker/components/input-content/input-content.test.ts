import { UmbInputContentElement } from './input-content.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import type { UmbTestRunnerWindow } from '@umbraco-cms/internal/test-utils';

import '@umbraco-cms/backoffice/document';

describe('UmbInputContentElement', () => {
	let element: UmbInputContentElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-input-content></umb-input-content>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputContentElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
