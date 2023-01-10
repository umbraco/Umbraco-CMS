import { v4 as uuidv4 } from 'uuid';
import { BehaviorSubject, Observable } from 'rxjs';
import { UmbNotificationService } from '../../../../../core/notification';
import { UmbNotificationDefaultData } from '../../../../../core/notification/layouts/default';
import { UmbNodeStoreBase } from '@umbraco-cms/stores/store';
import { UmbControllerHostInterface } from 'src/core/controller/controller-host.mixin';
import { UmbContextConsumerController } from 'src/core/context-api/consume/context-consumer.controller';
import { UmbObserverController } from '@umbraco-cms/observable-api';
import { UmbContextProviderController } from 'src/core/context-api/provide/context-provider.controller';
import { EntityTreeItem } from '@umbraco-cms/backend-api';
import { ContentDetails } from '@umbraco-cms/models';

// TODO: Consider if its right to have this many class-inheritance of WorkspaceContext
// TODO: Could we extract this code into a 'Manager' of its own, which will be instantiated by the concrete Workspace Context. This will be more transparent and 'reuseable'
export abstract class UmbWorkspaceContentContext<
	ContentTypeType extends ContentDetails = ContentDetails,
	StoreType extends UmbNodeStoreBase<ContentTypeType> = UmbNodeStoreBase<ContentTypeType>
> {

	protected _host: UmbControllerHostInterface;

	// TODO: figure out how fine grained we want to make our observables.
	// TODO: add interface
	protected _data!:BehaviorSubject<ContentTypeType>;
	public readonly data: Observable<ContentTypeType>;

	protected _notificationService?: UmbNotificationService;

	protected _store: StoreType|null = null;
	protected _storeSubscription?: UmbObserverController<ContentTypeType | null>;

	#isNew = true;

	public entityKey?: string;
	public entityType: string;

	constructor(
		host: UmbControllerHostInterface,
		defaultData: ContentTypeType,
		storeAlias: string,
		entityType: string
	) {

		this._host = host;

		//TODO: Use the UniqueBehaviorSubject, and separate observables for each part?

		this._data = new BehaviorSubject<ContentTypeType>(defaultData);
		this.data = this._data.asObservable();
		this.entityType = entityType;

		new UmbContextConsumerController(
			host,
			'umbNotificationService',
			(_instance: UmbNotificationService) => {
				this._notificationService = _instance;
			}
		);

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
		console.log("I'm new, and I will be created under ", parentKey)
	}

	protected _observeStore(): void {
		if(!this._store || !this.entityKey) {
			return;
		}

		if(!this.#isNew) {
			this._storeSubscription?.destroy();
			this._storeSubscription = new UmbObserverController(this._host, this._store.getByKey(this.entityKey),
			(content) => {
				if (!content) return; // TODO: Handle nicely if there is no content data.
				this.update(content as any);
			});
		}
	}

	public getStore() {
		return this._store;
	}

	abstract setPropertyValue(alias: string, value: unknown):void;


	public save(): Promise<void> {
		if(!this._store) {
			// TODO: more beautiful error:
			console.error("Could not save cause workspace context has no store.");
			return Promise.resolve();
		}
		return this._store.save([this.getData()])
			.then(() => {
				const data: UmbNotificationDefaultData = { message: 'Document Saved' };
				this._notificationService?.peek('positive', { data });
			})
			.catch(() => {
				const data: UmbNotificationDefaultData = { message: 'Failed to save Document' };
				this._notificationService?.peek('danger', { data });
			});
	}



	// TODO: how can we make sure to call this.
	public destroy(): void {
		this._data.unsubscribe();
	}
}
