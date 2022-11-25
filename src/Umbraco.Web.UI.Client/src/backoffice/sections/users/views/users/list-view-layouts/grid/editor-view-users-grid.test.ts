import { expect, fixture, html } from '@open-wc/testing';
import { UmbEditorViewUsersGridElement } from './editor-view-users-grid.element';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';

describe('UmbEditorViewUsersCreateElement', () => {
	let element: UmbEditorViewUsersGridElement;
	beforeEach(async () => {
		element = await fixture(html`<umb-editor-view-users-grid></umb-editor-view-users-grid>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbEditorViewUsersGridElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
