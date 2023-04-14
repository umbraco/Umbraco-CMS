import './document-type-workspace-view-design.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

//import { data } from '../../../../../core/mocks/data/document-type.data';
//import { UmbDocumentTypeContext } from '../../document-type.context';

import type { UmbDocumentTypeWorkspaceViewDesignElement } from './document-type-workspace-view-design.element';

export default {
	title: 'Workspaces/Document Type/Views/Design',
	component: 'umb-document-type-workspace-view-design',
	id: 'umb-document-type-workspace-view-design',
	decorators: [
		(story) => {
			return html`TODO: make use of mocked workspace context??`;
			/*html` <umb-context-provider key="umbDocumentTypeContext" .value=${new UmbDataTypeWorkspaceContext(data[0])}>
				${story()}
			</umb-context-provider>`,*/
		},
	],
} as Meta;

export const AAAOverview: Story<UmbDocumentTypeWorkspaceViewDesignElement> = () =>
	html` <umb-document-type-workspace-view-design></umb-document-type-workspace-view-design>`;
AAAOverview.storyName = 'Overview';
