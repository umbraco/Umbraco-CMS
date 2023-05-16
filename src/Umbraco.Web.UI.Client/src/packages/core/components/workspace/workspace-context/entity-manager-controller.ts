import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbContextConsumerController, UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbNotificationDefaultData,
	UmbNotificationContext,
	UMB_NOTIFICATION_CONTEXT_TOKEN,
} from '@umbraco-cms/backoffice/notification';
import { UmbObjectState, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import type { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbEntityDetailStore } from '@umbraco-cms/backoffice/store';

// Extend entityType base type?, so we are sure to have parentId?
// TODO: switch to use EntityDetailItem ? if we can have such type?
export class UmbEntityWorkspaceManager<
	StoreType extends UmbEntityDetailStore<EntityDetailsType>,
	EntityDetailsType extends EntityTreeItemResponseModel = ReturnType<StoreType['getScaffold']>
> {
	private _host;

	state = new UmbObjectState<EntityDetailsType | undefined>(undefined);

	protected _storeSubscription?: UmbObserverController<EntityDetailsType | undefined>;

	private _notificationContext?: UmbNotificationContext;
	private _store?: StoreType;

	#isNew = false;
	private _entityType;
	private _entityId!: string;

	private _createAtParentKey?: string | null;

	constructor(host: UmbControllerHostElement, entityType: string, storeToken: UmbContextToken<StoreType>) {
		this._host = host;
		this._entityType = entityType;

		new UmbContextConsumerController(this._host, UMB_NOTIFICATION_CONTEXT_TOKEN, (_instance) => {
			this._notificationContext = _instance;
		});

		// Create controller holding Token?
		new UmbContextConsumerController<StoreType>(this._host, storeToken, (_instance) => {
			this._store = _instance;
			this._observeStore();
		});
	}

	private _observeStore() {
		if (!this._store || !this._entityId) {
			return;
		}

		if (this.#isNew) {
			const newData = this._store.getScaffold(this._entityType, this._createAtParentKey || null);
			this.state.next(newData);
		} else {
			this._storeSubscription?.destroy();
			this._storeSubscription = new UmbObserverController(
				this._host,
				this._store.getByKey(this._entityId),
				(content) => {
					if (!content) return; // TODO: Handle nicely if there is no content data.
					this.state.next(content as any);
				}
			);
		}
	}

	getEntityType = () => {
		return this._entityType;
	};
	getEntityKey = (): string => {
		return this._entityId;
	};

	getStore = () => {
		return this._store;
	};

	getData = () => {
		return this.state.getValue();
	};

	load = (entityId: string) => {
		this.#isNew = false;
		this._entityId = entityId;
		this._observeStore();
	};

	create = (parentId: string | null) => {
		this.#isNew = true;
		this._entityId = UmbId.new();
		this._createAtParentKey = parentId;
	};

	save = (): Promise<void> => {
		if (!this._store) {
			// TODO: add a more beautiful error:
			console.error('Could not save cause workspace context has no store.');
			return Promise.resolve();
		}

		const documentData = this.getData();
		if (!documentData) {
			console.error('Could not save cause workspace context has no data.');
			return Promise.resolve();
		}

		return this._store
			.save([documentData])
			.then(() => {
				const data: UmbNotificationDefaultData = { message: 'Document Saved' };
				this._notificationContext?.peek('positive', { data });
			})
			.catch(() => {
				const data: UmbNotificationDefaultData = { message: 'Failed to save Document' };
				this._notificationContext?.peek('danger', { data });
			});
	};

	public destroy = (): void => {
		this.state.unsubscribe();
	};
}
