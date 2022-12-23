import { BehaviorSubject, Observable, Subscription } from "rxjs";
import { ContentTreeItem } from "@umbraco-cms/backend-api";
import { UmbContextConsumer } from "@umbraco-cms/context-api";
import { UmbTreeDataStore } from "@umbraco-cms/stores/store";

export class UmbCollectionContext<DataType extends ContentTreeItem, StoreType extends UmbTreeDataStore<DataType> = UmbTreeDataStore<DataType>> {


	private _target: HTMLElement;
	private _entityKey:string|null;


	protected _storeConsumer!:UmbContextConsumer;
	private _store?: StoreType;

	private _data: BehaviorSubject<Array<DataType>> = new BehaviorSubject(<Array<DataType>>[]);
	public readonly data: Observable<Array<DataType>> = this._data.asObservable();
	protected _dataObserver?:Subscription;

	
	private _selection: BehaviorSubject<Array<string>> = new BehaviorSubject(<Array<string>>[]);
	public readonly selection: Observable<Array<string>> = this._selection.asObservable();

	/*
	TODO:
	private _search: BehaviorSubject<string> = new BehaviorSubject('');
	public readonly search: Observable<string> = this._search.asObservable();
	*/

	constructor(target:HTMLElement, entityKey: string|null, storeAlias:string) {
		this._target = target;
		this._entityKey = entityKey;

		this._storeConsumer = new UmbContextConsumer(this._target, storeAlias, (_instance: StoreType) => {
			this._store = _instance;
			if(!this._store) {
				// TODO: if we keep the type assumption of _store existing, then we should here make sure to break the application in a good way.
				return;
			}
			this._onStoreSubscription();
		});
	}

	connectedCallback() {
		this._storeConsumer.attach();
	}

	disconnectedCallback() {
		this._storeConsumer.detach();
	}


	public getData() {
		return this._data.getValue();
	}

	/*
	public update(data: Partial<DataType>) {
		this._data.next({ ...this.getData(), ...data });
	}
	*/

	protected _onStoreSubscription(): void {
		if(!this._store) {
			return;
		}

		
		if (this._entityKey) {
			this._dataObserver = this._store.getTreeItemChildren(this._entityKey).subscribe((nodes) => {
				this._data.next(nodes);
			});
		} else {
			this._dataObserver = this._store.getTreeRoot().subscribe((nodes) => {
				this._data.next(nodes);
			});
		}
	}



	/*
	TODO:
	public setSearch(value: string) {
		if (!value) value = '';

		this._search.next(value);
	}
	*/
	

	public setSelection(value: Array<string>) {
		if (!value) return;
		this._selection.next(value);
	}

	public clearSelection() {
		this._selection.next([]);
	}

	public select(key: string) {
		const selection = this._selection.getValue();
		this._selection.next([...selection, key]);
	}

	public deselect(key: string) {
		const selection = this._selection.getValue();
		this._selection.next(selection.filter((k) => k !== key));
	}




	// TODO: how can we make sure to call this.
	public destroy(): void {
		this._data.unsubscribe();
	}

}

