import { UMB_RELATION_TYPE_WORKSPACE_CONTEXT } from '../../relation-type-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { RelationResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';
import UmbRelationDetailRepository from 'src/packages/relations/relations/repository/relation.repository.js';

@customElement('umb-workspace-view-relation-type-relation')
export class UmbWorkspaceViewRelationTypeRelationElement extends UmbLitElement implements UmbWorkspaceViewElement {
	//TODO Use real data
	@state()
	_relations: Array<RelationResponseModel> = [];

	#workspaceContext?: typeof UMB_RELATION_TYPE_WORKSPACE_CONTEXT.TYPE;
	#relationDetailRepository = new UmbRelationDetailRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_RELATION_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#requestRelations();
		});
	}

	async #requestRelations() {
		if (!this.#workspaceContext) {
			return;
		}

		const { data, error } = await this.#relationDetailRepository.read(this.#workspaceContext.unique);
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
				id: relation.unique + '-' + relation.childId, // Add the missing id property
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
		return html`<uui-box>Details</uui-box>`;
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

export default UmbWorkspaceViewRelationTypeRelationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-relation-type-relation': UmbWorkspaceViewRelationTypeRelationElement;
	}
}
