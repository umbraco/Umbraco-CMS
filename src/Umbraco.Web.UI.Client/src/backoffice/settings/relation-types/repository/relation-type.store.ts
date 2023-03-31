import type { RelationTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { ArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export const UMB_RELATION_TYPE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbRelationTypeStore>('UmbRelationTypeStore');

/**
 * @export
 * @class UmbRelationTypeStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Template Details
 */
export class UmbRelationTypeStore extends UmbStoreBase {
	#data = new ArrayState<RelationTypeResponseModel>([], (x) => x.key);

	/**
	 * Creates an instance of UmbRelationTypeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbRelationTypeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_RELATION_TYPE_STORE_CONTEXT_TOKEN.toString());
	}

	/**
	 * Append a relation-type to the store
	 * @param {RelationTypeResponseModel} RelationType
	 * @memberof UmbRelationTypeStore
	 */
	append(RelationType: RelationTypeResponseModel) {
		this.#data.append([RelationType]);
	}

	/**
	 * Append a relation-type to the store
	 * @param {key} RelationTypeResponseModel key.
	 * @memberof UmbRelationTypeStore
	 */
	byKey(key: RelationTypeResponseModel['key']) {
		return this.#data.getObservablePart((x) => x.find((y) => y.key === key));
	}

	/**
	 * Removes relation-types in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbRelationTypeStore
	 */
	remove(uniques: Array<RelationTypeResponseModel['key']>) {
		this.#data.remove(uniques);
	}
}
