import { getDisplayStateFromUserStatus } from '../../../../utils.js';
import type { UmbUserCollectionContext } from '../../user-collection.context.js';
import type { UmbUserDetailModel } from '../../../types.js';
import { css, html, nothing, customElement, state, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_DEFAULT_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbUserGroupDetailModel } from '@umbraco-cms/backoffice/user-group';
import { UmbUserGroupCollectionRepository } from '@umbraco-cms/backoffice/user-group';

@customElement('umb-user-grid-collection-view')
export class UmbUserGridCollectionViewElement extends UmbLitElement {
	@state()
	private _users: Array<UmbUserDetailModel> = [];

	@state()
	private _selection: Array<string | null> = [];

	@state()
	private _loading = false;

	#userGroups: Array<UmbUserGroupDetailModel> = [];
	#collectionContext?: UmbUserCollectionContext;
	#userGroupCollectionRepository = new UmbUserGroupCollectionRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance as UmbUserCollectionContext;
			this.observe(
				this.#collectionContext.selection.selection,
				(selection) => (this._selection = selection),
				'umbCollectionSelectionObserver',
			);
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

	#onSelect(user: UmbUserDetailModel) {
		this.#collectionContext?.selection.select(user.unique ?? '');
	}

	#onDeselect(user: UmbUserDetailModel) {
		this.#collectionContext?.selection.deselect(user.unique ?? '');
	}

	render() {
		if (this._loading) nothing;
		return html`
			<div id="user-grid">
				${repeat(
					this._users,
					(user) => user.unique,
					(user) => this.#renderUserCard(user),
				)}
			</div>
		`;
	}

	#renderUserCard(user: UmbUserDetailModel) {
		return html`
			<uui-card-user
				.name=${user.name ?? 'Unnamed user'}
				selectable
				?select-only=${this._selection.length > 0}
				?selected=${this.#collectionContext?.selection.isSelected(user.unique)}
				@open=${() => this._handleOpenCard(user.unique)}
				@selected=${() => this.#onSelect(user)}
				@deselected=${() => this.#onDeselect(user)}>
				${this.#renderUserTag(user)} ${this.#renderUserGroupNames(user)} ${this.#renderUserLoginDate(user)}
			</uui-card-user>
		`;
	}

	#renderUserTag(user: UmbUserDetailModel) {
		if (user.state && user.state === UserStateModel.ACTIVE) {
			return nothing;
		}

		const statusLook = user.state ? getDisplayStateFromUserStatus(user.state) : undefined;
		return html`<uui-tag
			slot="tag"
			size="s"
			look="${ifDefined(statusLook?.look)}"
			color="${ifDefined(statusLook?.color)}">
			<umb-localize key=${'user_' + statusLook?.key}></umb-localize>
		</uui-tag>`;
	}

	#renderUserGroupNames(user: UmbUserDetailModel) {
		const userGroupNames = this.#userGroups
			.filter((userGroup) => user.userGroupUniques?.includes(userGroup.unique))
			.map((userGroup) => userGroup.name)
			.join(', ');

		return html`<div>${userGroupNames}</div>`;
	}

	#renderUserLoginDate(user: UmbUserDetailModel) {
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
