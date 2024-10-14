import { UMB_RELATION_TYPE_WORKSPACE_CONTEXT } from '../relation-type-workspace.context-token.js';
import type { UmbRelationTypeDetailModel } from '../../../types.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import type { UmbRelationDetailModel } from '@umbraco-cms/backoffice/relations';
import { UmbRelationCollectionRepository } from '@umbraco-cms/backoffice/relations';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { UmbPaginationManager } from '@umbraco-cms/backoffice/utils';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-relation-type-detail-workspace-view')
export class UmbRelationTypeDetailWorkspaceViewElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	_relations: Array<UmbRelationDetailModel> = [];

	@state()
	_parent?: UmbRelationTypeDetailModel['parent'];

	@state()
	_child?: UmbRelationTypeDetailModel['child'];

	@state()
	_isBidirectional?: UmbRelationTypeDetailModel['isBidirectional'];

	@state()
	_isDependency?: UmbRelationTypeDetailModel['isDependency'];

	@state()
	_currentPageNumber = 1;

	@state()
	_totalPages = 1;

	@state()
	_loading = false;

	#workspaceContext?: typeof UMB_RELATION_TYPE_WORKSPACE_CONTEXT.TYPE;
	#relationCollectionRepository = new UmbRelationCollectionRepository(this);
	#paginationManager = new UmbPaginationManager();

	constructor() {
		super();

		this.#paginationManager.setPageSize(50);

		this.observe(this.#paginationManager.currentPage, (number) => (this._currentPageNumber = number));
		this.observe(this.#paginationManager.totalPages, (number) => (this._totalPages = number));

		this.consumeContext(UMB_RELATION_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#requestRelations();
			this.#observeDetails();
		});
	}

	#observeDetails() {
		if (!this.#workspaceContext) return;

		this.observe(
			observeMultiple([
				this.#workspaceContext.parent,
				this.#workspaceContext.child,
				this.#workspaceContext.isBidirectional,
				this.#workspaceContext.isDependency,
			]),
			([parent, child, isBidirectional, isDependency]) => {
				this._parent = parent;
				this._child = child;
				this._isBidirectional = isBidirectional;
				this._isDependency = isDependency;
			},
		);
	}

	async #requestRelations() {
		this._loading = true;

		const relationTypeUnique = this.#workspaceContext?.getUnique();
		if (!relationTypeUnique) throw new Error('Relation type unique is required');

		const { data } = await this.#relationCollectionRepository.requestCollection({
			relationType: {
				unique: relationTypeUnique,
			},
			skip: this.#paginationManager.getSkip(),
			take: this.#paginationManager.getPageSize(),
		});

		if (data) {
			this._relations = data.items;
			this.#paginationManager.setTotalItems(data.total);
			this._loading = false;
		}
	}

	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
		hideIcon: true,
	};

	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: 'Parent',
			alias: 'parent',
		},
		{
			name: 'Child',
			alias: 'child',
		},
		{
			name: 'Created',
			alias: 'created',
		},
		{
			name: 'Comment',
			alias: 'comment',
		},
	];

	private get _tableItems(): UmbTableItem[] {
		if (this._relations.length === 0) {
			return [
				{
					id: 'no-relations',
					data: [
						{
							columnAlias: 'parent',
							value: 'No relations found',
						},
					],
				},
			];
		}

		return this._relations.map((relation) => {
			return {
				id: relation.unique,
				data: [
					{
						columnAlias: 'parent',
						value: relation.parent.name,
					},
					{
						columnAlias: 'child',
						value: relation.child.name,
					},
					{
						columnAlias: 'created',
						value: this.localize.date(relation.createDate, { dateStyle: 'long', timeStyle: 'medium' }),
					},
					{
						columnAlias: 'comment',
						value: relation.comment,
					},
				],
			};
		});
	}

	#onPageChange(event: UUIPaginationEvent) {
		this.#paginationManager.setCurrentPageNumber(event.target?.current);
		this.#requestRelations();
	}

	override render() {
		return html`${this.#renderRelations()}${this.#renderDetails()}`;
	}

	#renderRelations() {
		if (this._loading) return html`<uui-loader></uui-loader>`;
		return html`
			<div>
				<umb-table .config=${this._tableConfig} .columns=${this._tableColumns} .items=${this._tableItems}></umb-table>

				${this._totalPages > 1
					? html`
							<uui-pagination
								.current=${this._currentPageNumber}
								.total=${this._totalPages}
								@change=${this.#onPageChange}></uui-pagination>
						`
					: nothing}
			</div>
		`;
	}

	#renderDetails() {
		return html`<uui-box>
			<umb-property-layout label="Parent Type" orientation="vertical"
				><div slot="editor">${this._parent?.objectType.name}</div>
			</umb-property-layout>
			<umb-property-layout label="Child Type" orientation="vertical">
				<div slot="editor">${this._child?.objectType.name}</div>
			</umb-property-layout>
			<umb-property-layout label="Bidirectional" orientation="vertical">
				<div slot="editor">${this._isBidirectional}</div>
			</umb-property-layout>
			<umb-property-layout label="Dependency" orientation="vertical">
				<div slot="editor">${this._isDependency}</div>
			</umb-property-layout>
		</uui-box>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: grid;
				gap: var(--uui-size-layout-1);
				padding: var(--uui-size-layout-1);
				grid-template-columns: 1fr 350px;
			}

			uui-loader {
				text-align: center;
			}

			uui-pagination {
				margin-top: var(--uui-size-layout-1);
				display: block;
			}
		`,
	];
}

export default UmbRelationTypeDetailWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-relation-type-detail-workspace-view': UmbRelationTypeDetailWorkspaceViewElement;
	}
}
