import { ContentTreeItem } from '@umbraco-cms/backend-api';
import { UmbTreeDataStore } from '@umbraco-cms/stores/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextToken, UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UniqueBehaviorSubject, UmbObserverController } from '@umbraco-cms/observable-api';
export class UmbCollectionContext<
	DataType extends ContentTreeItem,
	StoreType extends UmbTreeDataStore<DataType> = UmbTreeDataStore<DataType>
> {
	private _host: UmbControllerHostInterface;
	private _entityKey: string | null;

	private _store?: StoreType;
	protected _dataObserver?: UmbObserverController<DataType[]>;

	#data = new UniqueBehaviorSubject(<Array<DataType>>[]);
	public readonly data = this.#data.asObservable();

	#selection = new UniqueBehaviorSubject(<Array<string>>[]);
	public readonly selection = this.#selection.asObservable();

	/*
	TODO:
	private _search = new UniqueBehaviorSubject('');
	public readonly search = this._search.asObservable();
	*/

	constructor(host: UmbControllerHostInterface, entityKey: string | null, storeAlias: string) {
		this._host = host;
		this._entityKey = entityKey;

		new UmbContextConsumerController(this._host, storeAlias, (_instance: StoreType) => {
			this._store = _instance;
			if (!this._store) {
				// TODO: if we keep the type assumption of _store existing, then we should here make sure to break the application in a good way.
				return;
			}
			this._onStoreSubscription();
		});
	}

	/*
	public getData() {
		return this.#data.getValue();
	}
	*/

	/*
	public update(data: Partial<DataType>) {
		this._data.next({ ...this.getData(), ...data });
	}
	*/

	protected _onStoreSubscription(): void {
		if (!this._store) {
			return;
		}

		this._dataObserver?.destroy();

		if (this._entityKey) {
			this._dataObserver = new UmbObserverController(
				this._host,
				this._store.getTreeItemChildren(this._entityKey),
				(nodes) => {
					if (nodes) {
						this.#data.next(nodes);
					}
				}
			);
		} else {
			this._dataObserver = new UmbObserverController(this._host, this._store.getTreeRoot(), (nodes) => {
				if (nodes) {
					this.#data.next(nodes);
				}
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
		this.#selection.next(value);
	}

	public clearSelection() {
		this.#selection.next([]);
	}

	public select(key: string) {
		const selection = this.#selection.getValue();
		this.#selection.next([...selection, key]);
	}

	public deselect(key: string) {
		const selection = this.#selection.getValue();
		this.#selection.next(selection.filter((k) => k !== key));
	}

	// TODO: how can we make sure to call this.
	public destroy(): void {
		this.#data.unsubscribe();
	}
}

export const UMB_COLLECTION_CONTEXT_ALIAS = new UmbContextToken<UmbCollectionContext<any, any>>(
	UmbCollectionContext.name
);
