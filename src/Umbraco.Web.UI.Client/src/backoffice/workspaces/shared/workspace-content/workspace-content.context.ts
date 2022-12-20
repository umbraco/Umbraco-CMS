
import { UmbWorkspaceWithStoreContext } from "../workspace-context/workspace-with-store.context";
import type { DocumentDetails } from "@umbraco-cms/models";
import { UmbContentStoreBase } from "@umbraco-cms/stores/store";
import { UmbNotificationDefaultData } from "@umbraco-cms/services";

export class UmbWorkspaceContentContext<ContentTypeType extends DocumentDetails, StoreType extends UmbContentStoreBase<ContentTypeType>> extends UmbWorkspaceWithStoreContext<ContentTypeType, StoreType> {

	constructor(target:HTMLElement, defaultData:ContentTypeType, storeAlias:string, entityType: string, entityKey: string) {
		super(target, defaultData, storeAlias, entityType, entityKey);
	}


	protected _observeStore(): void {
		this._dataObserver = this._store.getByKey(this.entityKey).subscribe((content) => {
			if (!content) return; // TODO: Handle nicely if there is no content data.
			this.update(content as any);
		});
	}



	public save() {
		this._store.save([this.getData()]).then(() => {
			const data: UmbNotificationDefaultData = { message: 'Document Saved' };
			this._notificationService?.peek('positive', { data });
		}).catch(() => {
			const data: UmbNotificationDefaultData = { message: 'Failed to save Document' };
			this._notificationService?.peek('danger', { data });
		});
	}
	
	// TODO: trash?

}

