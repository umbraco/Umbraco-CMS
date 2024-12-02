import { UmbInstallerConsentElement } from './installer-consent.element.js';
import { expect, fixture, html } from '@open-wc/testing';

import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

// TODO: Write tests
describe('UmbInstallerConsentElement', () => {
	let element: UmbInstallerConsentElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer-consent></umb-installer-consent>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInstallerConsentElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).to.be.accessible(defaultA11yConfig);
		});
	}
});
