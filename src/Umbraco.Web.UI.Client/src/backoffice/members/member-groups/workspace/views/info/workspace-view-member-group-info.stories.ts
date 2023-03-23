import './workspace-view-member-group-info.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

//import { data } from '../../../../../core/mocks/data/data-type.data';
//import { UmbDataTypeContext } from '../../data-type.context';

import type { UmbWorkspaceViewMemberGroupInfoElement } from './workspace-view-member-group-info.element';

export default {
	title: 'Workspaces/Data Type/Views/Info',
	component: 'umb-workspace-view-member-group-info',
	id: 'umb-workspace-view-member-group-info',
	decorators: [
		(story) => {
			return html`TODO: make use of mocked workspace context??`;
			/*html` <umb-context-provider key="umbDataTypeContext" .value=${new UmbDataTypeWorkspaceContext(data[0])}>
				${story()}
			</umb-context-provider>`,*/
		},
	],
} as Meta;

export const AAAOverview: Story<UmbWorkspaceViewMemberGroupInfoElement> = () =>
	html` <umb-workspace-view-data-type-info></umb-workspace-view-data-type-info>`;
AAAOverview.storyName = 'Overview';
