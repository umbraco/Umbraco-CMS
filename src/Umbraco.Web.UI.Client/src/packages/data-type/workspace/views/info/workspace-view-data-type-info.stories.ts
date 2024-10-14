import './workspace-view-data-type-info.element.js';

import type { UmbWorkspaceViewDataTypeInfoElement } from './workspace-view-data-type-info.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

//import { data } from '../../../../../core/mocks/data/data-type.data.js';
//import { UmbDataTypeContext } from '../../data-type.context.js';

export default {
	title: 'Workspaces/Data Type/Views/Info',
	component: 'umb-workspace-view-data-type-info',
	id: 'umb-workspace-view-data-type-info',
	decorators: [
		() => {
			return html`TODO: make use of mocked workspace context??`;
			/*html` <umb-context-provider key="umbDataTypeContext" .value=${new UmbDataTypeWorkspaceContext(data[0])}>
				${story()}
			</umb-context-provider>`,*/
		},
	],
} as Meta;

export const AAAOverview: StoryFn<UmbWorkspaceViewDataTypeInfoElement> = () =>
	html` <umb-workspace-view-data-type-info></umb-workspace-view-data-type-info>`;
AAAOverview.storyName = 'Overview';
