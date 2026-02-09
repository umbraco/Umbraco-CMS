import { getDisplayStateFromUserStatus, TimeFormatOptions } from '../../utils.js';
import { UmbUserKind } from '../../utils/user-kind.js';
import type { UmbUserCollectionItemModel } from './types.js';
import {
	css,
	customElement,
	html,
	ifDefined,
	nothing,
	property,
	state,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbUserGroupItemRepository, type UmbUserGroupItemModel } from '@umbraco-cms/backoffice/user-group';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbEntityCollectionItemElement } from '@umbraco-cms/backoffice/collection';

@customElement('umb-user-collection-item-card')
export class UmbUserCollectionItemCardElement extends UmbLitElement implements UmbEntityCollectionItemElement {
	#item?: UmbUserCollectionItemModel | undefined;

	@property({ type: Object })
	public get item(): UmbUserCollectionItemModel | undefined {
		return this.#item;
	}
	public set item(value: UmbUserCollectionItemModel | undefined) {
		this.#item = value;
		this.#loadUserGroups();
	}

	@property({ type: Boolean })
	selectable = false;

	@property({ type: Boolean })
	selected = false;

	@property({ type: Boolean })
	selectOnly = false;

	@property({ type: Boolean })
	disabled = false;

	@property({ type: String })
	href?: string;

	@state()
	private _userGroupItems: Array<UmbUserGroupItemModel> = [];

	#userGroupItemRepository = new UmbUserGroupItemRepository(this);

	async #loadUserGroups() {
		if (!this.item || !this.item.userGroupUniques || this.item.userGroupUniques.length === 0) {
			this._userGroupItems = [];
			return;
		}

		const { data } = await this.#userGroupItemRepository.requestItems(
			this.item.userGroupUniques.map((ref) => ref.unique),
		);

		this._userGroupItems = data ?? [];
	}

	#onSelected(event: CustomEvent) {
		if (!this.item) return;
		event.stopPropagation();
		this.dispatchEvent(new UmbSelectedEvent(this.item.unique));
	}

	#onDeselected(event: CustomEvent) {
		if (!this.item) return;
		event.stopPropagation();
		this.dispatchEvent(new UmbDeselectedEvent(this.item.unique));
	}

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-card-user
				.name=${this.item.name ?? this.localize.term('general_unnamed')}
				href=${ifDefined(this.href)}
				?selectable=${this.selectable}
				?select-only=${this.selectOnly}
				?selected=${this.selected}
				?disabled=${this.disabled}
				@selected=${this.#onSelected}
				@deselected=${this.#onDeselected}>
				${this.#renderUserTag()} ${this.#renderUserGroupNames()} ${this.#renderUserLoginDate()}
				<umb-user-avatar
					slot="avatar"
					.name=${this.item.name}
					.kind=${this.item.kind}
					.imgUrls=${this.item.avatarUrls}></umb-user-avatar>
			</uui-card-user>
		`;
	}

	#renderUserTag() {
		if (this.item?.state && this.item?.state === UserStateModel.ACTIVE) {
			return nothing;
		}

		const statusLook = this.item?.state ? getDisplayStateFromUserStatus(this.item.state) : undefined;
		return html`
			<uui-tag slot="tag" look=${ifDefined(statusLook?.look)} color=${ifDefined(statusLook?.color)}>
				<umb-localize key=${'user_' + statusLook?.key}></umb-localize>
			</uui-tag>
		`;
	}

	#renderUserGroupNames() {
		const userGroupNames = this._userGroupItems
			.filter((userGroup) =>
				this.item?.userGroupUniques?.map((reference) => reference.unique).includes(userGroup.unique),
			)
			.map((userGroup) => userGroup.name)
			.join(', ');

		return html`<div>${userGroupNames}</div>`;
	}

	#renderUserLoginDate() {
		if (this.item?.kind === UmbUserKind.API) return nothing;
		return html`
			<div class="user-login-time">
				${when(
					this.item?.lastLoginDate,
					(lastLoginDate) => html`
						<umb-localize key="user_lastLogin">Last login</umb-localize>
						<span>${this.localize.date(lastLoginDate, TimeFormatOptions)}</span>
					`,
					() => html`<umb-localize key="user_noLogin">has not logged in yet</umb-localize>`,
				)}
			</div>
		`;
	}

	static override styles = [
		css`
			uui-card-user {
				width: 100%;
				min-width: auto;
				height: 100%;
				justify-content: normal;
				padding-top: var(--uui-size-space-5);
				flex-direction: column;

				umb-user-avatar {
					font-size: 1.6rem;
				}
			}

			.user-login-time {
				margin-top: var(--uui-size-1);
			}
		`,
	];
}

export { UmbUserCollectionItemCardElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-collection-item-card': UmbUserCollectionItemCardElement;
	}
}
