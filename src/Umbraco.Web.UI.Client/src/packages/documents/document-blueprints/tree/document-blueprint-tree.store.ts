import { UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_CONTEXT } from '../repository/detail/index.js';
import type { UmbDocumentBlueprintDetailModel } from '../types.js';
import type { UmbDocumentBlueprintTreeItemModel } from './types.js';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';
import { UmbStoreConnector } from '@umbraco-cms/backoffice/store';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbDocumentBlueprintTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Document Blueprints
 */
export class UmbDocumentBlueprintTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbDocumentBlueprintTreeStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentBlueprintTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_BLUEPRINT_TREE_STORE_CONTEXT.toString());

		new UmbStoreConnector<UmbDocumentBlueprintTreeItemModel, UmbDocumentBlueprintDetailModel>(host, {
			store: this,
			connectToStoreAlias: UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_CONTEXT,
			updateStoreItemMapper: (item) => this.#updateTreeItemMapper(item),
		});
	}

	#updateTreeItemMapper = (item: UmbDocumentBlueprintDetailModel) => {
		return { ...item, name: item.variants.map((variant) => variant.name)[0] };
	};
}

export const UMB_DOCUMENT_BLUEPRINT_TREE_STORE_CONTEXT = new UmbContextToken<UmbDocumentBlueprintTreeStore>(
	'UmbDocumentBlueprintTreeStore',
);
