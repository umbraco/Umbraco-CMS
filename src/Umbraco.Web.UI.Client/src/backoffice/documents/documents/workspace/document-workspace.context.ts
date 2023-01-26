import { v4 as uuidv4 } from 'uuid';
import { UmbDocumentDetailStore, UMB_DOCUMENT_DETAIL_STORE_CONTEXT_TOKEN } from '../document.detail.store';
import type { UmbWorkspaceEntityContextInterface } from '../../../shared/components/workspace/workspace-context/workspace-entity-context.interface';
import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import type { DocumentDetails } from '@umbraco-cms/models';
import { appendToFrozenArray, createObservablePart, ObjectState, UmbObserverController } from '@umbraco-cms/observable-api';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbNotificationDefaultData, UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/notification';

/*
const DefaultDocumentData = {
	key: '',
	name: '',
	icon: '',
	type: '',
	hasChildren: false,
	parentKey: '',
	isTrashed: false,
	properties: [
		{
			alias: '',
			label: '',
			description: '',
			dataTypeKey: '',
		},
	],
	data: [
		{
			alias: '',
			value: '',
		},
	],
	variants: [
		{
			name: '',
		},
	],
} as DocumentDetails;
*/

export class UmbDocumentWorkspaceContext extends UmbWorkspaceContext implements UmbWorkspaceEntityContextInterface<DocumentDetails | undefined> {


	#data = new ObjectState<DocumentDetails | undefined>(undefined);
	public readonly data = this.#data.asObservable();
	public readonly name = this.#data.getObservablePart((data) => data?.name);

	#isNew = false;
	private _entityKey?: string;

	protected _storeSubscription?: UmbObserverController;


	private _notificationService?: UmbNotificationService;
	private _store?: UmbDocumentDetailStore;


	constructor(host: UmbControllerHostInterface) {
		super(host);

		new UmbContextConsumerController(this._host, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, (_instance) => {
			this._notificationService = _instance;
		});
		new UmbContextConsumerController(this._host, UMB_DOCUMENT_DETAIL_STORE_CONTEXT_TOKEN, (_instance) => {
			this._store = _instance;
			this._observeStore();
		});


	}

	private _observeStore() {
		if (!this._store || !this._entityKey) {
			return;
		}

		if (!this.#isNew) {
			this._storeSubscription?.destroy();
			this._storeSubscription = new UmbObserverController(
				this._host,
				this._store.getByKey(this._entityKey),
				(content) => {
					if (!content) return; // TODO: Handle nicely if there is no content data.
					this.#data.next(content as any);
				}
			);
		}
	}

	public getStore() {
		return this._store;
	}

	load(entityKey: string) {
		this.#isNew = false;
		this._entityKey = entityKey;
		this._observeStore();
	}

	create(parentKey: string | null) {
		this.#isNew = true;
		this._entityKey = uuidv4();
		console.log("I'm new, and I will be created under ", parentKey);
	}

	getData() {
		return this.#data.getValue();
	}

	save(): Promise<void> {

		if (!this._store) {
			// TODO: add a more beautiful error:
			console.error('Could not save cause workspace context has no store.');
			return Promise.resolve();
		}

		const documentData = this.getData();
		if(!documentData) {
			console.error('Could not save cause workspace context has no data.');
			return Promise.resolve();
		}

		return this._store
			.save([documentData])
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
		this.#data.unsubscribe();
	}




	public setPropertyValue(alias: string, value: unknown) {

		// TODO: make sure to check that we have a details model? otherwise fail? 8This can be relevant if we use the same context for tree actions?
		const entry = {alias: alias, value: value};

		const newDataSet = appendToFrozenArray(this._data.getValue().data, entry, x => x.alias);

		this.#data.update({data: newDataSet});
	}

	/*
	concept notes:

	public saveAndPublish() {

	}

	public saveAndPreview() {

	}
	*/

}
