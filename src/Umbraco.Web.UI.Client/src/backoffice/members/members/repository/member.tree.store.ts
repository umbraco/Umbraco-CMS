import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbEntityTreeStore } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export const UMB_MEMBER_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberTreeStore>('UmbMemberTreeStore');

/**
 * @export
 * @class UmbMemberTreeStore
 * @extends {UmbEntityTreeStore}
 * @description - Tree Data Store for Members
 */
export class UmbMemberTreeStore extends UmbEntityTreeStore {
	/**
	 * Creates an instance of UmbTemplateTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMemberGroupTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}
