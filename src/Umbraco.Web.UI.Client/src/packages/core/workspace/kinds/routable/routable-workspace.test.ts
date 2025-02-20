import { UmbRoutableWorkspaceElement } from './routable-workspace.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbRoutableWorkspaceElement', () => {
	let element: UmbRoutableWorkspaceElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-routable-workspace></umb-routable-workspace>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbRoutableWorkspaceElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			// TODO: should we use shadowDom here?
			await expect(element).to.be.accessible(defaultA11yConfig);
		});
	}
});
