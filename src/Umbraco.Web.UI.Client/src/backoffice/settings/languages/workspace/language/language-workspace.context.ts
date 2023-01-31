import { UmbLanguageStore, UmbLanguageStoreItemType, UMB_LANGUAGE_STORE_CONTEXT_TOKEN } from '../../language.store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { ObjectState, UmbObserverController } from '@umbraco-cms/observable-api';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';

const DefaultLanguageData: UmbLanguageStoreItemType = {
	name: '',
	isoCode: '',
	isDefault: false,
	isMandatory: false,
};

export class UmbWorkspaceLanguageContext {
	public host: UmbControllerHostInterface;

	private _entityKey: string | null;

	private _data;
	public readonly data;

	private _store: UmbLanguageStore | null = null;
	protected _storeObserver?: UmbObserverController<UmbLanguageStoreItemType>;

	constructor(host: UmbControllerHostInterface, entityKey: string | null) {
		this.host = host;
		this._entityKey = entityKey;

		this._data = new ObjectState(DefaultLanguageData);
		this.data = this._data.asObservable();

		new UmbContextConsumerController(host, UMB_LANGUAGE_STORE_CONTEXT_TOKEN, (_instance: UmbLanguageStore) => {
			this._store = _instance;
			this._observeStore();
		});
	}

	private _observeStore(): void {
		if (!this._store || this._entityKey === null) {
			return;
		}

		this._storeObserver?.destroy();
		this._storeObserver = new UmbObserverController(this.host, this._store.getByIsoCode(this._entityKey), (content) => {
			if (!content) return; // TODO: Handle nicely if there is no content data.
			this.update(content);
		});
	}

	public getData() {
		return this._data.getValue();
	}

	public getAvailableLanguages() {
		//TODO: Don't use !, however this will be changed with the introduction of repositories.
		return this._store!.getAvailable();
	}

	public update(data: Partial<UmbLanguageStoreItemType>) {
		this._data.next({ ...this.getData(), ...data });
	}

	public save(): Promise<void> {
		if (!this._store) {
			// TODO: more beautiful error:
			console.error('Could not save cause workspace context has no store.');
			return Promise.resolve();
		}
		return this._store.save(this.getData());
	}
}
