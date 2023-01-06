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
//TODO: Property-Context: rename this method.
export function select$<T, R> (
	source$: Observable<T>,
	mappingFunction: MappingFunction<T, R>,
	memoizationFunction?: MemoizationFunction<R>
): Observable<R> {
	return source$.pipe(
	  map(mappingFunction),
	  distinctUntilChanged(memoizationFunction || defaultMemoization),
	  shareReplay(1) // TODO: investigate what happens if this was removed.
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

	private _data: UniqueBehaviorSubject<WorkspacePropertyData<ValueType>>;

	public readonly alias: Observable<WorkspacePropertyData<ValueType>['alias']>;
	public readonly label: Observable<WorkspacePropertyData<ValueType>['label']>;
	public readonly description: Observable<WorkspacePropertyData<ValueType>['description']>;
	public readonly value: Observable<WorkspacePropertyData<ValueType>['value']>;
	public readonly config: Observable<WorkspacePropertyData<ValueType>['config']>;


	constructor(host:UmbControllerHostInterface) {

		//this._host = host;

		// TODO: How do we connect this value with parent context?
		// Ensuring the property editor value-property is updated...
		// How about consuming a workspace context? When received maybe assuming these will fit or test if it likes to accept this property..

		this._data = new UniqueBehaviorSubject({} as WorkspacePropertyData<ValueType>);

		this.alias = select$(this._data, data => data.alias);
		this.label = select$(this._data, data => data.label);
		this.description = select$(this._data, data => data.description);
		this.value = select$(this._data, data => data.value);
		this.config = select$(this._data, data => data.config);


		new UmbContextProviderController(host, 'umbPropertyContext', this);
	
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
	public setLabel(label: WorkspacePropertyData<ValueType>['label']) {
		this.update({label: label});
	}
	public setDescription(description: WorkspacePropertyData<ValueType>['description']) {
		this.update({description: description});
	}
	public setValue(value: WorkspacePropertyData<ValueType>['value']) {
		this.update({value: value});
	}
	public setConfig(config: WorkspacePropertyData<ValueType>['config']) {
		this.update({config: config});
	}

	public resetValue() {
		console.log("property context reset")
		
		this.update({value: null});
	}

	// TODO: how can we make sure to call this.
	public destroy(): void {
		this._data.unsubscribe();
	}

}

