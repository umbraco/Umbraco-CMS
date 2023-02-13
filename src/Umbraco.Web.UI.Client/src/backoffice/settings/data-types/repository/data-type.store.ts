import type { DataTypeModel } from '@umbraco-cms/backend-api';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

/**
 * @export
 * @class UmbDataTypeStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Template Details
 */
export class UmbDataTypeStore extends UmbStoreBase {
	#data = new ArrayState<DataTypeModel>([], (x) => x.key);

	/**
	 * Creates an instance of UmbDataTypeStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbDataTypeStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UmbDataTypeStore.name);
	}

	/**
	 * Append a data-type to the store
	 * @param {DataTypeModel} dataType
	 * @memberof UmbDataTypeStore
	 */
	append(dataType: DataTypeModel) {
		this.#data.append([dataType]);
	}

	/**
	 * Append a data-type to the store
	 * @param {key} DataTypeModel key.
	 * @memberof UmbDataTypeStore
	 */
	byKey(key: DataTypeModel['key']) {
		return this.#data.getObservablePart((x) => x.find((y) => y.key === key));
	}

	/**
	 * Removes data-types in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbDataTypeStore
	 */
	remove(uniques: Array<DataTypeModel['key']>) {
		this.#data.remove(uniques);
	}
}

export const UMB_DATA_TYPE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDataTypeStore>(UmbDataTypeStore.name);
