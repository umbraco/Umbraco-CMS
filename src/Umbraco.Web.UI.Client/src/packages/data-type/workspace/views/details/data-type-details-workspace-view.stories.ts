import type { UmbDataTypeDetailsWorkspaceViewEditElement } from './data-type-details-workspace-view.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

//import { data } from '../../../../../core/mocks/data/data-type.data.js';

import './data-type-details-workspace-view.element.js';
//import { UmbDataTypeWorkspaceContext } from '../../workspace-data-type.context.js';

export default {
	title: 'Workspaces/Data Type/Views/Edit',
	component: 'umb-data-type-workspace-view-edit',
	id: 'umb-data-type-workspace-view-edit',
	decorators: [
		() => {
			return html`TODO: make use of mocked workspace context??`;
			/*html` <umb-context-provider key="umbDataTypeContext" .value=${new UmbDataTypeWorkspaceContext(data[0])}>
				${story()}
			</umb-context-provider>`,*/
		},
	],
} as Meta;

export const AAAOverview: StoryFn<UmbDataTypeDetailsWorkspaceViewEditElement> = () =>
	html` <umb-data-type-workspace-view-edit></umb-data-type-workspace-view-edit>`;
AAAOverview.storyName = 'Overview';
