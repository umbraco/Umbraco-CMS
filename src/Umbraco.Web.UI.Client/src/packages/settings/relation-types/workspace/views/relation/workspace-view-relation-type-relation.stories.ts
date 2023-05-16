import './workspace-view-relation-type-relation.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

//import { data } from '../../../../../core/mocks/data/relation-type.data';
//import { UmbRelationTypeContext } from '../../relation-type.context';

import type { UmbWorkspaceViewRelationTypeRelationElement } from './workspace-view-relation-type-relation.element';

export default {
	title: 'Workspaces/Relation Type/Views/Relation',
	component: 'umb-workspace-view-relation-type-relation',
	id: 'umb-workspace-view-relation-type-relation',
	decorators: [
		(story) => {
			return html`TODO: make use of mocked workspace context??`;
			/*html` <umb-context-provider key="umbRelationTypeContext" .value=${new UmbRelationTypeWorkspaceContext(data[0])}>
				${story()}
			</umb-context-provider>`,*/
		},
	],
} as Meta;

export const AAAOverview: Story<UmbWorkspaceViewRelationTypeRelationElement> = () =>
	html` <umb-workspace-view-relation-type-relation></umb-workspace-view-relation-type-relation>`;
AAAOverview.storyName = 'Overview';
