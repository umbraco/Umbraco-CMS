import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context-token.js';
import type { UmbUserInputElement } from '../../../../user/components/user-input/user-input.element.js';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UserGroupService, UserService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';

import '../../../../user/components/user-input/user-input.element.js';

@customElement('umb-user-group-workspace-users')
export class UmbUserGroupWorkspaceUsersElement extends UmbLitElement {
	@state()
	private _userUniques: string[] = [];

	@state()
	private _usersRemainingCount = 0;

	#unique?: string;
	#isNew = false;
	#persistedUserUniques: string[] = [];
	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;
	#workspaceContext?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (context) => {
			this.#notificationContext = context;
		});

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#observe();
		});
	}

	#observe() {
		this.observe(
			this.#workspaceContext?.unique,
			(value) => {
				this.#unique = value ?? undefined;
				if (value && !this.#isNew) {
					this.#loadUsers(value);
				}
			},
			'_observeUnique',
		);

		this.observe(
			this.#workspaceContext?.isNew,
			(isNew) => {
				const wasNew = this.#isNew;
				this.#isNew = isNew ?? false;

				// When the group transitions from newly created to persisted, save any pending user changes.
				if (wasNew === true && isNew === false) {
					this.#savePendingUserChanges();
				}
			},
			'_observeIsNew',
		);
	}

	async #loadUsers(unique: string) {
		const { data } = await tryExecute(
			this,
			UserService.getFilterUser({ query: { userGroupIds: [unique], take: 100 } }),
		);
		const uniques = data?.items.map((u) => u.id) ?? [];
		this.#persistedUserUniques = [...uniques];
		this._userUniques = [...uniques];
		const total = data?.total ?? 0;
		this._usersRemainingCount = Math.max(0, total - uniques.length);
	}

	async #savePendingUserChanges() {
		const unique = this.#unique;
		if (!unique || this._userUniques.length === 0) return;
		await this.#persistUserChanges(unique, this._userUniques);
	}

	async #persistUserChanges(unique: string, newSelection: string[]): Promise<boolean> {
		const toAdd = newSelection.filter((u) => !this.#persistedUserUniques.includes(u));
		const toRemove = this.#persistedUserUniques.filter((u) => !newSelection.includes(u));

		const [addError, removeError] = await Promise.all([
			this.#addUsersToGroup(unique, toAdd),
			this.#removeUsersFromGroup(unique, toRemove),
		]);

		if (addError) {
			this.#notificationContext?.peek('danger', {
				data: {
					headline: this.localize.term('speechBubbles_operationFailedHeader'),
					message: this.localize.term('user_addUsersToGroupError'),
				},
			});
		}

		if (removeError) {
			this.#notificationContext?.peek('danger', {
				data: {
					headline: this.localize.term('speechBubbles_operationFailedHeader'),
					message: this.localize.term('user_removeUsersFromGroupError'),
				},
			});
		}

		if (!addError && !removeError) {
			this.#persistedUserUniques = [...newSelection];
			return true;
		}

		return false;
	}

	async #addUsersToGroup(unique: string, userIds: string[]): Promise<boolean> {
		if (!userIds.length) return false;
		const { error } = await tryExecute(
			this,
			UserGroupService.postUserGroupByIdUsers({
				path: { id: unique },
				body: userIds.map((id) => ({ id })),
			}),
		);
		return !!error;
	}

	async #removeUsersFromGroup(unique: string, userIds: string[]): Promise<boolean> {
		if (!userIds.length) return false;
		const { error } = await tryExecute(
			this,
			UserGroupService.deleteUserGroupByIdUsers({
				path: { id: unique },
				body: userIds.map((id) => ({ id })),
			}),
		);
		return !!error;
	}

	async #onUsersChange(event: UmbChangeEvent) {
		event.stopPropagation();
		const target = event.target as UmbUserInputElement;
		const newSelection = target.selection;

		// For new (unsaved) groups, track locally — users will be persisted when the group is saved.
		if (this.#isNew) {
			this._userUniques = newSelection;
			return;
		}

		const unique = this.#unique;
		if (!unique) return;

		const previousSelection = [...this._userUniques];
		this._userUniques = newSelection; // optimistic update

		const success = await this.#persistUserChanges(unique, newSelection);
		if (!success) {
			this._userUniques = previousSelection; // revert on error
		}
	}

	override render() {
		return html`
			<uui-box>
				<div slot="headline"><umb-localize key="general_users"></umb-localize></div>
				<umb-user-input
					.selection=${this._userUniques}
					.remainingCount=${this._usersRemainingCount}
					@change=${this.#onUsersChange}></umb-user-input>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
			}
		`,
	];
}

export { UmbUserGroupWorkspaceUsersElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-workspace-users': UmbUserGroupWorkspaceUsersElement;
	}
}
