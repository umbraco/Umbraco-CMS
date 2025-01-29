import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import UmbBlockRteEntryInlineElement from './block-rte-entry-inline.element.js';

const blockGuid = '00000000-0000-0000-0000-000000000000';

describe('UmbBlockRteEntryInline', () => {
	let element: UmbBlockRteEntryInlineElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-rte-block-inline .contentKey=${blockGuid}></umb-rte-block-inline>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbBlockRteEntryInlineElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).to.be.accessible(defaultA11yConfig);
		});
	}
});
