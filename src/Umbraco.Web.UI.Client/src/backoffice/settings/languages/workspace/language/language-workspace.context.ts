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

	#entityKey: string | null;

	#data;
	public readonly data;

	#store: UmbLanguageStore | null = null;
	protected _storeObserver?: UmbObserverController<UmbLanguageStoreItemType>;

	constructor(host: UmbControllerHostInterface, entityKey: string | null) {
		this.host = host;
		this.#entityKey = entityKey;

		this.#data = new ObjectState(DefaultLanguageData);
		this.data = this.#data.asObservable();

		new UmbContextConsumerController(host, UMB_LANGUAGE_STORE_CONTEXT_TOKEN, (_instance: UmbLanguageStore) => {
			this.#store = _instance;
			this.#observeStore();
		});
	}

	#observeStore(): void {
		if (!this.#store || this.#entityKey === null) {
			return;
		}

		this._storeObserver?.destroy();
		this._storeObserver = new UmbObserverController(this.host, this.#store.getByIsoCode(this.#entityKey), (content) => {
			if (!content) return; // TODO: Handle nicely if there is no content data.
			this.update(content);
		});
	}

	public getData() {
		return this.#data.getValue();
	}

	public getAvailableCultures() {
		//TODO: Don't use !, however this will be changed with the introduction of repositories.
		return this.#store!.getAvailableCultures();
	}

	public update(data: Partial<UmbLanguageStoreItemType>) {
		this.#data.next({ ...this.getData(), ...data });
	}

	public save(): Promise<void> {
		if (!this.#store) {
			// TODO: more beautiful error:
			console.error('Could not save cause workspace context has no store.');
			return Promise.resolve();
		}
		return this.#store.save(this.getData());
	}
}
