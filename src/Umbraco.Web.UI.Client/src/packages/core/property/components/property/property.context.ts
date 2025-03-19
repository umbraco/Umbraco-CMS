import { UMB_PROPERTY_DATASET_CONTEXT } from '../../property-dataset/index.js';
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
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type {
	ManifestPropertyEditorUi,
	UmbPropertyEditorConfigProperty,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type {
	UmbPropertyTypeAppearanceModel,
	UmbPropertyTypeValidationModel,
} from '@umbraco-cms/backoffice/content-type';
import { UmbReadOnlyStateManager } from '@umbraco-cms/backoffice/utils';

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

	#validation = new UmbObjectState<UmbPropertyTypeValidationModel | undefined>(undefined);
	public readonly validation = this.#validation.asObservable();

	public readonly validationMandatory = this.#validation.asObservablePart((x) => x?.mandatory);
	public readonly validationMandatoryMessage = this.#validation.asObservablePart((x) => x?.mandatoryMessage);

	#dataPath = new UmbStringState(undefined);
	public readonly dataPath = this.#dataPath.asObservable();

	#editor = new UmbBasicState<UmbPropertyEditorUiElement | undefined>(undefined);
	public readonly editor = this.#editor.asObservable();

	#editorManifest = new UmbBasicState<ManifestPropertyEditorUi | undefined>(undefined);
	public readonly editorManifest = this.#editorManifest.asObservable();

	public readonly readonlyState = new UmbReadOnlyStateManager(this);
	public readonly isReadOnly = this.readonlyState.isReadOnly;

	/**
	 * Set the property editor UI element for this property.
	 * @param {UmbPropertyEditorUiElement | undefined} editorElement The property editor UI element
	 */
	setEditor(editorElement: UmbPropertyEditorUiElement | undefined) {
		this.#editor.setValue(editorElement ?? undefined);
	}

	/**
	 * Get the property editor UI element for this property.
	 * @returns {UmbPropertyEditorUiElement | undefined} The property editor UI element
	 */
	getEditor(): UmbPropertyEditorUiElement | undefined {
		return this.#editor.getValue();
	}

	/**
	 * Set the property editor manifest for this property.
	 * @param {ManifestPropertyEditorUi | undefined} manifest The property editor manifest
	 */
	setEditorManifest(manifest: ManifestPropertyEditorUi | undefined) {
		this.#editorManifest.setValue(manifest ?? undefined);
	}

	/**
	 * Get the property editor manifest for this property.
	 * @returns {UmbPropertyEditorUiElement | undefined} The property editor manifest
	 */
	getEditorManifest(): ManifestPropertyEditorUi | undefined {
		return this.#editorManifest.getValue();
	}

	// property variant ID:
	#variantId = new UmbClassState<UmbVariantId | undefined>(undefined);
	public readonly variantId = this.#variantId.asObservable();

	#variantDifference = new UmbStringState(undefined);
	public readonly variantDifference = this.#variantDifference.asObservable();

	#datasetContext?: typeof UMB_PROPERTY_DATASET_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host, UMB_PROPERTY_CONTEXT);

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (variantContext) => {
			this.#datasetContext = variantContext;
			this.setVariantId(variantContext.getVariantId?.());
			this._generateVariantDifferenceString();
			this._observeProperty();
		});

		this.observe(
			this.alias,
			() => {
				this._observeProperty();
			},
			null,
		);

		this.observe(
			this.configValues,
			(configValues) => {
				this.#config.setValue(configValues ? new UmbPropertyEditorConfigCollection(configValues) : undefined);
			},
			null,
		);

		this.observe(
			this.variantId,
			() => {
				this._generateVariantDifferenceString();
			},
			null,
		);
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

		this.observe(this.#datasetContext.readOnly, (value) => {
			const unique = 'UMB_DATASET';

			if (value) {
				this.readonlyState.addState({
					unique,
					message: '',
				});
			} else {
				this.readonlyState.removeState(unique);
			}
		});
	}

	private _generateVariantDifferenceString() {
		if (!this.#datasetContext) return;
		const contextVariantId = this.#datasetContext.getVariantId?.() ?? undefined;
		const propertyVariantId = this.#variantId.getValue();

		let shareMessage;
		if (contextVariantId && propertyVariantId) {
			if (contextVariantId.segment !== propertyVariantId.segment) {
				// TODO: Translate this, ideally the actual culture is mentioned in the message:
				shareMessage = 'Shared across culture';
			}
			if (contextVariantId.culture !== propertyVariantId.culture) {
				// TODO: Translate this:
				shareMessage = 'Shared';
			}
		}
		this.#variantDifference.setValue(shareMessage);
	}

	/**
	 * Set the alias of this property.
	 * @param {string | undefined} alias - The alias of the property
	 * @memberof UmbPropertyContext
	 */
	public setAlias(alias: string | undefined): void {
		this.#alias.setValue(alias);
	}

	/**
	 * Get the alias of this property.
	 * @returns {*}  {(string | undefined)}
	 * @memberof UmbPropertyContext
	 */
	public getAlias(): string | undefined {
		return this.#alias.getValue();
	}

	/**
	 * Set the label of this property.
	 * @param {(string | undefined)} label - The label of the property
	 * @memberof UmbPropertyContext
	 */
	public setLabel(label: string | undefined): void {
		this.#label.setValue(label);
	}

	/**
	 * Get the label of this property.
	 * @returns {(string | undefined)} - the label
	 * @memberof UmbPropertyContext
	 */
	public getLabel(): string | undefined {
		return this.#label.getValue();
	}

	/**
	 * Set the description of this property.
	 * @param {(string | undefined)} description
	 * @memberof UmbPropertyContext
	 */
	public setDescription(description: string | undefined): void {
		this.#description.setValue(description);
	}

	/**
	 * Get the description of this property.
	 * @returns {*}  {(string | undefined)}
	 * @memberof UmbPropertyContext
	 */
	public getDescription(): string | undefined {
		return this.#description.getValue();
	}

	/**
	 * Set the appearance of this property.
	 * @param {UmbPropertyTypeAppearanceModel | undefined} appearance - the appearance properties of this property
	 * @memberof UmbPropertyContext
	 */
	public setAppearance(appearance: UmbPropertyTypeAppearanceModel | undefined): void {
		this.#appearance.setValue(appearance);
	}

	/**
	 * Get the appearance of this property.
	 * @returns {UmbPropertyTypeAppearanceModel | undefined}- the appearance properties of this property
	 * @memberof UmbPropertyContext
	 */
	public getAppearance(): UmbPropertyTypeAppearanceModel | undefined {
		return this.#appearance.getValue();
	}

	/**
	 * Set the value of this property.
	 * @param {unknown} value - the whole value to be set
	 */
	public setValue(value: ValueType | undefined): void {
		const alias = this.#alias.getValue();
		if (!this.#datasetContext || !alias) return;
		this.#datasetContext?.setPropertyValue(alias, value);
	}

	/**
	 * Gets the current value of this property.
	 * Notice this is not reactive, you should us the `value` observable for that.
	 * @returns {unknown} - the current value of this property
	 */
	public getValue(): ValueType | undefined {
		return this.#value.getValue();
	}

	/**
	 * Set the config of this property.
	 * @param {Array<UmbPropertyEditorConfigProperty> | undefined} config - Array of configurations for this property
	 * @memberof UmbPropertyContext
	 */
	public setConfig(config: Array<UmbPropertyEditorConfigProperty> | undefined): void {
		this.#configValues.setValue(config ?? []);
	}

	/**
	 * Get the config of this property.
	 * @returns {Array<UmbPropertyEditorConfigProperty> | undefined} - Array of configurations for this property
	 * @memberof UmbPropertyContext
	 */
	public getConfig(): Array<UmbPropertyEditorConfigProperty> | undefined {
		return this.#configValues.getValue();
	}

	/**
	 * Set the variant ID of this property.
	 * @param {UmbVariantId | undefined} variantId - The property Variant ID, not necessary the same as the Property Dataset Context VariantId.
	 * @memberof UmbPropertyContext
	 */
	public setVariantId(variantId: UmbVariantId | undefined): void {
		this.#variantId.setValue(variantId);
	}

	/**
	 * Get the variant ID of this property.
	 * @returns {UmbVariantId | undefined} - The property Variant ID, not necessary the same as the Property Dataset Context VariantId.
	 * @memberof UmbPropertyContext
	 */
	public getVariantId(): UmbVariantId | undefined {
		return this.#variantId.getValue();
	}

	/**
	 * Set the validation of this property.
	 * @param {UmbPropertyTypeValidationModel | undefined} validation - Object holding the Validation Properties.
	 * @memberof UmbPropertyContext
	 */
	public setValidation(validation: UmbPropertyTypeValidationModel | undefined): void {
		this.#validation.setValue(validation);
	}

	/**
	 * Get the validation of this property.
	 * @returns {UmbPropertyTypeValidationModel | undefined} - Object holding the Validation Properties.
	 * @memberof UmbPropertyContext
	 */
	public getValidation(): UmbPropertyTypeValidationModel | undefined {
		return this.#validation.getValue();
	}

	/**
	 * Get the read only state of this property
	 * @returns {boolean} - If property is in read-only mode.
	 * @memberof UmbPropertyContext
	 */
	public getIsReadOnly(): boolean {
		return this.readonlyState.getIsReadOnly();
	}

	public setDataPath(dataPath: string | undefined): void {
		this.#dataPath.setValue(dataPath);
	}

	public getDataPath(): string | undefined {
		return this.#dataPath.getValue();
	}

	/**
	 * Reset the value of this property.
	 * @memberof UmbPropertyContext
	 */
	public resetValue(): void {
		this.setValue(undefined); // TODO: We should get the value from the server aka. the value from the persisted data. (Most workspaces holds this data, via dataset) [NL]
	}

	/**
	 * Clear the value of this property.
	 * @memberof UmbPropertyContext
	 */
	public clearValue(): void {
		this.setValue(undefined); // TODO: We should get the default value from Property Editor maybe even later the DocumentType, as that would hold the default value for the property. (Get it via the dataset) [NL]
	}

	public override destroy(): void {
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
