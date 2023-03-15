import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { UmbLitElement } from '@umbraco-cms/element';
import { RelationModel } from '@umbraco-cms/backend-api';
import { repeat } from 'lit-html/directives/repeat.js';

@customElement('umb-workspace-view-relation-type-relation')
export class UmbWorkspaceViewRelationTypeRelationElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}
		`,
	];

	@state()
	_relations: Array<RelationModel> = MockData;

	render() {
		return html`<uui-box headline="Relations">
			${repeat(
				this._relations,
				(relation) => relation.childId,
				(relation) => html`
					<div>
						<div>${relation.parentName}</div>
						<div>${relation.childName}</div>
					</div>
				`
			)}
		</uui-box>`;
	}
}

const MockData: Array<RelationModel> = [
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
