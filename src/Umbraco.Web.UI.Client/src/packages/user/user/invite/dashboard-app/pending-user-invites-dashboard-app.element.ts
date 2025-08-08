import { UmbUserStateFilter } from '../../collection/utils/index.js';
import { UmbUserCollectionRepository } from '../../collection/index.js';
import type { UmbUserDetailModel } from '../../types.js';
import { UMB_USER_WORKSPACE_PATH } from '../../paths.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	ManifestDashboardApp,
	UmbDashboardAppElement,
	UmbDashboardAppSize,
} from '@umbraco-cms/backoffice/dashboard';

@customElement('umb-pending-user-invites-dashboard-app')
export class UmbPendingUserInvitesDashboardAppElement extends UmbLitElement implements UmbDashboardAppElement {
	@property({ type: Object })
	manifest?: ManifestDashboardApp;

	@property({ type: String })
	size?: UmbDashboardAppSize;

	#userCollectionRepository = new UmbUserCollectionRepository(this);

	@state()
	private _pendingUserInvites: UmbUserDetailModel[] = [];

	protected override firstUpdated(): void {
		this.#loadInvitedUsers();
	}

	async #loadInvitedUsers() {
		const { data } = await this.#userCollectionRepository.requestCollection({
			take: 5,
			userStates: [UmbUserStateFilter.INVITED],
		});
		this._pendingUserInvites = data?.items ?? [];
	}

	override render() {
		return html`
			<umb-dashboard-app-layout>
				${this._pendingUserInvites.map(
					(user) => html`

							<uui-ref-node-user name=${user.name} href=${UMB_USER_WORKSPACE_PATH + '/edit/' + user.unique}>
								<umb-user-avatar
									style="font-size: 0.5em"
									slot="icon"
									.name=${user.name}
									.kind=${user.kind}
									.imgUrls=${user.avatarUrls}>
								</umb-user-avatar>
							</uui-ref-node-user>

					`,
				)}
			</umb-dashboard-app-layout>
		`;
	}

	static override styles = [UmbTextStyles, css``];
}

export { UmbPendingUserInvitesDashboardAppElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-pending-user-invites-dashboard-app': UmbPendingUserInvitesDashboardAppElement;
	}
}
