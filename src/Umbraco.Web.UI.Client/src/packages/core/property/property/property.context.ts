import { UMB_PROPERTY_DATASET_CONTEXT } from '../property-dataset/index.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import {
	UmbArrayState,
	UmbBasicState,
	UmbClassState,
	UmbDeepState,
	UmbStringState,
} from '@umbraco-cms/backoffice/observable-api';
import { UmbContextProviderController, UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbPropertyEditorConfigProperty } from '@umbraco-cms/backoffice/property-editor';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

export class UmbPropertyContext<ValueType = any> extends UmbControllerBase {
	private _providerController: UmbContextProviderController;

	#alias = new UmbStringState(undefined);
	public readonly alias = this.#alias.asObservable();
	#label = new UmbStringState(undefined);
	public readonly label = this.#label.asObservable();
	#description = new UmbStringState(undefined);
	public readonly description = this.#description.asObservable();
	#value = new UmbDeepState<ValueType | undefined>(undefined);
	public readonly value = this.#value.asObservable();
	#configValues = new UmbArrayState<UmbPropertyEditorConfigProperty>([], (x) => x.alias);
	public readonly configValues = this.#configValues.asObservable();

	#configCollection = new UmbClassState<UmbPropertyEditorConfigCollection | undefined>(undefined);
	public readonly config = this.#configCollection.asObservable();

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
		super(host);

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (variantContext) => {
			this.#datasetContext = variantContext;
			this._generateVariantDifferenceString();
			this._observeProperty();
		});

		this.observe(this.alias, () => {
			this._observeProperty();
		});

		this._providerController = new UmbContextProviderController(host, UMB_PROPERTY_CONTEXT, this);

		this.observe(this.configValues, (configValues) => {
			this.#configCollection.setValue(configValues ? new UmbPropertyEditorConfigCollection(configValues) : undefined);
		});

		this.observe(this.variantId, () => {
			this._generateVariantDifferenceString();
		});
	}

	private _observePropertyVariant?: UmbObserverController<UmbVariantId | undefined>;
	private _observePropertyValue?: UmbObserverController<ValueType | undefined>;
	private async _observeProperty(): Promise<void> {
		const alias = this.#alias.getValue();
		if (!this.#datasetContext || !alias) return;

		const variantIdSubject = (await this.#datasetContext.propertyVariantId?.(alias)) ?? undefined;
		this._observePropertyVariant?.destroy();
		if (variantIdSubject) {
			this._observePropertyVariant = this.observe(variantIdSubject, (variantId) => {
				this.#variantId.setValue(variantId);
			});
		}

		// TODO: Verify if we need to optimize runtime by parsing the propertyVariantID, cause this method retrieves it again:
		const subject = await this.#datasetContext.propertyValueByAlias<ValueType>(alias);

		this._observePropertyValue?.destroy();
		if (subject) {
			this._observePropertyValue = this.observe(subject, (value) => {
				this.#value.setValue(value);
			});
		}
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
		this.setValue(undefined); // TODO: We should get the default value from Property Editor maybe even later the DocumentType, as that would hold the default value for the property.
	}

	public destroy(): void {
		super.destroy();
		this.#alias.destroy();
		this.#label.destroy();
		this.#description.destroy();
		this.#configValues.destroy();
		this.#value.destroy();
		this.#configCollection.destroy();
		this._providerController.destroy(); // This would also be handled by the controller host, but if someone wanted to replace/remove this context without the host being destroyed. Then we have clean up out selfs here.
		this.#datasetContext = undefined;
	}
}

export const UMB_PROPERTY_CONTEXT = new UmbContextToken<UmbPropertyContext>('UmbPropertyContext');
