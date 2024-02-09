import { UMB_RELATION_TYPE_WORKSPACE_CONTEXT } from '../../relation-type-workspace.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { RelationResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-workspace-view-relation-type-relation')
export class UmbWorkspaceViewRelationTypeRelationElement extends UmbLitElement implements UmbWorkspaceViewElement {
	//TODO Use real data
	@state()
	_relations: Array<RelationResponseModel> = [];

	#workspaceContext?: typeof UMB_RELATION_TYPE_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_RELATION_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#getRelations();
		});
	}

	async #getRelations() {
		if (!this.#workspaceContext) {
			return;
		}

		const response = await this.#workspaceContext.getRelations();
		this._relations = response.data?.items ?? [];
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
				id: relation.parentId + '-' + relation.childId, // Add the missing id property
				data: [
					{
						columnAlias: 'parent',
						value: relation.parentName,
					},
					{
						columnAlias: 'child',
						value: relation.childName,
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
		return html`<uui-box headline="Relations">
			<umb-table .config=${this._tableConfig} .columns=${this._tableColumns} .items=${this._tableItems}></umb-table>
		</uui-box>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbWorkspaceViewRelationTypeRelationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-relation-type-relation': UmbWorkspaceViewRelationTypeRelationElement;
	}
}
