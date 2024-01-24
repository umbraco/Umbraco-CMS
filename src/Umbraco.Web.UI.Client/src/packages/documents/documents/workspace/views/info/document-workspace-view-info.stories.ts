import './document-workspace-view-info.element.js';

import { Meta, Story } from '@storybook/web-components';
import type { UmbDocumentWorkspaceViewInfoElement } from './document-workspace-view-info.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

// import { data } from '../../../../../../core/mocks/data/document.data.js';
// import { UmbNodeContext } from '../../node.context.js';

export default {
	title: 'Workspaces/Documents/Views/Info',
	component: 'umb-document-workspace-view-info',
	id: 'umb-document-workspace-view-info',
	decorators: [
		(story) => {
			return html`TODO: make use of mocked workspace context??`;
			/*html` <umb-context-provider key="workspaceContext" .value=${new UmbDataTypeWorkspaceContext(data[0])}>
				${story()}
			</umb-context-provider>`,*/
		},
	],
} as Meta;

export const AAAOverview: Story<UmbDocumentWorkspaceViewInfoElement> = () =>
	html` <umb-document-workspace-view-info></umb-document-workspace-view-info>`;
AAAOverview.storyName = 'Overview';
