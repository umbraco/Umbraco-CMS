import { distinctUntilChanged, map, Observable, shareReplay } from "rxjs";
import type { DataTypeDetails } from "@umbraco-cms/models";
import { UmbControllerHostInterface } from "src/core/controller/controller-host.mixin";
import { naiveObjectComparison, UniqueBehaviorSubject } from "src/core/observable-api/unique-behavior-subject";
import { UmbContextProviderController } from "src/core/context-api/provide/context-provider.controller";





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
	  shareReplay(1) // TODO: investigate what happens if this was removed. (its suppose to only give the first subscriber the current value, but i want to test this)
	)
}




export type WorkspacePropertyData<ValueType> = {
	alias?: string;
	label?: string;
	description?: string;
	value?: ValueType | null;
	config?: DataTypeDetails['data'];// This could potentially then come from hardcoded JS object and not the DataType store.
};

export class UmbWorkspacePropertyContext<ValueType> {

	//private _host: UmbControllerHostInterface;

	private _providerController: UmbContextProviderController;

	private _data: UniqueBehaviorSubject<WorkspacePropertyData<ValueType>> = new UniqueBehaviorSubject({} as WorkspacePropertyData<ValueType>);

	public readonly alias = CreateObservablePart(this._data, data => data.alias);
	public readonly label = CreateObservablePart(this._data, data => data.label);
	public readonly description = CreateObservablePart(this._data, data => data.description);
	public readonly value = CreateObservablePart(this._data, data => data.value);
	public readonly config = CreateObservablePart(this._data, data => data.config);

	constructor(host:UmbControllerHostInterface) {

		//this._host = host;

		// TODO: How do we connect this value with parent context?
		// Ensuring the property editor value-property is updated...
		// How about consuming a workspace context? When received maybe assuming these will fit or test if it likes to accept this property..

		this._providerController = new UmbContextProviderController(host, 'umbPropertyContext', this);
	
	}

	/*public getData() {
		return this._data.getValue();
	}*/



	public update(data: Partial<WorkspacePropertyData<ValueType>>) {
		this._data.next({ ...this._data.getValue(), ...data });
	}

	
	public setAlias(alias: WorkspacePropertyData<ValueType>['alias']) {
		this.update({alias: alias});
	}
	public getAlias() {
		return this._data.getValue().alias;
	}
	public setLabel(label: WorkspacePropertyData<ValueType>['label']) {
		this.update({label: label});
	}
	public setDescription(description: WorkspacePropertyData<ValueType>['description']) {
		this.update({description: description});
	}
	public setValue(value: WorkspacePropertyData<ValueType>['value']) {
		this.update({value: value});
	}
	public getValue() {
		return this._data.getValue().value;
	}
	public setConfig(config: WorkspacePropertyData<ValueType>['config']) {
		this.update({config: config});
	}

	public resetValue() {
		this.update({value: null});
	}

	// TODO: how can we make sure to call this.
	public destroy(): void {
		this._data.unsubscribe();
		this._providerController.destroy(); // This would also be handled by the controller host, but if someone wanted to replace/remove this context without the host being destroyed. Then we have clean up out selfs here.
	}

}

