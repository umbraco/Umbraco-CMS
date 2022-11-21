import { expect, fixture, html } from '@open-wc/testing';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';
import UmbEditorViewUsersInviteElement from './editor-view-users-invite.element';

describe('UmbEditorViewUsersInviteElement', () => {
	let element: UmbEditorViewUsersInviteElement;
	beforeEach(async () => {
		element = await fixture(html`<umb-editor-view-users-invite></umb-editor-view-users-invi>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbEditorViewUsersInviteElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
