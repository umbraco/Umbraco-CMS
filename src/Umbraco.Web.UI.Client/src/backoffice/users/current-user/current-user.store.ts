import type { UserDetails } from '@umbraco-cms/backoffice/models';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

export const UMB_CURRENT_USER_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbCurrentUserStore>('UmbCurrentUserStore');

export class UmbCurrentUserStore {
	#currentUser = new UmbObjectState<UserDetails | undefined>(undefined);
	public readonly currentUser = this.#currentUser.asObservable();
}
