import { getLookAndColorFromUserStatus } from '../../../../utils.js';
import { UmbUserCollectionContext } from '../../user-collection.context.js';
import { type UmbUserDetail } from '../../../types.js';
import { css, html, nothing, customElement, state, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UMB_COLLECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/collection';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UserStateModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-user-collection-grid-view')
export class UmbUserCollectionGridViewElement extends UmbLitElement {
	@state()
	private _users: Array<UmbUserDetail> = [];

	@state()
	private _selection: Array<string> = [];

	#collectionContext?: UmbUserCollectionContext;

	constructor() {
		super();

		//TODO: Get user group names

		this.consumeContext(UMB_COLLECTION_CONTEXT_TOKEN, (instance) => {
			this.#collectionContext = instance as UmbUserCollectionContext;
			this.observe(this.#collectionContext.selection, (selection) => (this._selection = selection));
			this.observe(this.#collectionContext.items, (items) => (this._users = items));
		});
	}

	//TODO How should we handle url stuff?
	private _handleOpenCard(id: string) {
		//TODO this will not be needed when cards works as links with href
		history.pushState(null, '', 'section/users/view/users/user/' + id); //TODO Change to a tag with href and make dynamic
	}

	#onSelect(user: UmbUserDetail) {
		this.#collectionContext?.select(user.id ?? '');
	}

	#onDeselect(user: UmbUserDetail) {
		this.#collectionContext?.deselect(user.id ?? '');
	}

	#renderUserCard(user: UmbUserDetail) {
		return html`
			<uui-card-user
				.name=${user.name ?? 'Unnamed user'}
				selectable
				?select-only=${this._selection.length > 0}
				?selected=${this.#collectionContext?.isSelected(user.id ?? '')}
				@open=${() => this._handleOpenCard(user.id ?? '')}
				@selected=${() => this.#onSelect(user)}
				@deselected=${() => this.#onDeselect(user)}>
				${this.#renderUserTag(user)} ${this.#renderUserLoginDate(user)}
			</uui-card-user>
		`;
	}

	#renderUserTag(user: UmbUserDetail) {
		if (user.state && user.state === UserStateModel.ACTIVE) {
			return nothing;
		}

		const statusLook = getLookAndColorFromUserStatus(user.state);
		return html`<uui-tag
			slot="tag"
			size="s"
			look="${ifDefined(statusLook?.look)}"
			color="${ifDefined(statusLook?.color)}">
			${user.state}
		</uui-tag>`;
	}

	#renderUserLoginDate(user: UmbUserDetail) {
		if (!user.lastLoginDate) {
			return html`<div class="user-login-time">${`${user.name} has not logged in yet`}</div>`;
		}

		return html`<div class="user-login-time">
			<div>Last login</div>
			${user.lastLoginDate}
		</div>`;
	}

	render() {
		return html`
			<div id="user-grid">
				${repeat(
					this._users,
					(user) => user.id,
					(user) => this.#renderUserCard(user)
				)}
			</div>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}

			#user-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
				gap: var(--uui-size-space-4);
			}

			uui-card-user {
				width: 100%;
				height: 180px;
			}

			.user-login-time {
				margin-top: auto;
			}
		`,
	];
}

export default UmbUserCollectionGridViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-collection-grid-view': UmbUserCollectionGridViewElement;
	}
}
