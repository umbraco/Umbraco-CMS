import { v4 as uuidv4 } from 'uuid';
import { UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, UmbNotificationDefaultData } from '@umbraco-cms/notification';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController, UmbContextProviderController } from '@umbraco-cms/context-api';
import { DeepState, UmbObserverController, createObservablePart } from '@umbraco-cms/observable-api';
import { UmbContentStore } from '@umbraco-cms/store';
import type { ContentTreeItem } from '@umbraco-cms/backend-api';

// TODO: Consider if its right to have this many class-inheritance of WorkspaceContext
// TODO: Could we extract this code into a 'Manager' of its own, which will be instantiated by the concrete Workspace Context. This will be more transparent and 'reuseable'
export abstract class UmbWorkspaceContentContext<
	ContentTypeType extends ContentTreeItem = ContentTreeItem,
	StoreType extends UmbContentStore<ContentTypeType> = UmbContentStore<ContentTypeType>
> {
	protected _host: UmbControllerHostInterface;

	// TODO: figure out how fine grained we want to make our observables.
	// TODO: add interface
	protected _data;
	public readonly data;
	public readonly name;

	protected _notificationService?: UmbNotificationService;

	protected _store: StoreType | null = null;
	protected _storeSubscription?: UmbObserverController<ContentTypeType | null>;

	#isNew = true;

	public entityKey?: string;
	public entityType: string;

	constructor(host: UmbControllerHostInterface, defaultData: ContentTypeType, storeAlias: string, entityType: string) {
		this._host = host;

		this._data = new DeepState<ContentTypeType>(defaultData);
		this.data = this._data.asObservable();
		this.name = createObservablePart(this._data, (data) => data.name);

		this.entityType = entityType;

		new UmbContextConsumerController(host, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, (_instance) => {
			this._notificationService = _instance;
		});

		new UmbContextConsumerController(host, storeAlias, (_instance: StoreType) => {
			this._store = _instance;
			if (!this._store) {
				// TODO: make sure to break the application in a good way.
				return;
			}
			this._observeStore();

			// TODO: first provide when we have umbNotificationService as well.
			new UmbContextProviderController(this._host, 'umbWorkspaceContext', this);
		});
	}

	public getData() {
		return this._data.getValue();
	}
	public update(data: Partial<ContentTypeType>) {
		this._data.next({ ...this.getData(), ...data });
	}

	load(entityKey: string) {
		this.#isNew = false;
		this.entityKey = entityKey;
		this._observeStore();
	}

	create(parentKey: string | null) {
		this.#isNew = true;
		this.entityKey = uuidv4();
		console.log("I'm new, and I will be created under ", parentKey);
	}

	protected _observeStore(): void {
		if (!this._store || !this.entityKey) {
			return;
		}

		if (!this.#isNew) {
			this._storeSubscription?.destroy();
			this._storeSubscription = new UmbObserverController(
				this._host,
				this._store.getByKey(this.entityKey),
				(content) => {
					if (!content) return; // TODO: Handle nicely if there is no content data.
					this.update(content as any);
				}
			);
		}
	}

	public getStore() {
		return this._store;
	}

	abstract setPropertyValue(alias: string, value: unknown): void;

	// TODO: consider turning this into an abstract so each context implement this them selfs.
	public save(): Promise<void> {
		if (!this._store) {
			// TODO: add a more beautiful error:
			console.error('Could not save cause workspace context has no store.');
			return Promise.resolve();
		}
		return this._store
			.save([this.getData()])
			.then(() => {
				const data: UmbNotificationDefaultData = { message: 'Document Saved' };
				this._notificationService?.peek('positive', { data });
			})
			.catch(() => {
				const data: UmbNotificationDefaultData = { message: 'Failed to save Document' };
				this._notificationService?.peek('danger', { data });
			});
	}

	// TODO: how can we make sure to call this, we might need to turn this thing into a ContextProvider(extending) for it to call destroy?
	public destroy(): void {
		this._data.unsubscribe();
	}
}
