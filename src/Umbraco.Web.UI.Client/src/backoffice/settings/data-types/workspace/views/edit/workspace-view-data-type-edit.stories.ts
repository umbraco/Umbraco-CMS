import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

//import { data } from '../../../../../core/mocks/data/data-type.data';

import type { UmbWorkspaceViewDataTypeEditElement } from './workspace-view-data-type-edit.element';

import './workspace-view-data-type-edit.element';
//import { UmbWorkspaceDataTypeContext } from '../../workspace-data-type.context';

export default {
	title: 'Workspaces/Data Type/Views/Edit',
	component: 'umb-workspace-view-data-type-edit',
	id: 'umb-workspace-view-data-type-edit',
	decorators: [
		(story) => {
			return html`TODO: make use of mocked workspace context??`;
			/*html` <umb-context-provider key="umbDataTypeContext" .value=${new UmbWorkspaceDataTypeContext(data[0])}>
				${story()}
			</umb-context-provider>`,*/
		}
	],
} as Meta;

export const AAAOverview: Story<UmbWorkspaceViewDataTypeEditElement> = () =>
	html` <umb-workspace-view-data-type-edit></umb-workspace-view-data-type-edit>`;
AAAOverview.storyName = 'Overview';
