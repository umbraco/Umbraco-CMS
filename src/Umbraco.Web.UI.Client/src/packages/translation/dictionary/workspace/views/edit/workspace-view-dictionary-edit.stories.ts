import { Meta, Story } from '@storybook/web-components';
import type { UmbWorkspaceViewDictionaryEditElement } from './workspace-view-dictionary-edit.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';
//import { data } from '../../../../../core/mocks/data/dictionary.data.js';
import './workspace-view-dictionary-edit.element.js';
//import { UmbWorkspaceDictionaryContext } from '../../workspace-dictionary.context.js';

export default {
	title: 'Workspaces/Dictionary/Views/Edit',
	component: 'umb-workspace-view-dictionary-edit',
	id: 'umb-workspace-view-dictionary-edit',
	decorators: [
		(story) => {
			return html`TODO: make use of mocked workspace context??`;
			/*html` <umb-context-provider key="umbDataTypeContext" .value=${new UmbWorkspaceDictionaryContext(data[0])}>
				${story()}
			</umb-context-provider>`,*/
		},
	],
} as Meta;

export const AAAOverview: Story<UmbWorkspaceViewDictionaryEditElement> = () =>
	html` <umb-workspace-view-dictionary-edit></umb-workspace-view-dictionary-edit>`;

AAAOverview.storyName = 'Overview';
