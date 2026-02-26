import { UmbMemberGroupCollectionRepository } from '../../collection/index.js';
import type { UmbMemberGroupDetailModel } from '../../types.js';
import type {
	UmbMemberGroupPickerModalValue,
	UmbMemberGroupPickerModalData,
} from './member-group-picker-modal.token.js';
import { css, customElement, html, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbPaginationManager, UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';

const PAGE_SIZE = 50;

@customElement('umb-member-group-picker-modal')
export class UmbMemberGroupPickerModalElement extends UmbModalBaseElement<
	UmbMemberGroupPickerModalData,
	UmbMemberGroupPickerModalValue
> {
	@state()
	private _memberGroups: Array<UmbMemberGroupDetailModel> = [];

	@state()
	private _currentPage: number = 1;

	@state()
	private _totalPages: number = 1;

	#collectionRepository = new UmbMemberGroupCollectionRepository(this);
	#selectionManager = new UmbSelectionManager(this);
	#pagination = new UmbPaginationManager();

	override connectedCallback(): void {
		super.connectedCallback();
		this.#pagination.setPageSize(PAGE_SIZE);
		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.value?.selection ?? []);
	}

	override firstUpdated() {
		this.#loadMemberGroups();
	}

	async #loadMemberGroups() {
		const skip = this.#pagination.getSkip();
		const { data } = await this.#collectionRepository.requestCollection({ skip, take: PAGE_SIZE });
		this._memberGroups = data?.items ?? [];
		this.#pagination.setTotalItems(data?.total ?? 0);
		this._currentPage = this.#pagination.getCurrentPageNumber();
		this._totalPages = this.#pagination.getTotalPages();
	}

	#onPageChange(event: UUIPaginationEvent) {
		event.stopPropagation();
		this.#pagination.setCurrentPageNumber(event.target.current);
		this.#loadMemberGroups();
	}

	get #filteredMemberGroups() {
		if (this.data?.filter) {
			return this._memberGroups.filter(this.data.filter as any);
		} else {
			return this._memberGroups;
		}
	}

	#submit() {
		this.value = { selection: this.#selectionManager.getSelection() };
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('defaultdialogs_chooseMemberGroup')}>
				<uui-box>
					${repeat(
						this.#filteredMemberGroups,
						(item) => item.unique,
						(item) => html`
							<uui-menu-item
								label=${item.name ?? ''}
								selectable
								@selected=${() => this.#selectionManager.select(item.unique)}
								@deselected=${() => this.#selectionManager.deselect(item.unique)}
								?selected=${this.#selectionManager.isSelected(item.unique)}>
								<uui-icon slot="icon" name="icon-users"></uui-icon>
							</uui-menu-item>
						`,
					)}
				</uui-box>
				${when(
					this._totalPages > 1,
					() =>
						html` <uui-pagination
							.current=${this._currentPage}
							.total=${this._totalPages}
							firstlabel=${this.localize.term('general_first')}
							previouslabel=${this.localize.term('general_previous')}
							nextlabel=${this.localize.term('general_next')}
							lastlabel=${this.localize.term('general_last')}
							@change=${this.#onPageChange}></uui-pagination>`,
				)}
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
					<uui-button
						label=${this.localize.term('general_choose')}
						look="primary"
						color="positive"
						@click=${this.#submit}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static override styles = [
		css`
			uui-pagination {
				display: block;
				margin-top: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbMemberGroupPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-group-picker-modal': UmbMemberGroupPickerModalElement;
	}
}
