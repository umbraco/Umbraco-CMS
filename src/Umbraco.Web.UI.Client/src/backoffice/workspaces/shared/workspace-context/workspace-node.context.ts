
import { UmbWorkspaceWithStoreContext } from "./workspace-with-store.context";
import { UmbNodeStoreBase } from "@umbraco-cms/stores/store";
import { UmbNotificationDefaultData, UmbNotificationService } from "@umbraco-cms/services";
import { ContentTreeItem } from "@umbraco-cms/backend-api";
import { UmbContextConsumer } from "@umbraco-cms/context-api";

// TODO: Consider if its right to have this many class-inheritance of WorkspaceContext
export class UmbWorkspaceNodeContext<ContentTypeType extends ContentTreeItem = ContentTreeItem, StoreType extends UmbNodeStoreBase<ContentTypeType> = UmbNodeStoreBase<ContentTypeType>> extends UmbWorkspaceWithStoreContext<ContentTypeType, StoreType> {

	protected _notificationService?: UmbNotificationService;
	protected _notificationConsumer!:UmbContextConsumer;

	public entityKey:string;

	constructor(target:HTMLElement, defaultData:ContentTypeType, storeAlias:string, entityKey: string) {
		super(target, defaultData, storeAlias);

		this._notificationConsumer = new UmbContextConsumer(this._target, 'umbNotificationService', (_instance: UmbNotificationService) => {
			this._notificationService = _instance;
		});

		this.entityKey = entityKey;
	}


	connectedCallback() {
		super.connectedCallback();
		this._notificationConsumer.attach();
	}

	disconnectedCallback() {
		super.connectedCallback();
		this._notificationConsumer.detach();
	}


	protected _onStoreSubscription(): void {
		this._dataObserver = this._store.getByKey(this.entityKey).subscribe((content) => {
			if (!content) return; // TODO: Handle nicely if there is no content data.
			this.update(content as any);
		});
	}



	public save(): Promise<void> {
		return this._store.save([this.getData()]).then(() => {
			const data: UmbNotificationDefaultData = { message: 'Document Saved' };
			this._notificationService?.peek('positive', { data });
		}).catch(() => {
			const data: UmbNotificationDefaultData = { message: 'Failed to save Document' };
			this._notificationService?.peek('danger', { data });
		});
	}

}

