import { UMB_RELATION_TYPE_WORKSPACE_CONTEXT } from '../relation-type-workspace.context-token.js';
import type { UmbRelationTypeDetailModel } from '../../../types.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbRelationDetailModel } from '@umbraco-cms/backoffice/relations';
import { UmbRelationCollectionRepository } from '@umbraco-cms/backoffice/relations';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

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

	#skip = 0;
	#take = 50;

	#workspaceContext?: typeof UMB_RELATION_TYPE_WORKSPACE_CONTEXT.TYPE;
	#relationCollectionRepository = new UmbRelationCollectionRepository(this);

	constructor() {
		super();

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
		if (!this.#workspaceContext) {
			return;
		}

		/*
		const { data } = await this.#relationCollectionRepository.requestCollection({
			skip: this.#skip,
			take: this.#take,
		});

		if (data) {
			console.log(data);
		}
		*/
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
						value: relation.createDate,
					},
					{
						columnAlias: 'comment',
						value: relation.comment,
					},
				],
			};
		});
	}

	render() {
		return html`${this.#renderRelations()}${this.#renderDetails()}`;
	}

	#renderRelations() {
		return html` <umb-table
			.config=${this._tableConfig}
			.columns=${this._tableColumns}
			.items=${this._tableItems}></umb-table>`;
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

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: grid;
				gap: var(--uui-size-layout-1);
				padding: var(--uui-size-layout-1);
				grid-template-columns: 1fr 350px;
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
