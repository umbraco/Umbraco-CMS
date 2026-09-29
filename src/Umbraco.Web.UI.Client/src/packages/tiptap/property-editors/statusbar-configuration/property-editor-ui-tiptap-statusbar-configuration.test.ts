import { UmbPropertyEditorUiTiptapStatusbarConfigurationElement } from './property-editor-ui-tiptap-statusbar-configuration.element.js';
import type { UmbTiptapStatusbarValue } from '../../components/types.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyContext, UmbPropertyDatasetContextBase } from '@umbraco-cms/backoffice/property';

const STATUSBAR_EXTENSION_ALIASES = [
	'Test.Statusbar.One',
	'Test.Statusbar.Two',
	'Test.Statusbar.Three',
	'Test.Statusbar.Four',
];

const manifests: Array<UmbExtensionManifest> = STATUSBAR_EXTENSION_ALIASES.map((alias) => ({
	type: 'tiptapStatusbarExtension',
	alias,
	name: alias,
	meta: { alias, icon: 'icon-code', label: alias },
}));

@customElement('umb-test-tiptap-statusbar-configuration-host')
class UmbTestTiptapStatusbarConfigurationHostElement extends UmbLitElement {
	readonly datasetContext = new UmbPropertyDatasetContextBase(this);
	readonly propertyContext = new UmbPropertyContext(this);

	constructor() {
		super();
		this.propertyContext.setAlias('statusbar');
		this.datasetContext.setValues([{ alias: 'extensions', value: [] }]);
	}

	override render() {
		return html`<slot></slot>`;
	}
}

describe('UmbPropertyEditorUiTiptapStatusbarConfigurationElement', () => {
	let element: UmbPropertyEditorUiTiptapStatusbarConfigurationElement;

	/** The `.area` element of the designer, which is what receives a dropped statusbar item. */
	const areaElement = (areaIndex: number) => element.shadowRoot!.querySelectorAll<HTMLElement>('.area')[areaIndex];

	const dragEvent = (type: string, dataTransfer: DataTransfer) =>
		new DragEvent(type, { bubbles: true, cancelable: true, composed: true, dataTransfer });

	/** Drags an item from the available items list onto an area, as a user would with the mouse. */
	const dragAvailableItemToArea = async (alias: string, areaIndex: number) => {
		const selector = `.available-items [data-mark="tiptap-statusbar-item:${alias}"]`;
		await waitUntil(() => element.shadowRoot!.querySelector(selector), `available item for ${alias} was rendered`);
		const availableItem = element.shadowRoot!.querySelector<HTMLElement>(selector)!;

		const dataTransfer = new DataTransfer();
		availableItem.dispatchEvent(dragEvent('dragstart', dataTransfer));
		areaElement(areaIndex).dispatchEvent(dragEvent('dragover', dataTransfer));
		areaElement(areaIndex).dispatchEvent(dragEvent('drop', dataTransfer));
		availableItem.dispatchEvent(dragEvent('dragend', dataTransfer));

		await element.updateComplete;
	};

	/**
	 * Mimics a drop that belongs to another editor, such as the toolbar designer that sits alongside this one on the
	 * data type workspace. Its data transfer does not carry the type of a statusbar item.
	 */
	const dispatchForeignDrop = async (areaIndex: number) => {
		const dataTransfer = new DataTransfer();
		dataTransfer.setData('text/umb-tiptap-toolbar-item', 'Test.Toolbar.One');
		const event = dragEvent('drop', dataTransfer);
		areaElement(areaIndex).dispatchEvent(event);

		await element.updateComplete;
		return event;
	};

	const givenStatusbar = async (value: UmbTiptapStatusbarValue) => {
		const host = await fixture<UmbTestTiptapStatusbarConfigurationHostElement>(html`
			<umb-test-tiptap-statusbar-configuration-host>
				<umb-property-editor-ui-tiptap-statusbar-configuration .value=${value}>
				</umb-property-editor-ui-tiptap-statusbar-configuration>
			</umb-test-tiptap-statusbar-configuration-host>
		`);

		element = host.querySelector('umb-property-editor-ui-tiptap-statusbar-configuration')!;
		await element.updateComplete;
		await waitUntil(
			() => element.shadowRoot!.querySelectorAll('.area').length === value.length,
			'the areas of the statusbar were rendered',
		);
	};

	const aliasesInUse = () => (element.value ?? []).flat();

	const duplicatedAliases = () => {
		const aliases = aliasesInUse();
		return [...new Set(aliases.filter((alias, index) => aliases.indexOf(alias) !== index))];
	};

	beforeEach(async () => {
		umbExtensionsRegistry.registerMany(manifests);
		await givenStatusbar([['Test.Statusbar.One', 'Test.Statusbar.Two'], []]);
	});

	afterEach(() => {
		umbExtensionsRegistry.unregisterMany(manifests.map((manifest) => manifest.alias));
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUiTiptapStatusbarConfigurationElement);
	});

	it('appends an item dragged from the available items to the end of the area', async () => {
		await dragAvailableItemToArea('Test.Statusbar.Three', 0);

		expect(element.value).to.deep.equal([
			['Test.Statusbar.One', 'Test.Statusbar.Two', 'Test.Statusbar.Three'],
			[],
		]);
	});

	it('cannot duplicate an item when a drag of another editor is dropped on an area', async () => {
		await dragAvailableItemToArea('Test.Statusbar.Three', 0);
		await dispatchForeignDrop(1);

		expect(duplicatedAliases()).to.be.empty;
	});

	it('drops the repeat occurrences of an item already duplicated in the value', async () => {
		await givenStatusbar([['Test.Statusbar.One', 'Test.Statusbar.Two'], ['Test.Statusbar.One']]);

		await dragAvailableItemToArea('Test.Statusbar.Three', 0);

		expect(element.value).to.deep.equal([
			['Test.Statusbar.One', 'Test.Statusbar.Two', 'Test.Statusbar.Three'],
			[],
		]);
	});

	it('leaves a drag of another editor to be handled elsewhere', async () => {
		const event = await dispatchForeignDrop(0);

		expect(event.defaultPrevented, 'the drop was claimed by an area').to.be.false;
		expect(aliasesInUse()).to.not.include('Test.Toolbar.One');
	});
});
