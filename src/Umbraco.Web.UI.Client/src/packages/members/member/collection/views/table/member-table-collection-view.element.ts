import type { UmbMemberCollectionModel } from '../../types.js';
import { UMB_MEMBER_COLLECTION_CONTEXT } from '../../member-collection.context-token.js';
import type { UmbMemberCollectionContext } from '../../member-collection.context.js';
import { UmbMemberKind } from '../../../utils/index.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbMemberTypeItemRepository } from '@umbraco-cms/backoffice/member-type';

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
		{
			name: this.localize.term('general_username'),
			alias: 'memberUsername',
		},
		{
			name: this.localize.term('general_email'),
			alias: 'memberEmail',
		},
		{
			name: this.localize.term('content_membertype'),
			alias: 'memberType',
		},
		{
			name: this.localize.term('member_kind'),
			alias: 'memberKind',
		},
		{
			name: '',
			alias: 'entityActions',
			align: 'right',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	#collectionContext?: UmbMemberCollectionContext;
	#memberTypeItemRepository = new UmbMemberTypeItemRepository(this);

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

	async #createTableItems(members: Array<UmbMemberCollectionModel>) {
		const memberTypeUniques = members.map((member) => member.memberType.unique);
		const { data: memberTypes } = await this.#memberTypeItemRepository.requestItems(memberTypeUniques);

		this._tableItems = members.map((member) => {
			// TODO: get correct variant name
			const name = member.variants[0].name;
			const kind =
				member.kind === UmbMemberKind.API
					? this.localize.term('member_memberKindApi')
					: this.localize.term('member_memberKindDefault');

			const memberType = memberTypes?.find((type) => type.unique === member.memberType.unique);

			return {
				id: member.unique,
				icon: memberType?.icon,
				data: [
					{
						columnAlias: 'memberName',
						value: html`<a href=${'section/member-management/workspace/member/edit/' + member.unique}>${name}</a>`,
					},
					{
						columnAlias: 'memberUsername',
						value: member.username,
					},
					{
						columnAlias: 'memberEmail',
						value: member.email,
					},
					{
						columnAlias: 'memberType',
						value: memberType?.name,
					},
					{
						columnAlias: 'memberKind',
						value: kind,
					},
					{
						columnAlias: 'entityActions',
						value: html`<umb-entity-actions-table-column-view
							.value=${{
								entityType: member.entityType,
								unique: member.unique,
								name: member.variants[0].name,
							}}></umb-entity-actions-table-column-view>`,
					},
				],
			};
		});
	}

	override render() {
		return html`
			<umb-table .config=${this._tableConfig} .columns=${this._tableColumns} .items=${this._tableItems}></umb-table>
		`;
	}

	static override styles = [
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
