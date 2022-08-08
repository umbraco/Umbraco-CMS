import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { UmbNodeContext } from '../../node.context';
import { UmbEditorViewNodeEditElement } from './editor-view-node-edit.element';
import './editor-view-node-edit.element';
import { data } from '../../../../../mocks/data/content.data';

export default {
	title: 'Editors/Node/Views/Edit',
	component: 'umb-editor-view-node-edit',
	id: 'umb-editor-view-node-edit',
	decorators: [
		(story) =>
			html` <umb-context-provider key="umbNodeContext" .value=${new UmbNodeContext(data[0])}>
				${story()}
			</umb-context-provider>`,
	],
} as Meta;

export const AAAOverview: Story<UmbEditorViewNodeEditElement> = () =>
	html` <umb-editor-view-node-edit></umb-editor-view-node-edit>`;
AAAOverview.storyName = 'Overview';
