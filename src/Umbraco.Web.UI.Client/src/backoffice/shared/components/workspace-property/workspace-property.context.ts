import { distinctUntilChanged, map, Observable, shareReplay } from "rxjs";
import { UmbWorkspaceContentContext } from "../workspace/workspace-content/workspace-content.context";
import type { DataTypeDetails } from "@umbraco-cms/models";
import { UmbControllerHostInterface } from "src/core/controller/controller-host.mixin";
import { naiveObjectComparison, UniqueBehaviorSubject } from "src/core/observable-api/unique-behavior-subject";
import { UmbContextProviderController } from "src/core/context-api/provide/context-provider.controller";
import { UmbContextConsumerController } from "src/core/context-api/consume/context-consumer.controller";





//TODO: Property-Context: move these methods out:
type MappingFunction<T, R> = (mappable: T) => R;
type MemoizationFunction<R> = (previousResult: R, currentResult: R) => boolean;

function defaultMemoization(previousValue: any, currentValue: any): boolean {
  if (typeof previousValue === 'object' && typeof currentValue === 'object') {
    return naiveObjectComparison(previousValue, currentValue);
  }
  return previousValue === currentValue;
}
export function CreateObservablePart<T, R> (
	source$: Observable<T>,
	mappingFunction: MappingFunction<T, R>,
	memoizationFunction?: MemoizationFunction<R>
): Observable<R> {
	return source$.pipe(
	  map(mappingFunction),
	  distinctUntilChanged(memoizationFunction || defaultMemoization),
	  shareReplay(1)
	)
}





export type WorkspacePropertyData<ValueType> = {
	alias?: string;
	label?: string;
	description?: string;
	value?: ValueType | null;
	config?: DataTypeDetails['data'];// This could potentially then come from hardcoded JS object and not the DataType store.
};

export class UmbWorkspacePropertyContext<ValueType = unknown> {


	private _providerController: UmbContextProviderController;

	private _data: UniqueBehaviorSubject<WorkspacePropertyData<ValueType>> = new UniqueBehaviorSubject({} as WorkspacePropertyData<ValueType>);

	public readonly alias = CreateObservablePart(this._data, data => data.alias);
	public readonly label = CreateObservablePart(this._data, data => data.label);
	public readonly description = CreateObservablePart(this._data, data => data.description);
	public readonly value = CreateObservablePart(this._data, data => data.value);
	public readonly config = CreateObservablePart(this._data, data => data.config);

	private _workspaceContext?: UmbWorkspaceContentContext;


	constructor(host:UmbControllerHostInterface) {

		new UmbContextConsumerController(host, 'umbWorkspaceContext', (workspaceContext) => {
			this._workspaceContext = workspaceContext;
		});

		this._providerController = new UmbContextProviderController(host, 'umbPropertyContext', this);

	
	}
	
	public setAlias(alias: WorkspacePropertyData<ValueType>['alias']) {
		this._data.update({alias: alias});
	}
	public setLabel(label: WorkspacePropertyData<ValueType>['label']) {
		this._data.update({label: label});
	}
	public setDescription(description: WorkspacePropertyData<ValueType>['description']) {
		this._data.update({description: description});
	}
	public setValue(value: WorkspacePropertyData<ValueType>['value']) {

		if(value === this._data.getValue().value) return;

		this._data.update({value: value});

		const alias = this._data.getValue().alias;
		if(alias) {
			this._workspaceContext?.setPropertyValue(alias, value);
		}
	}
	public setConfig(config: WorkspacePropertyData<ValueType>['config']) {
		this._data.update({config: config});
	}

	public resetValue() {
		this.setValue(null);// TODO: Consider if this can be configured/provided from Property Editor or DataType Configuration or even locally specified in DocumentType.
	}

	// TODO: how can we make sure to call this.
	public destroy(): void {
		this._data.unsubscribe();
		this._providerController.destroy(); // This would also be handled by the controller host, but if someone wanted to replace/remove this context without the host being destroyed. Then we have clean up out selfs here.
	}

}

