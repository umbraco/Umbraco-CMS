import { EntityTreeItem, MemberTypeResource, } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { createObservablePart, UniqueArrayBehaviorSubject } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/stores/store-base';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';


export const UMB_MEMBER_TYPE_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberTypeTreeStore>('UmbMemberTypeTreeStore');


/**
 * @export
 * @class UmbMemberTypeTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Member Types
 */
export class UmbMemberTypeTreeStore extends UmbStoreBase {


	// TODO: use the right type here:
	#data = new UniqueArrayBehaviorSubject<EntityTreeItem>([], (x) => x.key);


	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_MEMBER_TYPE_TREE_STORE_CONTEXT_TOKEN.toString());
	}

	// TODO: How can we avoid having this in both stores?
	/**
	 * @description - Delete a Data Type.
	 * @param {string[]} keys
	 * @memberof UmbMemberTypesStore
	 * @return {*}  {Promise<void>}
	 */
	async delete(keys: string[]) {
		// TODO: use backend cli when available.
		await fetch('/umbraco/backoffice/member-type/delete', {
			method: 'POST',
			body: JSON.stringify(keys),
			headers: {
				'Content-Type': 'application/json',
			},
		});

		this.#data.remove(keys);
	}

	getTreeRoot() {
		tryExecuteAndNotify(this._host, MemberTypeResource.getTreeMemberTypeRoot({})).then(({ data }) => {
			if (data) {
				// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
				this.#data.append(data.items);
			}
		});

		// TODO: remove ignore when we know how to handle trashed items.
		return createObservablePart(this.#data, (items) => items.filter((item) => item.parentKey === null));
	}

	getTreeItemChildren(key: string) {
		/*
		tryExecuteAndNotify(
			this._host,
			MemberTypeResource.getTreeMemberTypeChildren({
				parentKey: key,
			})
		).then(({ data }) => {
			if (data) {
				// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
				this.#data.append(data.items);
			}
		});
		*/

		return createObservablePart(this.#data, (items) => items.filter((item) => item.parentKey === key));
	}

	getTreeItems(keys: Array<string>) {
		if (keys?.length > 0) {
			tryExecuteAndNotify(
				this._host,
				MemberTypeResource.getTreeMemberTypeItem({
					key: keys,
				})
			).then(({ data }) => {
				if (data) {
					// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
					this.#data.append(data);
				}
			});
		}

		return createObservablePart(this.#data, (items) => items.filter((item) => keys.includes(item.key ?? '')));
	}
}
