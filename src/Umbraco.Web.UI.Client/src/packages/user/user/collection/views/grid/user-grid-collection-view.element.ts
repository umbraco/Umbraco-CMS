import { getDisplayStateFromUserStatus } from '../../../utils.js';
import type { UmbUserCollectionContext } from '../../user-collection.context.js';
import type { UmbUserDetailModel } from '../../../types.js';
import { UMB_USER_COLLECTION_CONTEXT } from '../../user-collection.context-token.js';
import { UMB_USER_WORKSPACE_PATH } from '../../../paths.js';
import { UmbUserKind } from '../../../utils/index.js';
import { css, html, nothing, customElement, state, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
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

	// TODO: we need to use the item repository here
	#userGroupCollectionRepository = new UmbUserGroupCollectionRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_USER_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
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

	#onSelect(user: UmbUserDetailModel) {
		this.#collectionContext?.selection.select(user.unique ?? '');
	}

	#onDeselect(user: UmbUserDetailModel) {
		this.#collectionContext?.selection.deselect(user.unique ?? '');
	}

	override render() {
		if (this._loading) return nothing;
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
		const href = UMB_USER_WORKSPACE_PATH + '/edit/' + user.unique;

		return html`
			<uui-card-user
				.name=${user.name ?? 'Unnamed user'}
				href=${href}
				selectable
				?select-only=${this._selection.length > 0}
				?selected=${this.#collectionContext?.selection.isSelected(user.unique)}
				@selected=${() => this.#onSelect(user)}
				@deselected=${() => this.#onDeselect(user)}>
				${this.#renderUserTag(user)} ${this.#renderUserGroupNames(user)} ${this.#renderUserLoginDate(user)}
				<umb-user-avatar
					slot="avatar"
					.name=${user.name}
					.kind=${user.kind}
					.imgUrls=${user.avatarUrls}
					style="font-size: 1.6rem;"></umb-user-avatar>
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
			.filter((userGroup) => user.userGroupUniques?.map((reference) => reference.unique).includes(userGroup.unique))
			.map((userGroup) => userGroup.name)
			.join(', ');

		return html`<div>${userGroupNames}</div>`;
	}

	#renderUserLoginDate(user: UmbUserDetailModel) {
		if (user.kind === UmbUserKind.API) return nothing;

		if (!user.lastLoginDate) {
			return html`<div class="user-login-time">${`${user.name} ${this.localize.term('user_noLogin')}`}</div>`;
		}
		const lastLoggedinLocalTime: Date = new Date(user.lastLoginDate);
		const formattedTime = lastLoggedinLocalTime.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

		return html`<div class="user-login-time">
			<umb-localize key="user_lastLogin"></umb-localize>
			${this.localize.date(user.lastLoginDate)} ${formattedTime}
		</div>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}

			#user-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
				gap: var(--uui-size-space-4);
			}

			uui-card-user {
				width: 100%;
				justify-content: normal;
				padding-top: var(--uui-size-space-5);
				flex-direction: column;
			}

			.user-login-time {
				margin-top: auto;
				width: 100%;
    			word-wrap: break-word;
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
