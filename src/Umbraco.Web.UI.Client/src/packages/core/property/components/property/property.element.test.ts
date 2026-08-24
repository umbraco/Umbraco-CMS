import { UmbPropertyElement } from './property.element.js';
import { expect, fixture, html, waitUntil, aTimeout } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestPropertyEditorUi, UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';

const ELEMENT_NAME = 'umb-test-property-editor-ui';
const ALIAS_DEFAULT = 'Umb.Test.PropertyEditorUi.Default';
const ALIAS_SUPPORTS_VARIANT_CHANGE = 'Umb.Test.PropertyEditorUi.SupportsVariantChange';

class UmbTestPropertyEditorUiElement extends HTMLElement implements UmbPropertyEditorUiElement {
	static instanceCount = 0;
	manifest?: ManifestPropertyEditorUi;
	alias?: string;
	name?: string;
	value?: unknown;
	dataSourceAlias?: string;
	readonly?: boolean;
	mandatory?: boolean;
	mandatoryMessage?: string;
	destroyed = false;

	constructor() {
		super();
		UmbTestPropertyEditorUiElement.instanceCount++;
	}

	destroy() {
		this.destroyed = true;
	}
}
customElements.define(ELEMENT_NAME, UmbTestPropertyEditorUiElement);

function manifest(alias: string, supportVariantChange?: boolean): ManifestPropertyEditorUi {
	return {
		type: 'propertyEditorUi',
		alias,
		name: alias,
		elementName: ELEMENT_NAME,
		meta: {
			label: alias,
			icon: 'icon-circle',
			group: 'Common',
			supportsVariantChange: supportVariantChange,
		},
	} as ManifestPropertyEditorUi;
}

const DATA_PATH_EN_US = "$.values[?(@.alias == 'test' && @.culture == 'en-us')].value";
const DATA_PATH_DA_DK = "$.values[?(@.alias == 'test' && @.culture == 'da-dk')].value";

describe('UmbPropertyElement', () => {
	let element: UmbPropertyElement;

	beforeEach(async () => {
		UmbTestPropertyEditorUiElement.instanceCount = 0;
		umbExtensionsRegistry.register(manifest(ALIAS_DEFAULT));
		umbExtensionsRegistry.register(manifest(ALIAS_SUPPORTS_VARIANT_CHANGE, true));
		element = await fixture(html`<umb-property></umb-property>`);
	});

	afterEach(() => {
		umbExtensionsRegistry.unregister(ALIAS_DEFAULT);
		umbExtensionsRegistry.unregister(ALIAS_SUPPORTS_VARIANT_CHANGE);
	});

	async function getEditorElement(): Promise<UmbTestPropertyEditorUiElement> {
		await waitUntil(
			() => element.shadowRoot?.querySelector(ELEMENT_NAME),
			'Property Editor UI element was not created',
		);
		return element.shadowRoot!.querySelector(ELEMENT_NAME) as UmbTestPropertyEditorUiElement;
	}

	describe('variant switching (dataPath change)', () => {
		it('re-creates the Property Editor UI element when the manifest does not declare supportVariantChange', async () => {
			element.propertyEditorUiAlias = ALIAS_DEFAULT;
			element.dataPath = DATA_PATH_EN_US;
			const firstInstance = await getEditorElement();

			element.dataPath = DATA_PATH_DA_DK;
			await waitUntil(
				() => UmbTestPropertyEditorUiElement.instanceCount === 2,
				'Property Editor UI element was not re-created for the new variant',
			);

			const secondInstance = await getEditorElement();
			expect(secondInstance).to.not.equal(firstInstance);
			expect(firstInstance.destroyed).to.be.true;
		});

		it('keeps the same Property Editor UI element when the manifest declares supportsVariantChange: true', async () => {
			element.propertyEditorUiAlias = ALIAS_SUPPORTS_VARIANT_CHANGE;
			element.dataPath = DATA_PATH_EN_US;
			const firstInstance = await getEditorElement();

			element.dataPath = DATA_PATH_DA_DK;
			await aTimeout(20);

			const secondInstance = await getEditorElement();
			expect(secondInstance).to.equal(firstInstance);
			expect(UmbTestPropertyEditorUiElement.instanceCount).to.equal(1);
		});

		it('does not re-create the element when dataPath is set to the same value again', async () => {
			element.propertyEditorUiAlias = ALIAS_DEFAULT;
			element.dataPath = DATA_PATH_EN_US;
			await getEditorElement();

			element.dataPath = DATA_PATH_EN_US;
			await aTimeout(20);

			expect(UmbTestPropertyEditorUiElement.instanceCount).to.equal(1);
		});
	});
});
