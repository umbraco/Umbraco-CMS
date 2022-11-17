import { expect, fixture, html } from '@open-wc/testing';
import UmbEditorUserGroupElement from './editor-user-group.element';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';

describe('UmbEditorUserGroupElement', () => {
	let element: UmbEditorUserGroupElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-editor-user-group></umb-editor-user-group> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbEditorUserGroupElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
