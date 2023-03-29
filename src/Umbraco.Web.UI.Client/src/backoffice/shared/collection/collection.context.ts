import { Observable } from 'rxjs';
import type { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbContextToken, UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { ArrayState, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { umbExtensionsRegistry, createExtensionClass } from '@umbraco-cms/backoffice/extensions-api';
import { UmbTreeRepository } from '@umbraco-cms/backoffice/repository';

// TODO: Clean up the need for store as Media has switched to use Repositories(repository).
export class UmbCollectionContext<DataType extends EntityTreeItemResponseModel = EntityTreeItemResponseModel> {
	private _host: UmbControllerHostElement;
	private _entityType: string | null;
	private _entityKey: string | null;

	#repository?: UmbTreeRepository<any>;

	private _store?: any;
	protected _dataObserver?: UmbObserverController<DataType[]>;

	#data = new ArrayState(<Array<DataType>>[]);
	public readonly data = this.#data.asObservable();

	#selection = new ArrayState(<Array<string>>[]);
	public readonly selection = this.#selection.asObservable();

	/*
	TODO:
	private _search = new StringState('');
	public readonly search = this._search.asObservable();
	*/

	constructor(
		host: UmbControllerHostElement,
		entityType: string | null,
		entityKey: string | null,
		storeAlias?: string,
		repositoryAlias?: string
	) {
		this._entityType = entityType;
		this._host = host;
		this._entityKey = entityKey;

		if (storeAlias) {
			new UmbContextConsumerController(this._host, storeAlias, (_instance) => {
				this._store = _instance;
				if (!this._store) {
					// TODO: if we keep the type assumption of _store existing, then we should here make sure to break the application in a good way.
					return;
				}
				this._onStoreSubscription();
			});
		} else if (repositoryAlias) {
			new UmbObserverController(
				this._host,
				umbExtensionsRegistry.getByTypeAndAlias('repository', repositoryAlias),
				async (repositoryManifest) => {
					if (repositoryManifest) {
						// TODO: use the right interface here, we might need a collection repository interface.
						const result = await createExtensionClass<UmbTreeRepository>(repositoryManifest, [this._host]);
						this.#repository = result;
						this._onRepositoryReady();
					}
				}
			);
		}
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

	public getEntityType() {
		return this._entityType;
	}

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

	protected async _onRepositoryReady() {
		if (!this.#repository) {
			return;
		}

		this._dataObserver?.destroy();

		if (this._entityKey) {
			// TODO: we should be able to get an observable from this call. either return a observable or a asObservable() method.
			const observable = (await this.#repository.requestTreeItemsOf(this._entityKey)).asObservable?.();

			if (observable) {
				this._dataObserver = new UmbObserverController(this._host, observable as Observable<DataType[]>, (nodes) => {
					if (nodes) {
						this.#data.next(nodes);
					}
				});
			}
		} else {
			const observable = (await this.#repository.requestRootTreeItems()).asObservable?.();

			if (observable) {
				this._dataObserver = new UmbObserverController(this._host, observable as Observable<DataType[]>, (nodes) => {
					if (nodes) {
						this.#data.next(nodes);
					}
				});
			}
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
		this.#selection.appendOne(key);
	}

	public deselect(key: string) {
		this.#selection.filter((k) => k !== key);
	}

	// TODO: how can we make sure to call this.
	public destroy(): void {
		this.#data.unsubscribe();
	}
}

export const UMB_COLLECTION_CONTEXT_TOKEN = new UmbContextToken<UmbCollectionContext<any>>('UmbCollectionContext');
