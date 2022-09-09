import './editor-view-node-info.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { data } from '../../../../../../mocks/data/node.data';
import { UmbNodeContext } from '../../node.context';

import type { UmbEditorViewNodeInfoElement } from './editor-view-node-info.element';

export default {
	title: 'Editors/Shared/Node/Views/Info',
	component: 'umb-editor-view-node-info',
	id: 'umb-editor-view-node-info',
	decorators: [
		(story) =>
			html` <umb-context-provider key="umbNodeContext" .value=${new UmbNodeContext(data[0])}>
				${story()}
			</umb-context-provider>`,
	],
} as Meta;

export const AAAOverview: Story<UmbEditorViewNodeInfoElement> = () =>
	html` <umb-editor-view-node-info></umb-editor-view-node-info>`;
AAAOverview.storyName = 'Overview';
