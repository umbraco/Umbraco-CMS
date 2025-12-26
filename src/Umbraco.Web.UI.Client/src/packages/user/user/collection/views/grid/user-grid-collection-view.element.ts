import { UMB_USER_COLLECTION_CONTEXT } from '../../user-collection.context-token.js';
import type { UmbUserCollectionContext } from '../../user-collection.context.js';
import type { UmbUserDetailModel } from '../../../types.js';
import { UMB_EDIT_USER_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-user-grid-collection-view')
export class UmbUserGridCollectionViewElement extends UmbLitElement {
	@state()
	private _users: Array<UmbUserDetailModel> = [];

	@state()
	private _selection: Array<string | null> = [];

	@state()
	private _loading = false;

	#collectionContext?: UmbUserCollectionContext;

	constructor() {
		super();

		this.consumeContext(UMB_USER_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;

			this.observe(
				this.#collectionContext?.selection.selection,
				(selection) => (this._selection = selection ?? []),
				'umbCollectionSelectionObserver',
			);

			this.observe(
				this.#collectionContext?.items,
				(items) => (this._users = items ?? []),
				'umbCollectionItemsObserver',
			);
		});
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
		return html`
			<umb-entity-collection-item-card
				.item=${user}
				href=${UMB_EDIT_USER_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: user.unique })}
				selectable
				?select-only=${this._selection.length > 0}
				?selected=${this.#collectionContext?.selection.isSelected(user.unique)}
				@selected=${() => this.#onSelect(user)}
				@deselected=${() => this.#onDeselect(user)}></umb-entity-collection-item-card>
		`;
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
				grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
				gap: var(--uui-size-space-4);
			}
		`,
	];
}

export { UmbUserGridCollectionViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-grid-collection-view': UmbUserGridCollectionViewElement;
	}
}
