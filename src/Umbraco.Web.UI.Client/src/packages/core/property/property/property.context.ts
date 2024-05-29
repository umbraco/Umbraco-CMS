import { UMB_PROPERTY_DATASET_CONTEXT } from '../property-dataset/index.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import {
	UmbArrayState,
	UmbBasicState,
	UmbClassState,
	UmbDeepState,
	UmbObjectState,
	UmbStringState,
} from '@umbraco-cms/backoffice/observable-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbPropertyEditorConfigProperty } from '@umbraco-cms/backoffice/property-editor';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbPropertyTypeAppearanceModel } from '@umbraco-cms/backoffice/content-type';

export class UmbPropertyContext<ValueType = any> extends UmbContextBase<UmbPropertyContext<ValueType>> {
	#alias = new UmbStringState(undefined);
	public readonly alias = this.#alias.asObservable();
	#label = new UmbStringState(undefined);
	public readonly label = this.#label.asObservable();
	#description = new UmbStringState(undefined);
	public readonly description = this.#description.asObservable();
	#appearance = new UmbObjectState<UmbPropertyTypeAppearanceModel | undefined>(undefined);
	public readonly appearance = this.#appearance.asObservable();
	#value = new UmbDeepState<ValueType | undefined>(undefined);
	public readonly value = this.#value.asObservable();
	#configValues = new UmbArrayState<UmbPropertyEditorConfigProperty>([], (x) => x.alias);
	public readonly configValues = this.#configValues.asObservable();

	#config = new UmbClassState<UmbPropertyEditorConfigCollection | undefined>(undefined);
	public readonly config = this.#config.asObservable();

	private _editor = new UmbBasicState<UmbPropertyEditorUiElement | undefined>(undefined);
	public readonly editor = this._editor.asObservable();
	setEditor(editor: UmbPropertyEditorUiElement | undefined) {
		this._editor.setValue(editor ?? undefined);
	}
	getEditor() {
		return this._editor.getValue();
	}

	// property variant ID:
	#variantId = new UmbClassState<UmbVariantId | undefined>(undefined);
	public readonly variantId = this.#variantId.asObservable();

	private _variantDifference = new UmbStringState(undefined);
	public readonly variantDifference = this._variantDifference.asObservable();

	#datasetContext?: typeof UMB_PROPERTY_DATASET_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host, UMB_PROPERTY_CONTEXT);

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (variantContext) => {
			this.#datasetContext = variantContext;
			this._generateVariantDifferenceString();
			this._observeProperty();
		});

		this.observe(this.alias, () => {
			this._observeProperty();
		});

		this.observe(this.configValues, (configValues) => {
			this.#config.setValue(configValues ? new UmbPropertyEditorConfigCollection(configValues) : undefined);
		});

		this.observe(this.variantId, () => {
			this._generateVariantDifferenceString();
		});
	}

	private async _observeProperty(): Promise<void> {
		const alias = this.#alias.getValue();
		if (!this.#datasetContext || !alias) return;

		this.observe(
			await this.#datasetContext.propertyVariantId?.(alias),
			(variantId) => {
				this.#variantId.setValue(variantId);
			},
			'observeVariantId',
		);

		this.observe(
			await this.#datasetContext.propertyValueByAlias<ValueType>(alias),
			(value) => {
				this.#value.setValue(value);
			},
			'observeValue',
		);
	}

	private _generateVariantDifferenceString() {
		if (!this.#datasetContext) return;
		const contextVariantId = this.#datasetContext.getVariantId?.() ?? undefined;
		this._variantDifference.setValue(
			contextVariantId ? this.#variantId.getValue()?.toDifferencesString(contextVariantId) : '',
		);
	}

	public setAlias(alias: string | undefined): void {
		this.#alias.setValue(alias);
	}
	public getAlias(): string | undefined {
		return this.#alias.getValue();
	}
	public setLabel(label: string | undefined): void {
		this.#label.setValue(label);
	}
	public getLabel(): string | undefined {
		return this.#label.getValue();
	}
	public setDescription(description: string | undefined): void {
		this.#description.setValue(description);
	}
	public getDescription(): string | undefined {
		return this.#description.getValue();
	}
	public setAppearance(appearance: UmbPropertyTypeAppearanceModel | undefined): void {
		this.#appearance.setValue(appearance);
	}
	public getAppearance(): UmbPropertyTypeAppearanceModel | undefined {
		return this.#appearance.getValue();
	}
	/**
	 * Set the value of this property.
	 * @param value {ValueType} the whole value to be set
	 */
	public setValue(value: ValueType | undefined): void {
		const alias = this.#alias.getValue();
		if (!this.#datasetContext || !alias) return;
		this.#datasetContext?.setPropertyValue(alias, value);
	}
	/**
	 * Gets the current value of this property.
	 * Notice this is not reactive, you should us the `value` observable for that.
	 * @returns {ValueType}
	 */
	public getValue(): ValueType | undefined {
		return this.#value.getValue();
	}
	public setConfig(config: Array<UmbPropertyEditorConfigProperty> | undefined): void {
		this.#configValues.setValue(config ?? []);
	}
	public getConfig(): Array<UmbPropertyEditorConfigProperty> | undefined {
		return this.#configValues.getValue();
	}
	public setVariantId(variantId: UmbVariantId | undefined): void {
		this.#variantId.setValue(variantId);
	}
	public getVariantId(): UmbVariantId | undefined {
		return this.#variantId.getValue();
	}

	public resetValue(): void {
		this.setValue(undefined); // TODO: We should get the value from the server aka. the value from the persisted data. (Most workspaces holds this data, via dataset) [NL]
	}
	public clearValue(): void {
		this.setValue(undefined); // TODO: We should get the default value from Property Editor maybe even later the DocumentType, as that would hold the default value for the property. (Get it via the dataset) [NL]
	}

	public destroy(): void {
		super.destroy();
		this.#alias.destroy();
		this.#label.destroy();
		this.#description.destroy();
		this.#appearance.destroy();
		this.#configValues.destroy();
		this.#value.destroy();
		this.#config.destroy();
		this.#datasetContext = undefined;
	}
}

export const UMB_PROPERTY_CONTEXT = new UmbContextToken<UmbPropertyContext>('UmbPropertyContext');
