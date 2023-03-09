import './workspace-view-relation-type-info.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

//import { data } from '../../../../../core/mocks/data/relation-type.data';
//import { UmbRelationTypeContext } from '../../relation-type.context';

import type { UmbWorkspaceViewRelationTypeInfoElement } from './workspace-view-relation-type-info.element';

export default {
	title: 'Workspaces/Relation Type/Views/Info',
	component: 'umb-workspace-view-relation-type-info',
	id: 'umb-workspace-view-relation-type-info',
	decorators: [
		(story) => {
			return html`TODO: make use of mocked workspace context??`;
			/*html` <umb-context-provider key="umbRelationTypeContext" .value=${new UmbRelationTypeWorkspaceContext(data[0])}>
				${story()}
			</umb-context-provider>`,*/
		},
	],
} as Meta;

export const AAAOverview: Story<UmbWorkspaceViewRelationTypeInfoElement> = () =>
	html` <umb-workspace-view-relation-type-info></umb-workspace-view-relation-type-info>`;
AAAOverview.storyName = 'Overview';
