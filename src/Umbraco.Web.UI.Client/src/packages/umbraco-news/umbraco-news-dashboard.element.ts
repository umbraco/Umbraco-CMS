import { UMB_AUTH_CONTEXT } from '../core/auth/auth.context.token.js';
import { tryExecuteAndNotify } from '../core/resources/tryExecuteAndNotify.function.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UserGroupResource, type UserGroupResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('umb-umbraco-news-dashboard')
export class UmbUmbracoNewsDashboardElement extends UmbLitElement {
	@state()
	_groups: UserGroupResponseModel[] = [];

	async clearToken() {
		const authContext = await this.getContext(UMB_AUTH_CONTEXT);
		authContext.clearTokenStorage();
	}

	async makeAuthorizedRequest() {
		const { data } = await tryExecuteAndNotify(this, UserGroupResource.getUserGroup({ skip: 0, take: 10 }));

		if (data) {
			this._groups = data.items;
		}
	}

	render() {
		return html`
			<uui-box class="uui-text">
				<p>
					<uui-button look="primary" @click=${this.clearToken}>Clear all tokens</uui-button>
					<uui-button look="primary" @click=${this.makeAuthorizedRequest}>Make authorized request</uui-button>

					${this._groups.map((group) => html` <p>${group.name}</p> `)}
				</p>
			</uui-box>
		`;
	}
}

export default UmbUmbracoNewsDashboardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-umbraco-news-dashboard': UmbUmbracoNewsDashboardElement;
	}
}
