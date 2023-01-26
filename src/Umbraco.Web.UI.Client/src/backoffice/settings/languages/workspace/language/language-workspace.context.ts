import { UmbLanguageStore, UmbLanguageStoreItemType } from '../../language.store';
import type { LanguageDetails } from '@umbraco-cms/models';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UniqueBehaviorSubject } from 'src/core/observable-api/unique-behavior-subject';
import { UmbContextConsumerController } from 'src/core/context-api/consume/context-consumer.controller';
import { UmbObserverController } from '@umbraco-cms/observable-api';

const DefaultLanguageData: UmbLanguageStoreItemType = {
	id: 0,
	key: '',
	name: '',
	isoCode: '',
	isDefault: false,
	isMandatory: false,
};

export class UmbWorkspaceLanguageContext {
	public host: UmbControllerHostInterface;

	private _entityKey!: string;

	private _data;
	public readonly data;

	private _store: UmbLanguageStore | null = null;
	protected _storeObserver?: UmbObserverController<LanguageDetails>;

	constructor(host: UmbControllerHostInterface, entityKey: string) {
		this.host = host;
		this._entityKey = entityKey;

		this._data = new UniqueBehaviorSubject<LanguageDetails>(DefaultLanguageData);
		this.data = this._data.asObservable();

		new UmbContextConsumerController(host, 'umbLanguageStore', (_instance: UmbLanguageStore) => {
			this._store = _instance;
			if (!this._store) {
				// TODO: make sure to break the application in a good way.
				return;
			}

			this._observeStore();
		});
	}

	private _observeStore(): void {
		if (!this._store) {
			return;
		}
		this._storeObserver?.destroy();
		this._storeObserver = new UmbObserverController(this.host, this._store.getByKey(this._entityKey), (content) => {
			if (!content) return; // TODO: Handle nicely if there is no content data.
			this.update(content);
		});
	}

	public getData() {
		return this._data.getValue();
	}
	public update(data: Partial<LanguageDetails>) {
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
