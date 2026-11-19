import { UmbWorkspaceHeaderNameEditableElement } from './workspace-header-name-editable.element.js';
import { UMB_NAMABLE_WORKSPACE_CONTEXT } from '../../namable/index.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbReadOnlyGuardManager, type UmbGuardRule } from '@umbraco-cms/backoffice/utils';

@customElement('umb-test-workspace-header-name-editable-host')
class UmbTestWorkspaceHeaderNameEditableHostElement extends UmbElementMixin(HTMLElement) {
	readOnlyGuard = new UmbReadOnlyGuardManager<UmbGuardRule>(this);

	constructor() {
		super();
		this.provideContext(UMB_NAMABLE_WORKSPACE_CONTEXT, {
			IS_ENTITY_DETAIL_WORKSPACE_CONTEXT: true,
			getName: () => 'Test',
			readOnlyGuard: this.readOnlyGuard,
			getHostElement: () => this,
		} as never);
	}
}

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

	it('makes the name input read-only while the workspace entity is read-only', async () => {
		const host = await fixture<UmbTestWorkspaceHeaderNameEditableHostElement>(
			html`<umb-test-workspace-header-name-editable-host>
				<umb-workspace-header-name-editable></umb-workspace-header-name-editable>
			</umb-test-workspace-header-name-editable-host>`,
		);
		const nameElement = host.querySelector('umb-workspace-header-name-editable')!;
		expect(isNameInputReadonly(nameElement)).to.be.false;

		host.readOnlyGuard.addRule({ unique: 'test', permitted: true });
		await waitUntil(() => isNameInputReadonly(nameElement), 'the name input did not become read-only');

		host.readOnlyGuard.removeRule('test');
		await waitUntil(() => !isNameInputReadonly(nameElement), 'the name input did not become editable again');
	});
});
