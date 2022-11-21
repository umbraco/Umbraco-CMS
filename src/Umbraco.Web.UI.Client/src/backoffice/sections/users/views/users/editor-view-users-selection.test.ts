import { expect, fixture, html } from '@open-wc/testing';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';
import UmbEditorViewUsersSelectionElement from './editor-view-users-selection.element';

describe('UmbEditorViewUsersSelectionElement', () => {
	let element: UmbEditorViewUsersSelectionElement;
	beforeEach(async () => {
		element = await fixture(html`<umb-editor-view-users-selection></umb-editor-view-users-selection>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbEditorViewUsersSelectionElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
