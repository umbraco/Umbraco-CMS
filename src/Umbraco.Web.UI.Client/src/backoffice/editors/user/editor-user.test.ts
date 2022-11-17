import { expect, fixture, html } from '@open-wc/testing';

import { defaultA11yConfig } from '@umbraco-cms/test-utils';
import UmbEditorUserElement from './editor-user.element';

describe('UmbEditorUserElement', () => {
	let element: UmbEditorUserElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-editor-user></umb-editor-user>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbEditorUserElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
