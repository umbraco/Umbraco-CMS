import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbCurrentUserRepository } from '../current-user.repository.js';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import {UmbChangeCurrentUserPasswordServerDataSource} from './change-current-user-password.server.data-source.js'
export class UmbChangeCurrentUserPasswordRepository extends UmbCurrentUserRepository{
    #changePasswordSource: UmbChangeCurrentUserPasswordServerDataSource;
    protected notificationContext?: UmbNotificationContext;

    constructor(host: UmbControllerHost){
        super(host);
        this.#changePasswordSource = new UmbChangeCurrentUserPasswordServerDataSource(host);

        this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
            this.notificationContext = instance;
        }).asPromise();
    }

    async changePassword(userId: string, newPassword: string, oldPassword: string, isCurrentUser: boolean) {
		if (!userId) throw new Error('User id is missing');
		if (!newPassword) throw new Error('New password is missing');
		if (isCurrentUser && !oldPassword) throw new Error('Old password is missing');

		const { data, error } = await this.#changePasswordSource.changePassword(userId, newPassword, oldPassword, isCurrentUser);

		if (!error) {
			const notification = { data: { message: `Password changed` } };
			this.notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}
}

export default UmbChangeCurrentUserPasswordRepository;
