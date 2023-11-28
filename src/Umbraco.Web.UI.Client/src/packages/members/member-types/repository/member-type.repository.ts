import type { MemberTypeDetails } from '../types.js';
import { UmbMemberTypeTreeStore, UMB_MEMBER_TYPE_TREE_STORE_CONTEXT } from '../tree/index.js';
import { UmbMemberTypeStore, UMB_MEMBER_TYPE_STORE_CONTEXT } from './member-type.store.js';
import { UmbMemberTypeDetailServerDataSource } from './sources/member-type.detail.server.data.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';

export class UmbMemberTypeRepository extends UmbBaseController {
	#init!: Promise<unknown>;

	#treeStore?: UmbMemberTypeTreeStore;

	#detailSource: UmbMemberTypeDetailServerDataSource;
	#store?: UmbMemberTypeStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		// TODO: figure out how spin up get the correct data source
		this.#detailSource = new UmbMemberTypeDetailServerDataSource(this);

		this.#init = Promise.all([
			this.consumeContext(UMB_MEMBER_TYPE_STORE_CONTEXT, (instance) => {
				this.#store = instance;
			}),

			this.consumeContext(UMB_MEMBER_TYPE_TREE_STORE_CONTEXT, (instance) => {
				this.#treeStore = instance;
			}),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}),
		]);
	}

	// DETAILS

	async createScaffold() {
		await this.#init;
		return this.#detailSource.createScaffold();
	}

	async requestById(id: string) {
		await this.#init;

		// TODO: should we show a notification if the id is missing?
		// Investigate what is best for Acceptance testing, cause in that perspective a thrown error might be the best choice?
		if (!id) {
			throw new Error('Id is missing');
		}
		const { data, error } = await this.#detailSource.requestById(id);

		if (data) {
			this.#store?.append(data);
		}
		return { data, error };
	}
	async byId(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.#init;
		return this.#store!.byId(id);
	}

	async delete(id: string) {
		await this.#init;

		if (!id) {
			throw new Error('Id is missing');
		}

		const { error } = await this.#detailSource.delete(id);

		if (!error) {
			const notification = { data: { message: `Member type deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server.
		// Consider notify a workspace if a member type is deleted from the store while someone is editing it.
		this.#store?.removeItem(id);
		this.#treeStore?.removeItem(id);

		return { error };
	}

	async save(id: string, updatedMemberType: any) {
		if (!id) throw new Error('Key is missing');
		if (!updatedMemberType) throw new Error('Member Type is missing');

		await this.#init;

		const { error } = await this.#detailSource.update(id, updatedMemberType);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server
			// Consider notify a workspace if a member type is updated in the store while someone is editing it.
			//this.#store?.append(detail);
			this.#treeStore?.updateItem(id, updatedMemberType);

			const notification = { data: { message: `Member type '${updatedMemberType.name}' saved` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	async create(detail: MemberTypeDetails) {
		await this.#init;

		if (!detail.name) {
			throw new Error('Name is missing');
		}

		const { data, error } = await this.#detailSource.create(detail);

		if (!error) {
			const notification = { data: { message: `Member type '${detail.name}' created` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}
}
