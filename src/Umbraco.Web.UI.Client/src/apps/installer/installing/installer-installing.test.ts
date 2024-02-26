import { expect, fixture, html } from '@open-wc/testing';

import { UmbInstallerInstallingElement } from './installer-installing.element.js';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

// TODO: Write tests
describe('UmbInstallerInstallingElement', () => {
	let element: UmbInstallerInstallingElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer-installing></umb-installer-installing>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInstallerInstallingElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).to.be.accessible(defaultA11yConfig);
		});
	}
});
