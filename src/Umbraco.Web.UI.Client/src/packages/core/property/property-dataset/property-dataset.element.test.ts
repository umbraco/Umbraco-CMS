import type { UmbPropertyValueData } from '../types/property-value-data.type.js';
import { UMB_PROPERTY_DATASET_CONTEXT } from './property-dataset-context.token.js';
import { UmbPropertyDatasetElement } from './property-dataset.element.js';
import { expect, fixture, oneEvent } from '@open-wc/testing';
import { customElement, html, property, state, LitElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

@customElement('test-property-editor')
export class UmbTestPropertyEditorElement extends UmbElementMixin(LitElement) {
	//
	_datasetContext?: typeof UMB_PROPERTY_DATASET_CONTEXT.TYPE;

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
			this._datasetContext?.setPropertyValue(this._alias, value);
			this.dispatchEvent(new UmbChangeEvent());
		}
	}

	protected async _observeProperty() {
		if (!this._datasetContext || !this._alias) return;
		this.observe(
			await this._datasetContext.propertyValueByAlias(this._alias),
			(value) => {
				this._value = value as string;
			},
			'observeValue',
		);
	}

	constructor() {
		super();
		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (variantContext) => {
			this._datasetContext = variantContext;
			this._observeProperty();
		});
	}
}

// Adapter property editor, which tests what happens if a editor sets values of other editors.
@customElement('test-adapter-property-editor')
export class UmbTestAdapterPropertyEditorElement extends UmbTestPropertyEditorElement {
	protected override async _observeProperty() {
		super._observeProperty();
		if (!this._datasetContext || !this._alias) return;
		this.observe(
			await this._datasetContext.propertyValueByAlias(this._alias),
			(value) => {
				this._datasetContext?.setPropertyValue('testAlias', 'setByAdapter_' + value);
			},
			'observeValue',
		);
	}
}

const dataSet: Array<UmbPropertyValueData> = [
	{
		alias: 'testAlias',
		value: 'testValue',
	},
];

describe('UmbBasicVariantElement', () => {
	describe('Public API', () => {
		let datasetElement: UmbPropertyDatasetElement;

		beforeEach(async () => {
			datasetElement = await fixture(
				html`<umb-property-dataset .name=${'hi'} .value=${dataSet}> </umb-property-dataset>`,
			);
		});

		describe('properties', () => {
			it('has a value property', () => {
				expect(datasetElement).to.have.property('value').to.be.an.a('array');
			});

			it('has a name property', () => {
				expect(datasetElement).to.have.property('name').to.be.an.a('string');
			});
		});
	});

	describe('Data bindings', () => {
		let datasetElement: UmbPropertyDatasetElement;
		let propertyEditor: UmbTestPropertyEditorElement;

		beforeEach(async () => {
			datasetElement = await fixture(
				html`<umb-property-dataset .value=${dataSet}>
					<test-property-editor alias="testAlias"></test-property-editor>
				</umb-property-dataset>`,
			);

			propertyEditor = datasetElement.querySelector('test-property-editor') as UmbTestPropertyEditorElement;
		});

		it('defines with its own instance', () => {
			expect(datasetElement).to.be.instanceOf(UmbPropertyDatasetElement);
		});

		it('provides the value for the property editor to get', () => {
			expect(propertyEditor.alias).to.equal('testAlias');
			expect(propertyEditor.getValue()).to.equal('testValue');
		});

		it('property editor sets value on it self', () => {
			propertyEditor.setValue('testValue2');
			expect(propertyEditor.getValue()).to.equal('testValue2');
		});

		it('retrieves value set by child', () => {
			propertyEditor.setValue('testValue2');
			expect(datasetElement.context.getValues()[0].alias).to.equal('testAlias');
			expect(datasetElement.context.getValues()[0].value).to.equal('testValue2');
		});

		it('fires change event', async () => {
			const listener = oneEvent(datasetElement, UmbChangeEvent.TYPE);

			expect(propertyEditor.alias).to.eq('testAlias');
			propertyEditor.setValue('testValue3');

			const event = (await listener) as unknown as UmbChangeEvent;
			expect(event).to.exist;
			expect(event.type).to.eq(UmbChangeEvent.TYPE);
			expect(event.target).to.equal(datasetElement);
		});

		it('does respond to changes triggered internally', async () => {
			const adapterPropertyEditor = document.createElement(
				'test-adapter-property-editor',
			) as UmbTestAdapterPropertyEditorElement;

			// We actually do not use the alias of the adapter property editor, but we need to set this to start observing the property value:
			adapterPropertyEditor.alias = 'testAdapterAlias';
			datasetElement.appendChild(adapterPropertyEditor);

			const listener = oneEvent(datasetElement, UmbChangeEvent.TYPE);

			// The alias of the original property editor must be 'testAlias' for the adapter to set the value of it.
			expect(propertyEditor.alias).to.eq('testAlias');
			expect(adapterPropertyEditor.alias).to.eq('testAdapterAlias');
			adapterPropertyEditor.setValue('testValue4');

			const event = (await listener) as unknown as UmbChangeEvent;
			expect(event).to.exist;
			expect(event.type).to.eq(UmbChangeEvent.TYPE);
			expect(event.target).to.equal(datasetElement);
			expect(adapterPropertyEditor.getValue()).to.equal('testValue4');
			expect(propertyEditor.getValue()).to.equal('setByAdapter_testValue4');
		});
	});
});
