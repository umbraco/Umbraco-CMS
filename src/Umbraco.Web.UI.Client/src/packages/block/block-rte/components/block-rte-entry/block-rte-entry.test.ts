import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import UmbBlockRteEntryElement from './block-rte-entry.element.js';

const blockGuid = '00000000-0000-0000-0000-000000000000';

describe('UmbBlockRteEntry', () => {
	let element: UmbBlockRteEntryElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-rte-block .contentKey=${blockGuid}></umb-rte-block>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbBlockRteEntryElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).to.be.accessible(defaultA11yConfig);
		});
	}
});
