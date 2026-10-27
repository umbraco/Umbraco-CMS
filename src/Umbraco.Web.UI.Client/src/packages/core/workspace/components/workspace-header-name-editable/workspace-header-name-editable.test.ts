import { UmbWorkspaceHeaderNameEditableElement } from './workspace-header-name-editable.element.js';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbWorkspaceHeaderNameEditableElement', () => {
	let element: UmbWorkspaceHeaderNameEditableElement;

	const isNameInputReadonly = (target: UmbWorkspaceHeaderNameEditableElement) =>
		target.shadowRoot!.querySelector('uui-input')!.readonly;

	beforeEach(async () => {
		element = await fixture(html`<umb-workspace-header-name-editable></umb-workspace-header-name-editable>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbWorkspaceHeaderNameEditableElement);
	});

	it('keeps the name input editable by default', () => {
		expect(isNameInputReadonly(element)).to.be.false;
	});

	it('makes the name input read-only when readonly is set', async () => {
		element.readonly = true;
		await element.updateComplete;

		expect(isNameInputReadonly(element)).to.be.true;
	});

	it('makes the name input read-only when the readonly attribute is set', async () => {
		const readonlyElement = await fixture<UmbWorkspaceHeaderNameEditableElement>(
			html`<umb-workspace-header-name-editable readonly></umb-workspace-header-name-editable>`,
		);

		expect(isNameInputReadonly(readonlyElement)).to.be.true;
	});
});
