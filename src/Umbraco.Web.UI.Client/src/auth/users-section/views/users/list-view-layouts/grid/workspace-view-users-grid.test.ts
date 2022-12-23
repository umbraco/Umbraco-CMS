import { expect, fixture, html } from '@open-wc/testing';
import { UmbWorkspaceViewUsersGridElement } from './workspace-view-users-grid.element';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';

describe('UmbWorkspaceViewUsersCreateElement', () => {
	let element: UmbWorkspaceViewUsersGridElement;
	beforeEach(async () => {
		element = await fixture(html`<umb-workspace-view-users-grid></umb-workspace-view-users-grid>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbWorkspaceViewUsersGridElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
