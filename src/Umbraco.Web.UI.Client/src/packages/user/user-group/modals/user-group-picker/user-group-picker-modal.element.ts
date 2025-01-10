import { UmbUserGroupCollectionRepository } from '../../collection/repository/index.js';
import type { UmbUserGroupDetailModel } from '../../types.js';
import { html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import type { UMB_USER_GROUP_PICKER_MODAL } from '@umbraco-cms/backoffice/user-group';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UUIMenuItemEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbSelectedEvent, UmbDeselectedEvent } from '@umbraco-cms/backoffice/event';
import { UmbUserGroupRefElement } from '../../components/user-group-ref';

@customElement('umb-user-group-picker-modal')
export class UmbUserGroupPickerModalElement extends UmbModalBaseElement<
	(typeof UMB_USER_GROUP_PICKER_MODAL)['DATA'],
	(typeof UMB_USER_GROUP_PICKER_MODAL)['VALUE']
> {
	@state()
	private _userGroups: Array<UmbUserGroupDetailModel> = [];

	#selectionManager = new UmbSelectionManager(this);
	#userGroupCollectionRepository = new UmbUserGroupCollectionRepository(this);

	override connectedCallback(): void {
		super.connectedCallback();

		// TODO: in theory this config could change during the lifetime of the modal, so we could observe it
		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.value?.selection ?? []);
		this.observe(this.#selectionManager.selection, (selection) => this.updateValue({ selection }), 'selectionObserver');
	}

	protected override firstUpdated(): void {
		this.#observeUserGroups();
	}

	async #observeUserGroups() {
		const { error, asObservable } = await this.#userGroupCollectionRepository.requestCollection();
		if (error) return;
		this.observe(asObservable(), (items) => (this._userGroups = items), 'umbUserGroupsObserver');
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

	#onSubmit() {
		this.updateValue({ selection: this.#selectionManager.getSelection() });
		this._submitModal();
	}

	//TODO: This looks good, but uses in-line html, so it should be defined in the umb-user-group-ref it self.

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('user_selectUserGroup', false)}>
				<uui-box>
					${this._userGroups.map((userGroup) => {
						const isSelected = this.#selectionManager.isSelected(userGroup.unique);
						return html`
							<umb-user-group-ref
								.name=${userGroup.name}
								selectable
								@selected=${(event: UUIMenuItemEvent) => this.#onSelected(event, userGroup)}
								@deselected=${(event: UUIMenuItemEvent) => this.#onDeselected(event, userGroup)}
								?selected=${isSelected}
								.icon=${userGroup.icon || ''}
								.userPermissionAliases=${userGroup.sections}>
								<uui-icon .name=${userGroup.icon || undefined} slot="icon"></uui-icon>
								<div slot="detail">
									<div>
										<strong>Sections:</strong> ${userGroup.sections.length
											? userGroup.sections.map((section) => section.split('.').pop()).join(', ')
											: 'No sections allowed'}
									</div>
									<div>
										<strong>Media Start Node:</strong> ${userGroup.mediaStartNode
											? userGroup.mediaStartNode.unique
											: 'No media startnode selected'}
									</div>
									<div>
										<strong>Content Start Node:</strong> ${userGroup.documentStartNode
											? userGroup.documentStartNode.unique
											: 'No content startnode selected'}
									</div>
								</div>
							</umb-user-group-ref>
						`;
					})}
				</uui-box>
				<div slot="actions">
					<uui-button label="Cancel" @click=${this._rejectModal}></uui-button>
					<uui-button label="Confirm" look="primary" color="positive" @click=${this.#onSubmit}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}
}

export default UmbUserGroupPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-picker-modal': UmbUserGroupPickerModalElement;
	}
}
