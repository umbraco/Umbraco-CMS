import type { DataTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export const UMB_DATA_TYPE_STORE_CONTEXT = new UmbContextToken<UmbDataTypeDetailStore>('UmbDataTypeStore');

/**
 * @export
 * @class UmbDataTypeStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Template Details
 */
export class UmbDataTypeDetailStore extends UmbStoreBase<DataTypeResponseModel> {
	/**
	 * Creates an instance of UmbDataTypeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDataTypeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DATA_TYPE_STORE_CONTEXT.toString(), new UmbArrayState<DataTypeResponseModel>([], (x) => x.id));
	}

	/**
	 * Retrieve a data-type from the store
	 * @param {id} string id.
	 * @memberof UmbDataTypeStore
	 */
	byId(id: DataTypeResponseModel['id']) {
		return this._data.asObservablePart((x) => x.find((y) => y.id === id));
	}

	withPropertyEditorUiAlias(propertyEditorUiAlias: string) {
		// TODO: Use a model for the data-type tree items: ^^Most likely it should be parsed to the UmbEntityTreeStore as a generic type.
		return this._data.asObservablePart((items) =>
			items.filter((item) => (item as any).propertyEditorUiAlias === propertyEditorUiAlias),
		);
	}
}
