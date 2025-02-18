import { UmbUserGroupCollectionRepository } from '../../collection/repository/index.js';
import type { UmbUserGroupDetailModel } from '../../types.js';
import { css, customElement, html, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { debounce, UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UMB_USER_GROUP_PICKER_MODAL } from '@umbraco-cms/backoffice/user-group';
import type { UUIInputEvent, UUIMenuItemEvent } from '@umbraco-cms/backoffice/external/uui';

import '../../components/user-group-ref/user-group-ref.element.js';

@customElement('umb-user-group-picker-modal')
export class UmbUserGroupPickerModalElement extends UmbModalBaseElement<
	typeof UMB_USER_GROUP_PICKER_MODAL.DATA,
	typeof UMB_USER_GROUP_PICKER_MODAL.VALUE
> {
	@state()
	private _filteredItems: Array<UmbUserGroupDetailModel> = [];

	@state()
	private _userGroups: Array<UmbUserGroupDetailModel> = [];

	#debouncedFilter = debounce((filter: string) => {
		this._filteredItems = filter
			? this._userGroups.filter(
					(userGroup) =>
						userGroup.alias.toLowerCase().includes(filter) || userGroup.name.toLowerCase().includes(filter),
				)
			: this._userGroups;
	}, 500);

	#selectionManager = new UmbSelectionManager(this);

	#userGroupCollectionRepository = new UmbUserGroupCollectionRepository(this);

	constructor() {
		super();

		this.#observeUserGroups();
	}

	override connectedCallback() {
		super.connectedCallback();

		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.value?.selection ?? []);

		this.observe(this.#selectionManager.selection, (selection) => this.updateValue({ selection }), 'selectionObserver');
	}

	async #observeUserGroups() {
		const { error, asObservable } = await this.#userGroupCollectionRepository.requestCollection();
		if (error) return;

		this.observe(asObservable(), (items) => (this._userGroups = this._filteredItems = items), 'umbUserGroupsObserver');
	}

	#onSelected(event: UUIMenuItemEvent, item: UmbUserGroupDetailModel) {
		if (!item.unique) throw new Error('User group unique is required');
		event.stopPropagation();

		this.#selectionManager.select(item.unique);

		this.requestUpdate();
		this.modalContext?.dispatchEvent(new UmbSelectedEvent(item.unique));
	}

	#onDeselected(event: UUIMenuItemEvent, item: UmbUserGroupDetailModel) {
		if (!item.unique) throw new Error('User group unique is required');
		event.stopPropagation();

		this.#selectionManager.deselect(item.unique);

		this.requestUpdate();
		this.modalContext?.dispatchEvent(new UmbDeselectedEvent(item.unique));
	}

	#onFilterInput(event: UUIInputEvent) {
		const query = (event.target.value as string) || '';
		this.#debouncedFilter(query.toLowerCase());
	}

	#onSubmit() {
		this.updateValue({ selection: this.#selectionManager.getSelection() });
		this._submitModal();
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('user_chooseUserGroup', true)}>
				<uui-box>
					<uui-input
						type="search"
						id="filter"
						label=${this.localize.term('placeholders_filter')}
						placeholder=${this.localize.term('placeholders_filter')}
						@input=${this.#onFilterInput}
						${umbFocus()}>
						<uui-icon name="search" slot="prepend" id="filter-icon"></uui-icon>
					</uui-input>
					${repeat(
						this._filteredItems,
						(userGroup) => userGroup.alias,
						(userGroup) => html`
							<umb-user-group-ref
								.name=${userGroup.name}
								select-only
								selectable
								?selected=${this.#selectionManager.isSelected(userGroup.unique)}
								?documentRootAccess=${userGroup.documentRootAccess}
								.documentStartNode=${!userGroup.documentRootAccess ? userGroup.documentStartNode?.unique : null}
								?mediaRootAccess=${userGroup.mediaRootAccess}
								.mediaStartNode=${!userGroup.mediaRootAccess ? userGroup.mediaStartNode?.unique : null}
								.sections=${userGroup.sections}
								@selected=${(event: UUIMenuItemEvent) => this.#onSelected(event, userGroup)}
								@deselected=${(event: UUIMenuItemEvent) => this.#onDeselected(event, userGroup)}>
								${when(userGroup.icon, () => html`<umb-icon name=${userGroup.icon!} slot="icon"></umb-icon>`)}
							</umb-user-group-ref>
						`,
					)}
				</uui-box>
				<div slot="actions">
					<uui-button label=${this.localize.term('general_cancel')} @click=${this._rejectModal}></uui-button>
					<uui-button
						label=${this.localize.term('buttons_choose')}
						look="primary"
						color="positive"
						@click=${this.#onSubmit}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static override styles = [
		css`
			#filter {
				width: 100%;
				margin-bottom: var(--uui-size-space-4);
			}

			#filter-icon {
				display: flex;
				color: var(--uui-color-border);
				height: 100%;
				padding-left: var(--uui-size-space-2);
			}
		`,
	];
}

export default UmbUserGroupPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-picker-modal': UmbUserGroupPickerModalElement;
	}
}
