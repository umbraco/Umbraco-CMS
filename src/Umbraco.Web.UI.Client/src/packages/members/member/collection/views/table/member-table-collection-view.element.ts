import type { UmbMemberCollectionModel } from '../../types.js';
import { UMB_MEMBER_COLLECTION_CONTEXT } from '../../member-collection.context-token.js';
import type { UmbMemberCollectionContext } from '../../member-collection.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-member-table-collection-view')
export class UmbMemberTableCollectionViewElement extends UmbLitElement {
	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: this.localize.term('general_name'),
			alias: 'memberName',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	#collectionContext?: UmbMemberCollectionContext;

	constructor() {
		super();

		this.consumeContext(UMB_MEMBER_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeCollectionItems();
		});
	}

	#observeCollectionItems() {
		if (!this.#collectionContext) return;
		this.observe(this.#collectionContext.items, (items) => this.#createTableItems(items), 'umbCollectionItemsObserver');
	}

	#createTableItems(members: Array<UmbMemberCollectionModel>) {
		this._tableItems = members.map((member) => {
			// TODO: get correct variant name
			const name = member.variants[0].name;

			return {
				id: member.unique,
				icon: 'icon-user',
				data: [
					{
						columnAlias: 'memberName',
						value: html`<a href=${'section/member-management/workspace/member/edit/' + member.unique}>${name}</a>`,
					},
				],
			};
		});
	}

	render() {
		return html`
			<umb-table .config=${this._tableConfig} .columns=${this._tableColumns} .items=${this._tableItems}></umb-table>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}
		`,
	];
}

export default UmbMemberTableCollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-table-collection-view': UmbMemberTableCollectionViewElement;
	}
}
