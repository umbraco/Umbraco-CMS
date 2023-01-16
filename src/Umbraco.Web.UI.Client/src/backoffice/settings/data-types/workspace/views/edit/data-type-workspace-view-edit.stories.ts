import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

//import { data } from '../../../../../core/mocks/data/data-type.data';

import type { UmbDataTypeWorkspaceViewEditElement } from './data-type-workspace-view-edit.element';

import './data-type-workspace-view-edit.element';
//import { UmbWorkspaceDataTypeContext } from '../../workspace-data-type.context';

export default {
	title: 'Workspaces/Data Type/Views/Edit',
	component: 'umb-data-type-workspace-view-edit',
	id: 'umb-data-type-workspace-view-edit',
	decorators: [
		(story) => {
			return html`TODO: make use of mocked workspace context??`;
			/*html` <umb-context-provider key="umbDataTypeContext" .value=${new UmbWorkspaceDataTypeContext(data[0])}>
				${story()}
			</umb-context-provider>`,*/
		}
	],
} as Meta;

export const AAAOverview: Story<UmbDataTypeWorkspaceViewEditElement> = () =>
	html` <umb-data-type-workspace-view-edit></umb-data-type-workspace-view-edit>`;
AAAOverview.storyName = 'Overview';
