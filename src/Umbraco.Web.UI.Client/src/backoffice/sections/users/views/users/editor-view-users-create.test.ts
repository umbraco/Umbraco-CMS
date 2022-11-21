import { expect, fixture, html } from '@open-wc/testing';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';
import UmbEditorViewUsersCreateElement from './editor-view-users-create.element';

describe('UmbEditorViewUsersCreateElement', () => {
	let element: UmbEditorViewUsersCreateElement;
	beforeEach(async () => {
		element = await fixture(html`<umb-editor-view-users-create></umb-editor-view-users-invi>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbEditorViewUsersCreateElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
