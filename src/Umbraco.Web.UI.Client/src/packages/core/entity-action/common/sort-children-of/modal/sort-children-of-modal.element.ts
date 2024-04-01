import type { UmbSortChildrenOfModalData, UmbSortChildrenOfModalValue } from './sort-children-of-modal.token.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, customElement, css, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbTreeRepository, UmbUniqueTreeItemModel } from '@umbraco-cms/backoffice/tree';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';

const elementName = 'umb-sort-children-of-modal';

@customElement(elementName)
export class UmbSortChildrenOfModalElement extends UmbModalBaseElement<
	UmbSortChildrenOfModalData,
	UmbSortChildrenOfModalValue
> {
	@state()
	_items: Array<UmbUniqueTreeItemModel> = [];

	#sorter = new UmbSorterController<UmbUniqueTreeItemModel>(this, {
		getUniqueOfElement: (element) => {
			return element.dataset.unique;
		},
		getUniqueOfModel: (modelEntry) => {
			return modelEntry.unique;
		},
		identifier: 'Umb.SorterIdentifier.SortChildrenOfModal',
		itemSelector: 'uui-ref-node',
		containerSelector: 'uui-ref-list',
		onChange: (params) => {
			this._items = params.model;
			this.requestUpdate('_items');
		},
	});

	protected async firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): Promise<void> {
		super.firstUpdated(_changedProperties);

		if (!this.data?.unique === undefined) throw new Error('unique is required');
		if (!this.data?.itemRepositoryAlias) throw new Error('itemRepositoryAlias is required');

		const itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(this, this.data.itemRepositoryAlias);

		const treeRepository = await createExtensionApiByAlias<UmbTreeRepository<UmbUniqueTreeItemModel>>(
			this,
			this.data.treeRepositoryAlias,
		);

		const { data } = await treeRepository.requestTreeItemsOf({ parentUnique: this.data.unique, skip: 0, take: 100 });

		if (data) {
			this._items = data.items;
			this.#sorter.setModel(this._items);
		}
	}

	async #onSubmit(event: PointerEvent) {
		event?.stopPropagation();
		if (!this.data?.sortChildrenOfRepositoryAlias) throw new Error('sortChildrenOfRepositoryAlias is required');
		const sortChildrenOfRepository = await createExtensionApiByAlias<any>(
			this,
			this.data.sortChildrenOfRepositoryAlias,
		);

		debugger;

		/*
		const { error } = await sortChildrenOfRepository.sortChildrenOf({ unique: this.data.unique });
		if (!error) {
			console.log('Sorted');
		}
		*/
	}

	render() {
		return html`
			<umb-body-layout headline=${'Sort Children'}>
				<uui-box>
					<uui-ref-list>
						${repeat(
							this._items,
							(item) => item.unique,
							(item) => this.#renderItem(item),
						)}
					</uui-ref-list></uui-box
				>
				<uui-button slot="actions" label="Cancel" @click="${this._rejectModal}"></uui-button>
				<uui-button slot="actions" color="positive" look="primary" label="Sort"></uui-button>
			</umb-body-layout>
		`;
	}

	#renderItem(item: UmbUniqueTreeItemModel) {
		return html`<uui-ref-node .name=${item.name} data-unique=${item.unique}></uui-ref-node>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			#name {
				width: 100%;
			}
		`,
	];
}

export { UmbSortChildrenOfModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbSortChildrenOfModalElement;
	}
}
