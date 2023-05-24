import './media-edit-workspace-view.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

// import { data } from '../../../../../../core/mocks/data/media.data.js';
// import { UmbNodeContext } from '../../node.context.js';

import type { UmbMediaEditWorkspaceViewElement } from './media-edit-workspace-view.element.js';

export default {
	title: 'Workspaces/Media/Views/Edit',
	component: 'umb-media-edit-workspace-view',
	id: 'umb-media-edit-workspace-view',
	decorators: [
		(story) => {
			return html`TODO: make use of mocked workspace context??`;
			/*html` <umb-context-provider key="workspaceContext" .value=${new UmbDataTypeWorkspaceContext(data[0])}>
				${story()}
			</umb-context-provider>`,*/
		},
	],
} as Meta;

export const AAAOverview: Story<UmbMediaEditWorkspaceViewElement> = () =>
	html` <umb-media-edit-workspace-view></umb-media-edit-workspace-view>`;
AAAOverview.storyName = 'Overview';
