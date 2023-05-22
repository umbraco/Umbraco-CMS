import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbTableColumn, UmbTableConfig } from '@umbraco-cms/backoffice/components';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { RelationResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbWorkspaceEditorViewExtensionElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-workspace-view-relation-type-relation')
export class UmbWorkspaceViewRelationTypeRelationElement
	extends UmbLitElement
	implements UmbWorkspaceEditorViewExtensionElement
{
	//TODO Use real data
	@state()
	_relations: Array<RelationResponseModel> = MockData;

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

	private get _tableItems() {
		return this._relations.map((relation) => {
			return {
				key: relation.parentId + '-' + relation.childId,
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
		UUITextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}
		`,
	];
}

const MockData: Array<RelationResponseModel> = [
	{
		parentId: 1,
		parentName: 'Parent 1',
		childId: 2,
		childName: 'Child 1',
		createDate: '2021-01-01',
		comment: 'Comment 1',
	},
	{
		parentId: 1,
		parentName: 'Parent 1',
		childId: 3,
		childName: 'Child 2',
		createDate: '2021-01-01',
		comment: 'Comment 2',
	},
	{
		parentId: 1,
		parentName: 'Parent 1',
		childId: 4,
		childName: 'Child 3',
		createDate: '2021-01-01',
		comment: 'Comment 3',
	},
	{
		parentId: 1,
		parentName: 'Parent 1',
		childId: 5,
		childName: 'Child 4',
		createDate: '2021-01-01',
		comment: 'Comment 4',
	},
	{
		parentId: 1,
		parentName: 'Parent 1',
		childId: 6,
		childName: 'Child 5',
		createDate: '2021-01-01',
		comment: 'Comment 5',
	},
	{
		parentId: 1,
		parentName: 'Parent 1',
		childId: 7,
		childName: 'Child 6',
		createDate: '2021-01-01',
		comment: 'Comment 6',
	},
	{
		parentId: 1,
		parentName: 'Parent 1',
		childId: 8,
		childName: 'Child 7',
		createDate: '2021-01-01',
		comment: 'Comment 7',
	},
	{
		parentId: 1,
		parentName: 'Parent 1',
		childId: 9,
		childName: 'Child 8',
		createDate: '2021-01-01',
		comment: 'Comment 8',
	},
];

export default UmbWorkspaceViewRelationTypeRelationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-relation-type-relation': UmbWorkspaceViewRelationTypeRelationElement;
	}
}
