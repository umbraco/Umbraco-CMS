import type { RelationTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export const UMB_RELATION_TYPE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbRelationTypeStore>('UmbRelationTypeStore');

/**
 * @export
 * @class UmbRelationTypeStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Template Details
 */
export class UmbRelationTypeStore extends UmbStoreBase {
	/**
	 * Creates an instance of UmbRelationTypeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbRelationTypeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_RELATION_TYPE_STORE_CONTEXT_TOKEN.toString(),
			new UmbArrayState<RelationTypeResponseModel>([], (x) => x.id)
		);
	}

	/**
	 * Append a relation-type to the store
	 * @param {RelationTypeResponseModel} RelationType
	 * @memberof UmbRelationTypeStore
	 */
	append(RelationType: RelationTypeResponseModel) {
		this._data.append([RelationType]);
	}

	/**
	 * Append a relation-type to the store
	 * @param {id} RelationTypeResponseModel id.
	 * @memberof UmbRelationTypeStore
	 */
	byId(id: RelationTypeResponseModel['id']) {
		return this._data.getObservablePart((x) => x.find((y) => y.id === id));
	}

	/**
	 * Removes relation-types in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbRelationTypeStore
	 */
	remove(uniques: Array<RelationTypeResponseModel['id']>) {
		this._data.remove(uniques);
	}
}
