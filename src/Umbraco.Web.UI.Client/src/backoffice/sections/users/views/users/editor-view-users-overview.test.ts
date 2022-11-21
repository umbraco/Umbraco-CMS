import { expect, fixture, html } from '@open-wc/testing';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';
import UmbEditorViewUsersOverviewElement from './editor-view-users-overview.element';

describe('UmbEditorViewUsersOverviewElement', () => {
	let element: UmbEditorViewUsersOverviewElement;
	beforeEach(async () => {
		element = await fixture(html`<umb-editor-view-users-overview></umb-editor-view-users-overview>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbEditorViewUsersOverviewElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
