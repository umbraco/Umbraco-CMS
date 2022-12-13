import './editor-view-content-info.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { data } from '../../../../../../core/mocks/data/document.data';
import { UmbNodeContext } from '../../node.context';

import type { UmbEditorViewContentInfoElement } from './editor-view-content-info.element';

export default {
	title: 'Editors/Shared/Node/Views/Info',
	component: 'umb-editor-view-content-info',
	id: 'umb-editor-view-content-info',
	decorators: [
		(story) =>
			html` <umb-context-provider key="umbNodeContext" .value=${new UmbNodeContext(data[0])}>
				${story()}
			</umb-context-provider>`,
	],
} as Meta;

export const AAAOverview: Story<UmbEditorViewContentInfoElement> = () =>
	html` <umb-editor-view-content-info></umb-editor-view-content-info>`;
AAAOverview.storyName = 'Overview';
