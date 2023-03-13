import { UmbVariantId } from '../../variants/variant-id.class';
import { UmbWorkspaceVariableEntityContextInterface } from '../workspace/workspace-context/workspace-variable-entity-context.interface';
import { UMB_WORKSPACE_VARIANT_CONTEXT_TOKEN } from '../workspace/workspace-variant/workspace-variant.context';
import type { DataTypeModel } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { ObjectState, StringState, UmbObserverController } from '@umbraco-cms/observable-api';
import { UmbContextConsumerController, UmbContextProviderController } from '@umbraco-cms/context-api';

// If we get this from the server then we can consider using TypeScripts Partial<> around the model from the Management-API.
export type WorkspacePropertyData<ValueType> = {
	alias?: string;
	label?: string;
	description?: string;
	value?: ValueType | null;
	config?: DataTypeModel['data']; // This could potentially then come from hardcoded JS object and not the DataType store.
};

export class UmbWorkspacePropertyContext<ValueType = unknown> {
	#host: UmbControllerHostInterface;

	private _providerController: UmbContextProviderController;

	private _data = new ObjectState<WorkspacePropertyData<ValueType>>({});

	public readonly alias = this._data.getObservablePart((data) => data.alias);
	public readonly label = this._data.getObservablePart((data) => data.label);
	public readonly description = this._data.getObservablePart((data) => data.description);
	public readonly value = this._data.getObservablePart((data) => data.value);
	public readonly config = this._data.getObservablePart((data) => data.config);

	private _variantId?: UmbVariantId;

	private _variantDifference = new StringState(undefined);
	public readonly variantDifference = this._variantDifference.asObservable();

	private _workspaceContext?: UmbWorkspaceVariableEntityContextInterface;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
		// TODO: Figure out how to get the magic string in a better way.
		new UmbContextConsumerController<UmbWorkspaceVariableEntityContextInterface>(
			host,
			'umbWorkspaceContext',
			(workspaceContext) => {
				this._workspaceContext = workspaceContext;
			}
		);

		this._providerController = new UmbContextProviderController(host, 'umbPropertyContext', this);
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
			this._workspaceContext?.setPropertyValue(alias, value, this._variantId);
		}
	}
	public setConfig(config: WorkspacePropertyData<ValueType>['config']) {
		this._data.update({ config });
	}
	public setVariantId(variantId: UmbVariantId | undefined) {
		this._variantId = variantId;
		new UmbContextConsumerController(this.#host, UMB_WORKSPACE_VARIANT_CONTEXT_TOKEN, (variantContext) => {
			new UmbObserverController(this.#host, variantContext.variantId, (variantId) => {
				this._variantDifference.next(variantId ? this._variantId?.toDifferencesString(variantId) : '');
			});
		});
	}
	public getVariantId() {
		return this._variantId;
	}

	public resetValue() {
		this.setValue(null); // TODO: We should get the default value from Property Editor maybe even later the DocumentType, as that would hold the default value for the property.
	}

	public destroy(): void {
		this._data.unsubscribe();
		this._providerController.destroy(); // This would also be handled by the controller host, but if someone wanted to replace/remove this context without the host being destroyed. Then we have clean up out selfs here.
	}
}
