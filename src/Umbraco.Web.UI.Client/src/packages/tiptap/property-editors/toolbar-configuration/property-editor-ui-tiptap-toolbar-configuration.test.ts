import { UmbPropertyEditorUiTiptapToolbarConfigurationElement } from './property-editor-ui-tiptap-toolbar-configuration.element.js';
import type { UmbTiptapToolbarValue } from '../../components/types.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyContext, UmbPropertyDatasetContextBase } from '@umbraco-cms/backoffice/property';

const TOOLBAR_EXTENSION_ALIASES = ['Test.Toolbar.One', 'Test.Toolbar.Two', 'Test.Toolbar.Three', 'Test.Toolbar.Four'];

const manifests: Array<UmbExtensionManifest> = TOOLBAR_EXTENSION_ALIASES.map((alias) => ({
	type: 'tiptapToolbarExtension',
	kind: 'button',
	alias,
	name: alias,
	meta: { alias, icon: 'icon-code', label: alias },
}));

@customElement('umb-test-tiptap-toolbar-configuration-host')
class UmbTestTiptapToolbarConfigurationHostElement extends UmbLitElement {
	readonly datasetContext = new UmbPropertyDatasetContextBase(this);
	readonly propertyContext = new UmbPropertyContext(this);

	constructor() {
		super();
		this.propertyContext.setAlias('toolbar');
		this.datasetContext.setValues([{ alias: 'extensions', value: [] }]);
	}

	override render() {
		return html`<slot></slot>`;
	}
}

describe('UmbPropertyEditorUiTiptapToolbarConfigurationElement', () => {
	let element: UmbPropertyEditorUiTiptapToolbarConfigurationElement;

	/** The `.group` element of the designer, which is what receives a dropped toolbar item. */
	const groupElement = (groupIndex: number) =>
		element.shadowRoot!.querySelectorAll<HTMLElement>('.group')[groupIndex];

	/** The sorter's container, within the group element, which is where a reorder is dropped. */
	const groupSorterContainer = (groupIndex: number) =>
		element
			.shadowRoot!.querySelectorAll('umb-tiptap-toolbar-group-configuration')
			[groupIndex].shadowRoot!.querySelector<HTMLElement>('.items')!;

	const dragEvent = (type: string, dataTransfer: DataTransfer) =>
		new DragEvent(type, { bubbles: true, cancelable: true, composed: true, dataTransfer });

	/** Drags an item from the available items list onto a group, as a user would with the mouse. */
	const dragAvailableItemToGroup = async (alias: string, groupIndex: number) => {
		const selector = `.available-items [data-mark="tiptap-toolbar-item:${alias}"]`;
		await waitUntil(() => element.shadowRoot!.querySelector(selector), `available item for ${alias} was rendered`);
		const availableItem = element.shadowRoot!.querySelector<HTMLElement>(selector)!;

		const dataTransfer = new DataTransfer();
		availableItem.dispatchEvent(dragEvent('dragstart', dataTransfer));
		groupElement(groupIndex).dispatchEvent(dragEvent('dragover', dataTransfer));
		groupElement(groupIndex).dispatchEvent(dragEvent('drop', dataTransfer));
		availableItem.dispatchEvent(dragEvent('dragend', dataTransfer));

		await element.updateComplete;
	};

	/**
	 * Mimics the drop of the sorter that reorders the items of a group. The sorter carries its own identifier on the
	 * data transfer, and does not stop the drop from bubbling out to the group element.
	 */
	const dispatchSorterDrop = async (groupIndex: number) => {
		const dataTransfer = new DataTransfer();
		dataTransfer.setData('text/umb-sorter-identifier#umb-tiptap-toolbar-sorter', 'true');
		groupSorterContainer(groupIndex).dispatchEvent(dragEvent('drop', dataTransfer));

		await element.updateComplete;
	};

	const aliasesInUse = () => (element.value ?? []).flat(2);

	const occurrencesOf = (alias: string) => aliasesInUse().filter((x) => x === alias).length;

	const duplicatedAliases = () => {
		const aliases = aliasesInUse();
		return [...new Set(aliases.filter((alias, index) => aliases.indexOf(alias) !== index))];
	};

	const givenToolbar = async (value: UmbTiptapToolbarValue) => {
		const host = await fixture<UmbTestTiptapToolbarConfigurationHostElement>(html`
			<umb-test-tiptap-toolbar-configuration-host>
				<umb-property-editor-ui-tiptap-toolbar-configuration .value=${value}>
				</umb-property-editor-ui-tiptap-toolbar-configuration>
			</umb-test-tiptap-toolbar-configuration-host>
		`);

		element = host.querySelector('umb-property-editor-ui-tiptap-toolbar-configuration')!;
		await element.updateComplete;
		await waitUntil(
			() => element.shadowRoot!.querySelectorAll('.group').length === value.flat().length,
			'the groups of the toolbar were rendered',
		);
	};

	beforeEach(async () => {
		umbExtensionsRegistry.registerMany(manifests);
		await givenToolbar([[['Test.Toolbar.One', 'Test.Toolbar.Two']]]);
	});

	afterEach(() => {
		umbExtensionsRegistry.unregisterMany(manifests.map((manifest) => manifest.alias));
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUiTiptapToolbarConfigurationElement);
	});

	it('appends an item dragged from the available items to the end of the group', async () => {
		await dragAvailableItemToGroup('Test.Toolbar.Three', 0);

		expect(element.value).to.deep.equal([[['Test.Toolbar.One', 'Test.Toolbar.Two', 'Test.Toolbar.Three']]]);
	});

	it('cannot duplicate an item when its group is sorted after it was dragged in', async () => {
		await dragAvailableItemToGroup('Test.Toolbar.Three', 0);
		await dispatchSorterDrop(0);

		expect(occurrencesOf('Test.Toolbar.Three')).to.equal(1);
		expect(duplicatedAliases()).to.be.empty;
	});

	it('cannot duplicate an item into another group when that group is sorted after it was dragged in', async () => {
		await givenToolbar([[['Test.Toolbar.One'], ['Test.Toolbar.Two']]]);

		await dragAvailableItemToGroup('Test.Toolbar.Three', 0);
		await dispatchSorterDrop(1);

		expect(occurrencesOf('Test.Toolbar.Three')).to.equal(1);
		expect(duplicatedAliases()).to.be.empty;
	});

	it('drops the repeat occurrences of an item already duplicated in the value', async () => {
		await givenToolbar([[['Test.Toolbar.One', 'Test.Toolbar.Two', 'Test.Toolbar.One'], ['Test.Toolbar.Two']]]);

		await dragAvailableItemToGroup('Test.Toolbar.Three', 0);

		expect(element.value).to.deep.equal([[['Test.Toolbar.One', 'Test.Toolbar.Two', 'Test.Toolbar.Three'], []]]);
	});
});
