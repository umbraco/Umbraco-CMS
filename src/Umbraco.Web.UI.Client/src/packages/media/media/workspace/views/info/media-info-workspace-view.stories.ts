import './media-info-workspace-view.element.js';

import { Meta, Story } from '@storybook/web-components';
import type { UmbMediaInfoWorkspaceViewElement } from './media-info-workspace-view.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

// import { data } from '../../../../../../core/mocks/data/media.data.js';
// import { UmbNodeContext } from '../../node.context.js';

export default {
	title: 'Workspaces/Media/Views/Info',
	component: 'umb-media-info-workspace-view',
	id: 'umb-media-info-workspace-view',
	decorators: [
		(story) => {
			return html`TODO: make use of mocked workspace context??`;
			/*html` <umb-context-provider key="workspaceContext" .value=${new UmbDataTypeWorkspaceContext(data[0])}>
				${story()}
			</umb-context-provider>`,*/
		},
	],
} as Meta;

export const AAAOverview: Story<UmbMediaInfoWorkspaceViewElement> = () =>
	html` <umb-media-info-workspace-view></umb-media-info-workspace-view>`;
AAAOverview.storyName = 'Overview';
