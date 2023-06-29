import { UmbWorkspaceVariableEntityContextInterface } from '../workspace-context/workspace-variable-entity-context.interface.js';
import { UmbPropertyEditorExtensionElement } from '../../extension-registry/interfaces/property-editor-ui-extension-element.interface.js';
import { type WorkspacePropertyData } from '../types/workspace-property-data.type.js';
import { UMB_WORKSPACE_VARIANT_CONTEXT_TOKEN, UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbClassState,
	UmbObjectState,
	UmbStringState,
	UmbObserverController,
	UmbBasicState,
} from '@umbraco-cms/backoffice/observable-api';
import {
	UmbContextConsumerController,
	UmbContextProviderController,
	UmbContextToken,
} from '@umbraco-cms/backoffice/context-api';
import { UmbDataTypeConfigCollection } from '@umbraco-cms/backoffice/components';

export class UmbWorkspacePropertyContext<ValueType = any> {
	#host: UmbControllerHostElement;

	private _providerController: UmbContextProviderController;

	#data = new UmbObjectState<WorkspacePropertyData<ValueType>>({});

	public readonly alias = this.#data.getObservablePart((data) => data.alias);
	public readonly label = this.#data.getObservablePart((data) => data.label);
	public readonly description = this.#data.getObservablePart((data) => data.description);
	public readonly value = this.#data.getObservablePart((data) => data.value);
	public readonly configValues = this.#data.getObservablePart((data) => data.config);

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

	#workspaceVariantId?: UmbVariantId;

	#variantId = new UmbClassState<UmbVariantId | undefined>(undefined);
	public readonly variantId = this.#variantId.asObservable();

	private _variantDifference = new UmbStringState(undefined);
	public readonly variantDifference = this._variantDifference.asObservable();

	private _workspaceContext?: UmbWorkspaceVariableEntityContextInterface;
	private _workspaceVariantConsumer?: UmbContextConsumerController<typeof UMB_WORKSPACE_VARIANT_CONTEXT_TOKEN.TYPE>;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		new UmbContextConsumerController(host, UMB_ENTITY_WORKSPACE_CONTEXT, (workspaceContext) => {
			this._workspaceContext = workspaceContext as UmbWorkspaceVariableEntityContextInterface;
		});

		this._providerController = new UmbContextProviderController(host, UMB_WORKSPACE_PROPERTY_CONTEXT_TOKEN, this);

		this.configValues.subscribe((configValues) => {
			this.#configCollection.next(configValues ? new UmbDataTypeConfigCollection(configValues) : undefined);
		});

		this.variantId.subscribe((propertyVariantId) => {
			if (propertyVariantId) {
				if (!this._workspaceVariantConsumer) {
					this._workspaceVariantConsumer = new UmbContextConsumerController(
						this.#host,
						UMB_WORKSPACE_VARIANT_CONTEXT_TOKEN,
						(workspaceVariantContext) => {
							new UmbObserverController(this.#host, workspaceVariantContext.variantId, (workspaceVariantId) => {
								this.#workspaceVariantId = workspaceVariantId;
								this._generateVariantDifferenceString();
							});
						}
					);
				} else {
					this._generateVariantDifferenceString();
				}
			}
		});
	}

	private _generateVariantDifferenceString() {
		this._variantDifference.next(
			this.#workspaceVariantId ? this.#variantId.getValue()?.toDifferencesString(this.#workspaceVariantId) : ''
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
	public setValue(value: WorkspacePropertyData<ValueType>['value']) {
		// Note: Do not try to compare new / old value, as it can of any type. We trust the UmbObjectState in doing such.
		this.#data.update({ value });
	}
	public changeValue(value: WorkspacePropertyData<ValueType>['value']) {
		this.setValue(value);

		const alias = this.#data.getValue().alias;
		if (alias) {
			this._workspaceContext?.setPropertyValue(alias, value, this.#variantId.getValue());
		}
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
		this.setValue(null); // TODO: We should get the default value from Property Editor maybe even later the DocumentType, as that would hold the default value for the property.
	}

	public destroy(): void {
		this.#data.unsubscribe();
		this._providerController.destroy(); // This would also be handled by the controller host, but if someone wanted to replace/remove this context without the host being destroyed. Then we have clean up out selfs here.
	}
}

export const UMB_WORKSPACE_PROPERTY_CONTEXT_TOKEN = new UmbContextToken<UmbWorkspacePropertyContext>(
	'UmbWorkspacePropertyContext'
);
