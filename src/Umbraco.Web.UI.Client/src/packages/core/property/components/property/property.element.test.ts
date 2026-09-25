import { UmbPropertyElement } from './property.element.js';
import { expect, fixture, html, waitUntil, aTimeout } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestPropertyEditorUi, UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbValidationContext, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbPropertyDatasetContextBase } from '@umbraco-cms/backoffice/property';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const ELEMENT_NAME = 'umb-test-property-editor-ui';
const ALIAS_DEFAULT = 'Umb.Test.PropertyEditorUi.Default';
const ALIAS_SUPPORTS_VARIANT_CHANGE = 'Umb.Test.PropertyEditorUi.SupportsVariantChange';

// Form-control surface (via the real mixin), so `umb-property` builds a `UmbBindServerValidationToFormControl` for
// this element, as it does for a real Property Editor UI. Without it, `#setupControlValidation()` bails early. [NL]
@customElement(ELEMENT_NAME)
class UmbTestPropertyEditorUiElement
	extends UmbFormControlMixin<unknown, typeof UmbLitElement, undefined>(UmbLitElement, undefined)
	implements UmbPropertyEditorUiElement
{
	static instanceCount = 0;
	manifest?: ManifestPropertyEditorUi;
	alias?: string;
	name?: string;
	dataSourceAlias?: string;
	readonly?: boolean;
	mandatory?: boolean;
	mandatoryMessage?: string;
	destroyed = false;

	/** Simulates a user edit: the editor's own value moves and it announces it, as a real editor does. */
	userEdits(newValue: unknown): void {
		this.value = newValue;
		this.dispatchEvent(new UmbChangeEvent());
	}

	constructor() {
		super();
		UmbTestPropertyEditorUiElement.instanceCount++;
	}

	override destroy() {
		this.destroyed = true;
		super.destroy();
	}
}

@customElement('umb-test-property-host')
class UmbTestPropertyHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	validation = new UmbValidationContext(this);
	datasetContext = new UmbPropertyDatasetContextBase(this);
}

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
	let host: UmbTestPropertyHostElement;
	let element: UmbPropertyElement;

	beforeEach(async () => {
		UmbTestPropertyEditorUiElement.instanceCount = 0;
		umbExtensionsRegistry.register(manifest(ALIAS_DEFAULT));
		umbExtensionsRegistry.register(manifest(ALIAS_SUPPORTS_VARIANT_CHANGE, true));
		host = await fixture(html`<umb-test-property-host><umb-property></umb-property></umb-test-property-host>`);
		element = host.querySelector('umb-property') as UmbPropertyElement;
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

	describe('server validation binding', () => {
		beforeEach(() => {
			element.alias = 'test';
		});

		describe('while a server message exists for the current data path', () => {
			it('flags the property editor element invalid', async () => {
				element.propertyEditorUiAlias = ALIAS_DEFAULT;
				element.dataPath = DATA_PATH_EN_US;
				const editor = await getEditorElement();

				host.validation.messages.addMessage('server', DATA_PATH_EN_US, 'Server says no');
				await aTimeout(20);

				expect(editor.validity.valid).to.be.false;
				expect(editor.validationMessage).to.equal('Server says no');
			});

			// When user switch variant, the editor can stay, which results in a data change — which should not clear the server message. [NL]
			it('keeps the message when a new value arrives from the dataset', async () => {
				element.propertyEditorUiAlias = ALIAS_DEFAULT;
				element.dataPath = DATA_PATH_EN_US;
				await getEditorElement();

				host.validation.messages.addMessage('server', DATA_PATH_EN_US, 'Server says no');
				await aTimeout(20);

				host.datasetContext.setPropertyValue('test', 'a new value pushed from the dataset');
				await aTimeout(20);

				expect(host.validation.messages.getMessages()).to.have.lengthOf(1);
			});

			it('clears the message when the editor element itself reports a change', async () => {
				element.propertyEditorUiAlias = ALIAS_DEFAULT;
				element.dataPath = DATA_PATH_EN_US;
				const editor = await getEditorElement();

				host.validation.messages.addMessage('server', DATA_PATH_EN_US, 'Server says no');
				await aTimeout(20);

				editor.userEdits('edited by the user');

				expect(host.validation.messages.getMessages()).to.have.lengthOf(0);
			});
		});

		describe('moving to another variant’s data path', () => {
			it('keeps the previous variant’s message when the editor supports variant change', async () => {
				element.propertyEditorUiAlias = ALIAS_SUPPORTS_VARIANT_CHANGE;
				element.dataPath = DATA_PATH_DA_DK;
				await getEditorElement();
				host.datasetContext.setPropertyValue('test', 'Dansk værdi');
				await aTimeout(20);

				host.validation.messages.addMessage('server', DATA_PATH_DA_DK, 'Server says no');
				await aTimeout(20);

				// Switching variant pushes the other variant's value through the same, reused element.
				element.dataPath = DATA_PATH_EN_US;
				host.datasetContext.setPropertyValue('test', 'English value');
				await aTimeout(20);

				expect(host.validation.messages.getMessages()).to.have.lengthOf(1);
			});

			it('keeps the previous variant’s message when the editor is re-created', async () => {
				element.propertyEditorUiAlias = ALIAS_DEFAULT;
				element.dataPath = DATA_PATH_DA_DK;
				await getEditorElement();
				host.datasetContext.setPropertyValue('test', 'Dansk værdi');
				await aTimeout(20);

				host.validation.messages.addMessage('server', DATA_PATH_DA_DK, 'Server says no');
				await aTimeout(20);

				element.dataPath = DATA_PATH_EN_US;
				await waitUntil(
					() => UmbTestPropertyEditorUiElement.instanceCount === 2,
					'Property Editor UI element was not re-created for the new variant',
				);
				host.datasetContext.setPropertyValue('test', 'English value');
				await aTimeout(20);

				expect(host.validation.messages.getMessages()).to.have.lengthOf(1);
			});

			it('flags the element for a message belonging to the new data path', async () => {
				element.propertyEditorUiAlias = ALIAS_SUPPORTS_VARIANT_CHANGE;
				element.dataPath = DATA_PATH_DA_DK;
				await getEditorElement();

				element.dataPath = DATA_PATH_EN_US;
				const editor = await getEditorElement();

				host.validation.messages.addMessage('server', DATA_PATH_EN_US, 'Server says no');
				await aTimeout(20);

				expect(editor.validity.valid).to.be.false;
			});
		});

		describe('tearing the binding down', () => {
			it('keeps the message when the property editor UI is replaced', async () => {
				element.propertyEditorUiAlias = ALIAS_DEFAULT;
				element.dataPath = DATA_PATH_EN_US;
				await getEditorElement();

				host.validation.messages.addMessage('server', DATA_PATH_EN_US, 'Server says no');
				await aTimeout(20);

				element.propertyEditorUiAlias = ALIAS_SUPPORTS_VARIANT_CHANGE;
				await aTimeout(20);

				expect(host.validation.messages.getMessages()).to.have.lengthOf(1);
			});

			it('keeps the message when the data path is removed', async () => {
				element.propertyEditorUiAlias = ALIAS_DEFAULT;
				element.dataPath = DATA_PATH_EN_US;
				await getEditorElement();

				host.validation.messages.addMessage('server', DATA_PATH_EN_US, 'Server says no');
				await aTimeout(20);

				element.dataPath = undefined;
				await aTimeout(20);

				expect(host.validation.messages.getMessages()).to.have.lengthOf(1);
			});
		});
	});
});
