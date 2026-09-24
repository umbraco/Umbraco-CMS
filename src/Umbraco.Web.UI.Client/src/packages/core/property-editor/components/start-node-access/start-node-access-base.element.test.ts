import { UmbPropertyEditorUiStartNodeAccessElementBase } from './start-node-access-base.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-test-start-node-access')
class UmbTestStartNodeAccessElement extends UmbPropertyEditorUiStartNodeAccessElementBase {
	public pick(selection: Array<string>) {
		this._onPick(selection);
	}

	protected override renderPicker() {
		return html`<div id="picker"></div>`;
	}
}

describe('UmbPropertyEditorUiStartNodeAccessElementBase', () => {
	let element: UmbTestStartNodeAccessElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-test-start-node-access></umb-test-start-node-access>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbTestStartNodeAccessElement);
	});

	it('round-trips its value through the getter/setter', () => {
		element.value = { rootAccess: false, startNodes: [{ unique: 'A' }] };
		expect(element.value).to.deep.equal({ rootAccess: false, startNodes: [{ unique: 'A' }] });
	});

	it('defaults to no root access and no start nodes when given an undefined value', () => {
		element.value = undefined;
		expect(element.value).to.deep.equal({ rootAccess: false, startNodes: [] });
	});

	it('clears the selection when root access is toggled on', async () => {
		element.value = { rootAccess: false, startNodes: [{ unique: 'A' }] };
		await element.updateComplete;

		const toggle = element.shadowRoot!.querySelector('uui-toggle')! as HTMLElement & { checked: boolean };
		toggle.checked = true;
		toggle.dispatchEvent(new Event('change'));

		expect(element.value).to.deep.equal({ rootAccess: true, startNodes: [] });
	});

	it('clears root access when a start node is picked', () => {
		element.value = { rootAccess: true, startNodes: [] };
		element.pick(['A', 'B']);

		expect(element.value).to.deep.equal({ rootAccess: false, startNodes: [{ unique: 'A' }, { unique: 'B' }] });
	});
});
