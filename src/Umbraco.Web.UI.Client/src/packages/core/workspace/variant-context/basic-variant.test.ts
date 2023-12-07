import { expect, fixture, oneEvent } from '@open-wc/testing';
import type { UmbPropertyValueData } from '../types/property-value-data.type.js';
import { UMB_VARIANT_CONTEXT } from './variant-context.token.js';
import { UmbBasicVariantElement } from './basic-variant.element.js';
import { customElement, html, property, state, LitElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

@customElement('test-property-editor')
export class UmbTestPropertyEditorElement extends UmbElementMixin(LitElement) {
	//
	#variantContext?: typeof UMB_VARIANT_CONTEXT.TYPE;

	@property()
	public get alias() {
		return this._alias;
	}
	public set alias(value) {
		this._alias = value;
		this._observeProperty();
	}
	_alias?: string;

	@state()
	_value?: string;

	getValue() {
		return this._value;
	}
	setValue(value: string) {
		if (this._alias) {
			this.dispatchEvent(new UmbChangeEvent());
			this.#variantContext?.setPropertyValue(this._alias, value);
			this.dispatchEvent(new UmbChangeEvent());
		}
	}

	private async _observeProperty() {
		const alias = this._alias;
		if (!this.#variantContext || !alias) return;
		this.observe(
			await this.#variantContext.propertyValueByAlias(alias),
			(value) => {
				this._value = value as string;
			},
			'observeValue',
		);
	}

	constructor() {
		super();
		this.consumeContext(UMB_VARIANT_CONTEXT, async (variantContext) => {
			this.#variantContext = variantContext;
			this._observeProperty();
		});
	}
}

const dataSet: Array<UmbPropertyValueData> = [
	{
		alias: 'testAlias',
		value: 'testValue',
	},
];

describe('UmbBasicVariantElement', () => {
	describe('Data bindings', () => {
		let variantElement: UmbBasicVariantElement;
		let propertyEditor: UmbTestPropertyEditorElement;

		beforeEach(async () => {
			variantElement = await fixture(
				html`<umb-basic-variant .value=${dataSet}>
					<test-property-editor alias="testAlias"></test-property-editor>
				</umb-basic-variant>`,
			);

			propertyEditor = variantElement.querySelector('test-property-editor') as UmbTestPropertyEditorElement;
		});

		it('basic-variant is defined with its own instance', () => {
			expect(variantElement).to.be.instanceOf(UmbBasicVariantElement);
		});

		it('property editor gets value', () => {
			expect(propertyEditor.alias).to.equal('testAlias');
			expect(propertyEditor.getValue()).to.equal('testValue');
		});

		it('property editor sets value on it self', () => {
			propertyEditor.setValue('testValue2');
			expect(propertyEditor.getValue()).to.equal('testValue2');
		});

		it('property editor sets value on context', () => {
			propertyEditor.setValue('testValue2');
			expect(variantElement.context.getValues()[0].alias).to.equal('testAlias');
			expect(variantElement.context.getValues()[0].value).to.equal('testValue2');
		});

		it('variant element fires change event', async () => {
			const listener = oneEvent(variantElement, UmbChangeEvent.TYPE);

			expect(propertyEditor.alias).to.eq('testAlias');
			propertyEditor.setValue('testValue3');

			const event = (await listener) as unknown as UmbChangeEvent;
			expect(event).to.exist;
			expect(event.type).to.eq(UmbChangeEvent.TYPE);
			expect(event.target).to.equal(variantElement);
		});
	});
});
