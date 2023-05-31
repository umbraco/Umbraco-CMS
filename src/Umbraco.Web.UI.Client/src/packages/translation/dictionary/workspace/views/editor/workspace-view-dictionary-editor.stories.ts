import { Meta, Story } from '@storybook/web-components';
import type { UmbWorkspaceViewDictionaryEditorElement } from './workspace-view-dictionary-editor.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';
//import { data } from '../../../../../core/mocks/data/dictionary.data.js';
import './workspace-view-dictionary-edit.element.js';
//import { UmbWorkspaceDictionaryContext } from '../../workspace-dictionary.context.js';

export default {
	title: 'Workspaces/Dictionary/Views/Edit',
	component: 'umb-workspace-view-dictionary-editor',
	id: 'umb-workspace-view-dictionary-editor',
	decorators: [
		(story) => {
			return html`TODO: make use of mocked workspace context??`;
			/*html` <umb-context-provider key="umbDataTypeContext" .value=${new UmbWorkspaceDictionaryContext(data[0])}>
				${story()}
			</umb-context-provider>`,*/
		},
	],
} as Meta;

export const AAAOverview: Story<UmbWorkspaceViewDictionaryEditorElement> = () =>
	html` <umb-workspace-view-dictionary-editor></umb-workspace-view-dictionary-editor>`;

AAAOverview.storyName = 'Overview';
