import './document-info-workspace-view.element.js';

import { Meta, Story } from '@storybook/web-components';
import type { UmbDocumentInfoWorkspaceViewElement } from './document-info-workspace-view.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

// import { data } from '../../../../../../core/mocks/data/document.data.js';
// import { UmbNodeContext } from '../../node.context.js';


export default {
	title: 'Workspaces/Documents/Views/Info',
	component: 'umb-document-info-workspace-view',
	id: 'umb-document-info-workspace-view',
	decorators: [
		(story) => {
			return html`TODO: make use of mocked workspace context??`;
			/*html` <umb-context-provider key="workspaceContext" .value=${new UmbDataTypeWorkspaceContext(data[0])}>
				${story()}
			</umb-context-provider>`,*/
		},
	],
} as Meta;

export const AAAOverview: Story<UmbDocumentInfoWorkspaceViewElement> = () =>
	html` <umb-document-info-workspace-view></umb-document-info-workspace-view>`;
AAAOverview.storyName = 'Overview';
