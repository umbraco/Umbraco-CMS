import { UmbWorkspaceEntityContextInterface } from '../workspace/workspace-context/workspace-entity-context.interface';
import type { DataTypeDetails } from '@umbraco-cms/models';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { createObservablePart, ObjectState } from '@umbraco-cms/observable-api';
import { UmbContextConsumerController, UmbContextProviderController } from '@umbraco-cms/context-api';

// If we get this from the server then we can consider using TypeScripts Partial<> around the model from the Management-API.
export type WorkspacePropertyData<ValueType> = {
	alias?: string;
	label?: string;
	description?: string;
	value?: ValueType | null;
	config?: DataTypeDetails['data']; // This could potentially then come from hardcoded JS object and not the DataType store.
};

export class UmbWorkspacePropertyContext<ValueType = unknown> {
	private _providerController: UmbContextProviderController;

	private _data = new ObjectState<WorkspacePropertyData<ValueType>>({});

	public readonly alias = createObservablePart(this._data, (data) => data.alias);
	public readonly label = createObservablePart(this._data, (data) => data.label);
	public readonly description = createObservablePart(this._data, (data) => data.description);
	public readonly value = createObservablePart(this._data, (data) => data.value);
	public readonly config = createObservablePart(this._data, (data) => data.config);

	private _workspaceContext?: UmbWorkspaceEntityContextInterface;

	constructor(host: UmbControllerHostInterface) {
		// TODO: Figure out how to get the magic string in a better way.
		new UmbContextConsumerController<UmbWorkspaceEntityContextInterface>(host, 'umbWorkspaceContext', (workspaceContext) => {
			this._workspaceContext = workspaceContext;
		});

		this._providerController = new UmbContextProviderController(host, 'umbPropertyContext', this);
	}

	public setAlias(alias: WorkspacePropertyData<ValueType>['alias']) {
		this._data.update({ alias: alias });
	}
	public setLabel(label: WorkspacePropertyData<ValueType>['label']) {
		this._data.update({ label: label });
	}
	public setDescription(description: WorkspacePropertyData<ValueType>['description']) {
		this._data.update({ description: description });
	}
	public setValue(value: WorkspacePropertyData<ValueType>['value']) {
		// Note: Do not try to compare new / old value, as it can of any type. We trust the ObjectState in doing such.

		this._data.update({ value: value });

		const alias = this._data.getValue().alias;
		if (alias) {
			this._workspaceContext?.setPropertyValue(alias, value);
		}
	}
	public setConfig(config: WorkspacePropertyData<ValueType>['config']) {
		this._data.update({ config: config });
	}

	public resetValue() {
		this.setValue(null); // TODO: Consider if this can be configured/provided from Property Editor or DataType Configuration or even locally specified in DocumentType.
	}

	// TODO: how can we make sure to call this.
	public destroy(): void {
		this._data.unsubscribe();
		this._providerController.destroy(); // This would also be handled by the controller host, but if someone wanted to replace/remove this context without the host being destroyed. Then we have clean up out selfs here.
	}
}
