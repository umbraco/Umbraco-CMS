import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

//import { data } from '../../../../../core/mocks/data/relation-type.data';

import type { UmbRelationTypeWorkspaceViewRelationTypeElement } from './relation-type-workspace-view-relation-type.element';

import './relation-type-workspace-view-relation-type.element';
//import { UmbRelationTypeWorkspaceContext } from '../../workspace-relation-type.context';

export default {
	title: 'Workspaces/Relation Type/Views/RelationType',
	component: 'umb-relation-type-workspace-view-relation-type',
	id: 'umb-relation-type-workspace-view-relation-type',
	decorators: [
		(story) => {
			return html`TODO: make use of mocked workspace context??`;
			/*html` <umb-context-provider key="umbRelationTypeContext" .value=${new UmbRelationTypeWorkspaceContext(data[0])}>
				${story()}
			</umb-context-provider>`,*/
		},
	],
} as Meta;

export const AAAOverview: Story<UmbRelationTypeWorkspaceViewRelationTypeElement> = () =>
	html` <umb-relation-type-workspace-view-relation-type></umb-relation-type-workspace-view-relation-type>`;
AAAOverview.storyName = 'Overview';
