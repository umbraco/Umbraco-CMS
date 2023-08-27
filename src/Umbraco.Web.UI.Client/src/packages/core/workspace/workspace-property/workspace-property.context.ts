import { UmbPropertyEditorExtensionElement } from '../../extension-registry/interfaces/property-editor-ui-extension-element.interface.js';
import { type WorkspacePropertyData } from '../types/workspace-property-data.type.js';
import { UMB_DATASET_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbBaseController, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbClassState,
	UmbObjectState,
	UmbStringState,
	UmbObserverController,
	UmbBasicState,
} from '@umbraco-cms/backoffice/observable-api';
import {
	UmbContextProviderController,
	UmbContextToken,
} from '@umbraco-cms/backoffice/context-api';
import { UmbDataTypeConfigCollection } from '@umbraco-cms/backoffice/components';

export class UmbWorkspacePropertyContext<ValueType = any> extends UmbBaseController {

	private _providerController: UmbContextProviderController;

	#data = new UmbObjectState<WorkspacePropertyData<ValueType>>({});

	public readonly alias = this.#data.asObservablePart((data) => data.alias);
	public readonly label = this.#data.asObservablePart((data) => data.label);
	public readonly description = this.#data.asObservablePart((data) => data.description);
	public readonly value = this.#data.asObservablePart((data) => data.value);
	public readonly configValues = this.#data.asObservablePart((data) => data.config);

	#configCollection = new UmbClassState<UmbDataTypeConfigCollection | undefined>(undefined);
	public readonly config = this.#configCollection.asObservable();

	private _editor = new UmbBasicState<UmbPropertyEditorExtensionElement | undefined>(undefined);
	public readonly editor = this._editor.asObservable();
	setEditor(editor: UmbPropertyEditorExtensionElement | undefined) {
		this._editor.next(editor ?? undefined);
	}
	getEditor() {
		return this._editor.getValue();
	}

	// property variant ID:
	#variantId = new UmbClassState<UmbVariantId | undefined>(undefined);
	public readonly variantId = this.#variantId.asObservable();

	private _variantDifference = new UmbStringState(undefined);
	public readonly variantDifference = this._variantDifference.asObservable();

	#datasetContext?: typeof UMB_DATASET_CONTEXT.TYPE;

	constructor(host: UmbControllerHostElement) {
		super(host);

		this.consumeContext(UMB_DATASET_CONTEXT, (datasetContext) => {
			this.#datasetContext = datasetContext;
			this._generateVariantDifferenceString();
			this._observeProperty();
		});

		this._providerController = new UmbContextProviderController(host, UMB_WORKSPACE_PROPERTY_CONTEXT_TOKEN, this);

		this.observe(this.configValues, (configValues) => {
			this.#configCollection.next(configValues ? new UmbDataTypeConfigCollection(configValues) : undefined);
		});

		this.observe(this.variantId, () => {
			this._generateVariantDifferenceString();
		});
	}


	private _observePropertyVariant?: UmbObserverController<UmbVariantId | undefined>;
	private _observePropertyValue?: UmbObserverController<ValueType | undefined>;
	private async _observeProperty() {
		const alias = this.#data.getValue().alias;
		if (!this.#datasetContext || !alias) return;

		const variantIdSubject = await this.#datasetContext.propertyVariantId?.(alias) ?? undefined;
		this._observePropertyVariant?.destroy();
		if(variantIdSubject) {
			this._observePropertyVariant = this.observe(
				variantIdSubject,
				(variantId) => {
					this.#variantId.next(variantId);
				}
			);
		}

		// TODO: Verify if we need to optimize runtime by parsing the propertyVariantID, cause this method retrieves it again:
		const subject = await this.#datasetContext.propertyValueByAlias<ValueType>(alias)

		this._observePropertyValue?.destroy();
		if(subject) {
			this._observePropertyValue = this.observe(
				subject,
				(value) => {
					// Note: Do not try to compare new / old value, as it can of any type. We trust the UmbObjectState in doing such.
					this.#data.update({ value });
				}
			);
		}
	}

	private _generateVariantDifferenceString() {
		if(!this.#datasetContext) return;
		const datasetVariantId = this.#datasetContext.getVariantId?.() ?? undefined;
		this._variantDifference.next(
			datasetVariantId ? this.#variantId.getValue()?.toDifferencesString(datasetVariantId) : ''
		);
	}

	public setAlias(alias: WorkspacePropertyData<ValueType>['alias']) {
		this.#data.update({ alias });
	}
	public setLabel(label: WorkspacePropertyData<ValueType>['label']) {
		this.#data.update({ label });
	}
	public setDescription(description: WorkspacePropertyData<ValueType>['description']) {
		this.#data.update({ description });
	}
	// TODO: Refactor: consider rename to setValue:
	public changeValue(value: WorkspacePropertyData<ValueType>['value']) {
		const alias = this.#data.getValue().alias;
		if (!this.#datasetContext || !alias) return;

		this.#datasetContext?.setPropertyValue(alias, value);
	}
	public setConfig(config: WorkspacePropertyData<ValueType>['config'] | undefined) {
		this.#data.update({ config });
	}
	public setVariantId(variantId: UmbVariantId | undefined) {
		this.#variantId.next(variantId);
	}
	public getVariantId() {
		return this.#variantId.getValue();
	}

	public resetValue() {
		this.changeValue(null); // TODO: We should get the default value from Property Editor maybe even later the DocumentType, as that would hold the default value for the property.
	}

	public destroy(): void {
		this.#data.unsubscribe();
		this._providerController.destroy(); // This would also be handled by the controller host, but if someone wanted to replace/remove this context without the host being destroyed. Then we have clean up out selfs here.
	}
}

export const UMB_WORKSPACE_PROPERTY_CONTEXT_TOKEN = new UmbContextToken<UmbWorkspacePropertyContext>(
	'UmbWorkspacePropertyContext'
);
