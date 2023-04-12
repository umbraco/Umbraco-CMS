import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

//import { data } from '../../../../../core/mocks/data/data-type.data';

import type { UmbDataTypeDetailsWorkspaceViewEditElement } from './data-type-details-workspace-view.element';

import './data-type-details-workspace-view.element';
//import { UmbDataTypeWorkspaceContext } from '../../workspace-data-type.context';

export default {
	title: 'Workspaces/Data Type/Views/Edit',
	component: 'umb-data-type-workspace-view-edit',
	id: 'umb-data-type-workspace-view-edit',
	decorators: [
		(story) => {
			return html`TODO: make use of mocked workspace context??`;
			/*html` <umb-context-provider key="umbDataTypeContext" .value=${new UmbDataTypeWorkspaceContext(data[0])}>
				${story()}
			</umb-context-provider>`,*/
		},
	],
} as Meta;

export const AAAOverview: Story<UmbDataTypeDetailsWorkspaceViewEditElement> = () =>
	html` <umb-data-type-workspace-view-edit></umb-data-type-workspace-view-edit>`;
AAAOverview.storyName = 'Overview';
