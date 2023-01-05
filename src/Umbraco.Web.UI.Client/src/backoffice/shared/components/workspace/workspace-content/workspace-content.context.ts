import { v4 as uuidv4 } from 'uuid';
import { UmbNotificationService } from '../../../../../core/notification';
import { UmbNotificationDefaultData } from '../../../../../core/notification/layouts/default';
import { UmbWorkspaceContext } from '../workspace-context/workspace.context';
import { UmbNodeStoreBase } from '@umbraco-cms/stores/store';
import { ContentTreeItem } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from 'src/core/controller/controller-host.mixin';
import { UmbContextConsumerController } from 'src/core/context-api/consume/context-consumer.controller';
import { UmbObserverController } from '@umbraco-cms/observable-api';

// TODO: Consider if its right to have this many class-inheritance of WorkspaceContext
export class UmbWorkspaceContentContext<
	ContentTypeType extends ContentTreeItem = ContentTreeItem,
	StoreType extends UmbNodeStoreBase<ContentTypeType> = UmbNodeStoreBase<ContentTypeType>
> extends UmbWorkspaceContext<ContentTypeType> {

	protected _notificationService?: UmbNotificationService;

	protected _store: StoreType|null = null;
	protected _storeSubscription?: UmbObserverController<ContentTypeType | null>;

	public entityKey?: string;
	public entityType: string;

	constructor(
		host: UmbControllerHostInterface,
		defaultData: ContentTypeType,
		storeAlias: string,
		entityType: string
	) {
		super(host, defaultData);

		new UmbContextConsumerController(
			host,
			'umbNotificationService',
			(_instance: UmbNotificationService) => {
				this._notificationService = _instance;
			}
		);

		this.entityType = entityType;

		new UmbContextConsumerController(host, storeAlias, (_instance: StoreType) => {
			this._store = _instance;
			if (!this._store) {
				// TODO: make sure to break the application in a good way.
				return;
			}
			this._readyToLoad();
		});
	}

	load(entityKey: string) {
		this.entityKey = entityKey;
		this._readyToLoad();
	}

	create(parentKey: string | null) {
		this.entityKey = uuidv4();
		console.log("I'm new, and I will be created under ", parentKey)
	}

	protected _readyToLoad(): void {
		if(!this._store || !this.entityKey) {
			return;
		}
		this._storeSubscription?.destroy();
		this._storeSubscription = new UmbObserverController(this._host, this._store.getByKey(this.entityKey), 
		(content) => {
			if (!content) return; // TODO: Handle nicely if there is no content data.
			this.update(content as any);
		});
	}

	public getStore(): StoreType | null {
		return this._store;
	}

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
}