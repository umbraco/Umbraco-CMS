import { html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbTreePickerModalData, UmbPickerModalValue, UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbTreeElement, UmbTreeItemModelBase, type UmbTreeSelectionConfiguration } from '@umbraco-cms/backoffice/tree';

@customElement('umb-tree-picker-modal')
export class UmbTreePickerModalElement<TreeItemType extends UmbTreeItemModelBase> extends UmbModalBaseElement<
	UmbTreePickerModalData<TreeItemType>,
	UmbPickerModalValue
> {
	@state()
	_selectionConfiguration: UmbTreeSelectionConfiguration = {
		multiple: false,
		selectable: true,
		selection: [],
	};

	connectedCallback() {
		super.connectedCallback();

		// TODO: We should make a nicer way to observe the value..
		if (this.modalContext) {
			this.observe(this.modalContext.value, (value) => {
				this._selectionConfiguration.selection = value?.selection ?? [];
			});
		}

		this._selectionConfiguration.multiple = this.data?.multiple ?? false;
	}

	#onSelectionChange(e: CustomEvent) {
		e.stopPropagation();
		const element = e.target as UmbTreeElement;
		this.value = { selection: element.getSelection() };
		this.dispatchEvent(new UmbSelectionChangeEvent());
	}

	render() {
		return html`
			<umb-body-layout headline="Select">
				<uui-box>
					<umb-tree
						?hide-tree-root=${this.data?.hideTreeRoot}
						alias=${ifDefined(this.data?.treeAlias)}
						@selection-change=${this.#onSelectionChange}
						.selectionConfiguration=${this._selectionConfiguration}
						.filter=${this.data?.filter}
						.selectableFilter=${this.data?.pickableFilter}></umb-tree>
				</uui-box>
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
					<uui-button
						label=${this.localize.term('general_choose')}
						look="primary"
						color="positive"
						@click=${this._submitModal}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbTreePickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-picker-modal': UmbTreePickerModalElement<UmbTreeItemModelBase>;
	}
}
