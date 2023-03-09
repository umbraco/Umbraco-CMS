import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

//import { data } from '../../../../../core/mocks/data/relation-type.data';

import type { UmbRelationTypeWorkspaceViewEditElement } from './relation-type-workspace-view-edit.element';

import './relation-type-workspace-view-edit.element';
//import { UmbRelationTypeWorkspaceContext } from '../../workspace-relation-type.context';

export default {
	title: 'Workspaces/Data Type/Views/Edit',
	component: 'umb-relation-type-workspace-view-edit',
	id: 'umb-relation-type-workspace-view-edit',
	decorators: [
		(story) => {
			return html`TODO: make use of mocked workspace context??`;
			/*html` <umb-context-provider key="umbRelationTypeContext" .value=${new UmbRelationTypeWorkspaceContext(data[0])}>
				${story()}
			</umb-context-provider>`,*/
		},
	],
} as Meta;

export const AAAOverview: Story<UmbRelationTypeWorkspaceViewEditElement> = () =>
	html` <umb-relation-type-workspace-view-edit></umb-relation-type-workspace-view-edit>`;
AAAOverview.storyName = 'Overview';
