import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';
//import { data } from '../../../../../core/mocks/data/dictionary.data';
import type { UmbWorkspaceViewDictionaryEditElement } from './workspace-view-dictionary-edit.element';
import './workspace-view-dictionary-edit.element';
//import { UmbWorkspaceDictionaryContext } from '../../workspace-dictionary.context';

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
