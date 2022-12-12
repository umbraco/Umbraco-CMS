import './editor-view-content-edit.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { data } from '../../../../../../core/mocks/data/node.data';
import { UmbNodeContext } from '../../node.context';

import type { UmbEditorViewContentEditElement } from './editor-view-content-edit.element';

export default {
	title: 'Editors/Shared/Node/Views/Edit',
	component: 'umb-editor-view-content-edit',
	id: 'umb-editor-view-content-edit',
	decorators: [
		(story) =>
			html` <umb-context-provider key="umbNodeContext" .value=${new UmbNodeContext(data[0])}>
				${story()}
			</umb-context-provider>`,
	],
} as Meta;

export const AAAOverview: Story<UmbEditorViewContentEditElement> = () =>
	html` <umb-editor-view-content-edit></umb-editor-view-content-edit>`;
AAAOverview.storyName = 'Overview';
