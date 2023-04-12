import { UmbVariantId } from '../../variants/variant-id.class';
import { UmbWorkspaceVariableEntityContextInterface } from '../workspace/workspace-context/workspace-variable-entity-context.interface';
import { UMB_WORKSPACE_VARIANT_CONTEXT_TOKEN } from '../workspace/workspace-variant/workspace-variant.context';
import type { DataTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { ClassState, ObjectState, StringState, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import {
	UmbContextConsumerController,
	UmbContextProviderController,
	UmbContextToken,
	UMB_ENTITY_WORKSPACE_CONTEXT,
} from '@umbraco-cms/backoffice/context-api';

// If we get this from the server then we can consider using TypeScripts Partial<> around the model from the Management-API.
export type WorkspacePropertyData<ValueType> = {
	alias?: string;
	label?: string;
	description?: string;
	value?: ValueType | null;
	config?: DataTypeResponseModel['values']; // This could potentially then come from hardcoded JS object and not the DataType store.
};

export class UmbWorkspacePropertyContext<ValueType = any> {
	#host: UmbControllerHostElement;

	private _providerController: UmbContextProviderController;

	private _data = new ObjectState<WorkspacePropertyData<ValueType>>({});

	public readonly alias = this._data.getObservablePart((data) => data.alias);
	public readonly label = this._data.getObservablePart((data) => data.label);
	public readonly description = this._data.getObservablePart((data) => data.description);
	public readonly value = this._data.getObservablePart((data) => data.value);
	public readonly config = this._data.getObservablePart((data) => data.config);

	#workspaceVariantId?: UmbVariantId;

	#variantId = new ClassState<UmbVariantId | undefined>(undefined);
	public readonly variantId = this.#variantId.asObservable();

	private _variantDifference = new StringState(undefined);
	public readonly variantDifference = this._variantDifference.asObservable();

	private _workspaceContext?: UmbWorkspaceVariableEntityContextInterface;
	private _workspaceVariantConsumer?: UmbContextConsumerController<typeof UMB_WORKSPACE_VARIANT_CONTEXT_TOKEN.TYPE>;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		new UmbContextConsumerController(host, UMB_ENTITY_WORKSPACE_CONTEXT, (workspaceContext) => {
			this._workspaceContext = workspaceContext as UmbWorkspaceVariableEntityContextInterface;
		});

		this._providerController = new UmbContextProviderController(host, UMB_WORKSPACE_PROPERTY_CONTEXT_TOKEN, this);

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
		this._data.update({ alias });
	}
	public setLabel(label: WorkspacePropertyData<ValueType>['label']) {
		this._data.update({ label });
	}
	public setDescription(description: WorkspacePropertyData<ValueType>['description']) {
		this._data.update({ description });
	}
	public setValue(value: WorkspacePropertyData<ValueType>['value']) {
		// Note: Do not try to compare new / old value, as it can of any type. We trust the ObjectState in doing such.
		this._data.update({ value });
	}
	public changeValue(value: WorkspacePropertyData<ValueType>['value']) {
		this.setValue(value);

		const alias = this._data.getValue().alias;
		if (alias) {
			this._workspaceContext?.setPropertyValue(alias, value, this.#variantId.getValue());
		}
	}
	public setConfig(config: WorkspacePropertyData<ValueType>['config']) {
		this._data.update({ config });
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
		this._data.unsubscribe();
		this._providerController.destroy(); // This would also be handled by the controller host, but if someone wanted to replace/remove this context without the host being destroyed. Then we have clean up out selfs here.
	}
}

export const UMB_WORKSPACE_PROPERTY_CONTEXT_TOKEN = new UmbContextToken<UmbWorkspacePropertyContext>(
	'UmbWorkspacePropertyContext'
);
