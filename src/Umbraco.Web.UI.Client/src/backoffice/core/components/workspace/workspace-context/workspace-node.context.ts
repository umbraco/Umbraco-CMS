import { UmbNotificationService } from '../../../services/notification';
import { UmbNotificationDefaultData } from '../../../services/notification/layouts/default';
import { UmbWorkspaceWithStoreContext } from './workspace-with-store.context';
import { UmbNodeStoreBase } from 'src/backoffice/core/stores/store';
import { ContentTreeItem } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from 'src/core/controller/controller-host.mixin';
import { UmbContextConsumerController } from 'src/core/context-api/consume/context-consumer.controller';

// TODO: Consider if its right to have this many class-inheritance of WorkspaceContext
export class UmbWorkspaceNodeContext<
	ContentTypeType extends ContentTreeItem = ContentTreeItem,
	StoreType extends UmbNodeStoreBase<ContentTypeType> = UmbNodeStoreBase<ContentTypeType>
> extends UmbWorkspaceWithStoreContext<ContentTypeType, StoreType> {

	protected _notificationService?: UmbNotificationService;

	public entityKey: string;
	public entityType: string;

	constructor(
		host: UmbControllerHostInterface,
		defaultData: ContentTypeType,
		storeAlias: string,
		entityKey: string,
		entityType: string
	) {
		super(host, defaultData, storeAlias);

		new UmbContextConsumerController(
			host,
			'umbNotificationService',
			(_instance: UmbNotificationService) => {
				this._notificationService = _instance;
			}
		);

		this.entityKey = entityKey;
		this.entityType = entityType;
	}

	protected _onStoreSubscription(): void {
		this._store.getByKey(this.entityKey).subscribe((content) => {
			if (!content) return; // TODO: Handle nicely if there is no content data.
			this.update(content as any);
		});
	}

	public save(): Promise<void> {
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
}

