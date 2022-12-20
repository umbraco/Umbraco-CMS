import './workspace-view-content-edit.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

//import { data } from '../../../../../../core/mocks/data/document.data';
//import { UmbNodeContext } from '../../node.context';

import type { UmbWorkspaceViewContentEditElement } from './workspace-view-content-edit.element';

export default {
	title: 'Workspaces/Shared/Node/Views/Edit',
	component: 'umb-workspace-view-content-edit',
	id: 'umb-workspace-view-content-edit',
	decorators: [
		(story) => {
			return html`TODO: make use of mocked workspace context??`;
			/*html` <umb-context-provider key="workspaceContext" .value=${new UmbWorkspaceDataTypeContext(data[0])}>
				${story()}
			</umb-context-provider>`,*/
		}
	],
} as Meta;

export const AAAOverview: Story<UmbWorkspaceViewContentEditElement> = () =>
	html` <umb-workspace-view-content-edit></umb-workspace-view-content-edit>`;
AAAOverview.storyName = 'Overview';
