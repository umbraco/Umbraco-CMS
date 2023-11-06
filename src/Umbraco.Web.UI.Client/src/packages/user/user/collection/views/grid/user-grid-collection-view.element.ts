import { getDisplayStateFromUserStatus } from '../../../../utils.js';
import { UmbUserCollectionContext } from '../../user-collection.context.js';
import { type UmbUserDetail } from '../../../types.js';
import { css, html, nothing, customElement, state, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UserGroupResponseModel, UserStateModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbUserGroupCollectionRepository } from '@umbraco-cms/backoffice/user-group';

@customElement('umb-user-grid-collection-view')
export class UmbUserGridCollectionViewElement extends UmbLitElement {
	@state()
	private _users: Array<UmbUserDetail> = [];

	@state()
	private _selection: Array<string | null> = [];

	@state()
	private _loading = false;

	#userGroups: Array<UserGroupResponseModel> = [];
	#collectionContext?: UmbUserCollectionContext;
	#userGroupCollectionRepository = new UmbUserGroupCollectionRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance as UmbUserCollectionContext;
			this.observe(this.#collectionContext.selection, (selection) => (this._selection = selection), 'umbCollectionSelectionObserver');
			this.observe(this.#collectionContext.items, (items) => (this._users = items), 'umbCollectionItemsObserver');
		});

		this.#requestUserGroups();
	}

	async #requestUserGroups() {
		this._loading = true;

		const { data } = await this.#userGroupCollectionRepository.requestCollection();
		this.#userGroups = data?.items ?? [];
		this._loading = false;
	}

	//TODO How should we handle url stuff?
	private _handleOpenCard(id: string) {
		//TODO this will not be needed when cards works as links with href
		history.pushState(null, '', 'section/user-management/view/users/user/' + id); //TODO Change to a tag with href and make dynamic
	}

	#onSelect(user: UmbUserDetail) {
		this.#collectionContext?.select(user.id ?? '');
	}

	#onDeselect(user: UmbUserDetail) {
		this.#collectionContext?.deselect(user.id ?? '');
	}

	render() {
		if (this._loading) nothing;
		return html`
			<div id="user-grid">
				${repeat(
					this._users,
					(user) => user.id,
					(user) => this.#renderUserCard(user),
				)}
			</div>
		`;
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
				${this.#renderUserTag(user)} ${this.#renderUserGroupNames(user)} ${this.#renderUserLoginDate(user)}
			</uui-card-user>
		`;
	}

	#renderUserTag(user: UmbUserDetail) {
		if (user.state && user.state === UserStateModel.ACTIVE) {
			return nothing;
		}

		const statusLook = getDisplayStateFromUserStatus(user.state);
		return html`<uui-tag
			slot="tag"
			size="s"
			look="${ifDefined(statusLook?.look)}"
			color="${ifDefined(statusLook?.color)}">
			<umb-localize key=${'user_' + statusLook.key}></umb-localize>
		</uui-tag>`;
	}

	#renderUserGroupNames(user: UmbUserDetail) {
		const userGroupNames = this.#userGroups
			.filter((userGroup) => user.userGroupIds?.includes(userGroup.id!))
			.map((userGroup) => userGroup.name)
			.join(', ');

		return html`<div>${userGroupNames}</div>`;
	}

	#renderUserLoginDate(user: UmbUserDetail) {
		if (!user.lastLoginDate) {
			return html`<div class="user-login-time">${`${user.name} ${this.localize.term('user_noLogin')}`}</div>`;
		}

		return html`<div class="user-login-time">
			<umb-localize key="user_lastLogin"></umb-localize><br />
			${this.localize.date(user.lastLoginDate)}
		</div>`;
	}

	static styles = [
		UmbTextStyles,
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

export default UmbUserGridCollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-grid-collection-view': UmbUserGridCollectionViewElement;
	}
}
