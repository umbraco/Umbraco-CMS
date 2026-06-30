import { expect, fixture, html } from '@open-wc/testing';

import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import UmbUnsupportedRteBlockElement from './unsupported-rte-block.element.js';

describe('UmbUnsupportedRteBlock', () => {
	let element: UmbUnsupportedRteBlockElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-unsupported-rte-block></umb-unsupported-rte-block>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbUnsupportedRteBlockElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).to.be.accessible(defaultA11yConfig);
		});
	}
});
